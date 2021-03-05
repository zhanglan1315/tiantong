using System;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;

namespace Namei.Wcs.Api
{
  public class LifterTaskCommand: BaseController
  {
    private ICapPublisher _cap;

    private WmsService _wms;

    public LifterTaskCommand(
      ICapPublisher cap,
      WmsService wms
    ) {
      _cap = cap;
      _wms = wms;
    }

    public class LifterNotify
    {
      public string Method { get; set; }

      public string LiftCode { get; set; }

      public string Floor { get; set; }

      public long TaskCode { get; set; }

      public string BarCode { get; set; }

      public string Destination { get; set; }

      public string Operator { get; set; }
    }

    [Route("/finish")]
    [HttpPost("/lifter-tasks/create")]
    public object LiftersNotify([FromBody] LifterNotify param)
    {
      var message = "指令未识别";

      if (param.Operator is null) {
        param.Operator = "wms";
      }

      if (param.Method == "deliver") {
        _cap.Publish(LifterTaskImportedEvent.Message, new LifterTaskImportedEvent(
          lifterId: param.LiftCode,
          floor: param.Floor,
          taskCode: param.TaskCode.ToString(),
          barcode: param.BarCode,
          destination: param.Destination
        ));
        _cap.Publish(LifterTaskReceived.Message, LifterTaskReceived.From(
          lifterId: param.LiftCode,
          floor: param.Floor,
          destination: param.Destination,
          barcode: param.BarCode,
          taskCode: param.TaskCode.ToString(),
          operatr: param.Operator
        ));

        message = "收到创建提升机任务指令";
      } else if (param.Method == "pick") {
        message = "收到取货完成指令";

        _cap.Publish(LifterTaskTaken.Message, LifterTaskTaken.From(
          barcode: param.BarCode,
          lifterId: param.LiftCode,
          floor: param.Floor
        ));
      } else {
        message = "放取货信号接收异常";

        _cap.Publish(LifterOperationError.Message, LifterOperationError.From(
          lifterId: param.LiftCode,
          floor: param.Floor,
          operation: "wms.finish",
          message: message
        ));
      }

      var result = new {
        message = message,
        method = param.Method,
        liftCode = param.LiftCode,
        floor = param.Floor,
        destination = param.Destination,
        taskCode = param.TaskCode,
        Operator = param.Operator,
      };

      return Result.FromObject(result).StatusCode(201);
    }
  }
}
