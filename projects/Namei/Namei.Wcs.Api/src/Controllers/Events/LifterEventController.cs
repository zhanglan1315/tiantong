using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Namei.Wcs.Api
{
  public class LifterEventController: BaseController
  {
    public const string Group = "lifter";

    private ICapPublisher _cap;

    private LifterServiceManager _lifters;

    private WmsService _wms;

    private DomainContext _domain;

    public LifterEventController(
      ICapPublisher cap,
      DomainContext domain,
      LifterServiceManager lifters,
      WmsService wms
    ) {
      _cap = cap;
      _domain = domain;
      _lifters = lifters;
      _wms = wms;
    }

    private LifterRuntimeTask SaveRuntimeTask(LifterTask task)
    {
      var runtimeTask = _domain.LifterRuntimeTasks.Find(task.Barcode);

      if (runtimeTask == null) {
        runtimeTask = LifterRuntimeTask.From(task);
        _domain.Add(runtimeTask);
      } else {
        runtimeTask.UseLifterTaskId(task.Id);
      }

      _domain.SaveChanges();

      return runtimeTask;
    }

    [CapSubscribe(LifterTaskReceived.Message, Group = Group)]
    public void HandleTaskReceived(LifterTaskReceived param)
    {
      var task = LifterTask.From(
        lifterId: param.LifterId,
        floor: param.Floor,
        destination: param.Destination,
        barcode: param.Barcode,
        taskCode: param.TaskCode,
        operatr: param.Operator
      );

      _domain.Add(task);

      _domain.UseTransaction(() => {
        _domain.SaveChanges();
        SaveRuntimeTask(task);
        _cap.Publish(LifterTaskCreated.Message, LifterTaskCreated.From(task));
      },
      error => {
        _cap.Publish(LifterOperationError.Message, LifterOperationError.From(
          lifterId: task.LifterId,
          operation: "create",
          floor: task.Floor,
          message: "创建提升机任务失败：" + error.Message,
          level: "danger"
        ));
      });
    }

    [CapSubscribe(LifterTaskImportedEvent.Message, Group = Group)]
    public void HandleTaskImported(LifterTaskImportedEvent param)
    {
      if (param.LifterId != "1") {
        if (param.Barcode != null) {
          _lifters.Get(param.LifterId).SetPalletCode(param.Floor, param.Barcode);
        }
        if (param.Destination != null) {
          _lifters.Get(param.LifterId).SetDestination(param.Floor, param.Destination);
        }
      }

      _lifters.Get(param.LifterId).SetImported(param.Floor, true);
    }

    [CapSubscribe(LifterTaskScannedEvent.Message, Group = Group)]
    public void HandleTaskScanned(LifterTaskScannedEvent param)
    {
      var lifter = _lifters.Get(param.LifterId);

      if (lifter.GetDestination(param.Floor) != "0") {
        return;
      }

      var barcode = _lifters.Get(param.LifterId).GetPalletCode(param.Floor);
      var destination = "";
      var taskid = "";

      try {
        var info = _wms.GetPalletInfo(barcode);
        taskid = info.TaskId;
        destination = info.Destination;
      } catch {
        _cap.Publish(LifterTaskQueryFailedEvent.Message, new LifterTaskQueryFailedEvent(param.LifterId, param.Floor, barcode));
        return;
      }

      _cap.Publish(LifterTaskQueriedEvent.Message, new LifterTaskQueriedEvent(
        param.LifterId, param.Floor, barcode, destination, taskid
      ));
    }

    [CapSubscribe(LifterTaskQueriedEvent.Message, Group = Group)]
    public void HandleTaskQueried(LifterTaskQueriedEvent param)
    {
      _lifters.Get(param.LifterId).SetDestination(param.Floor, param.Destination);
    }

    [CapSubscribe(LifterTaskExportedEvent.Message, Group = Group)]
    public void HandleTaskExported(LifterTaskExportedEvent param)
    {
      var now = DateTime.Now;
      var lifter = _lifters.Get(param.LifterId);

      // 对于同一台提升机的同一楼层，10 秒内只接收一次取货请求
      if (lifter.ExportedAt[param.Floor].AddSeconds(10) > now) {
        return;
      }

      lifter.ExportedAt[param.Floor] = now;

      var barcode = lifter.GetPalletCode(param.Floor);

      if (barcode.Length != 6 || !int.TryParse(barcode, out _)) {
        _cap.Publish(LifterTaskPickingFailedEvent.Message, new LifterTaskPickingFailedEvent(param.LifterId, param.Floor, barcode, "托盘号异常"));
        return;
      }

      try {
        var taskId = _wms.GetPalletInfo(barcode).TaskId;
        _wms.RequestPicking(param.LifterId, param.Floor, barcode, taskId);
        _cap.Publish(LifterTaskPickingEvent.Message, new LifterTaskPickingEvent(param.LifterId, param.Floor, barcode, taskId));
      } catch (Exception e) {
        _cap.Publish(LifterTaskPickingFailedEvent.Message, new LifterTaskPickingFailedEvent(param.LifterId, param.Floor, barcode, e.Message));
      }
    }

    [CapSubscribe(LifterTaskTaken.Message, Group = Group)]
    public void HandleTaskTaken(LifterTaskTaken param)
    {
      var runtimeTask = _domain.LifterRuntimeTasks
        .Include(task => task.LifterTask)
        .First(task => task.Barcode == param.Barcode);
      var task = runtimeTask.LifterTask;
      var lifter = _lifters.Get(task.LifterId);

      lifter.SetPickuped(task.Floor, true);

      task.SetTaken();
      _domain.Remove(runtimeTask);
      _domain.SaveChanges();

      Task.Delay(2000).ContinueWith(async _ => {
        await _;
        lifter.SetPickuped(task.Floor, false);
      });
    }
  }
}