using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Renet.Web;

namespace Tiantong.Iot.Api
{
  [Route("/plc-workers")]
  public class PlcWorkerController: BaseController
  {
    private PlcManager _plcManager;

    private PlcRepository _plcRepository;

    public PlcWorkerController(
      PlcManager plcManager,
      PlcRepository plcRepository
    ) {
      _plcManager = plcManager;
      _plcRepository = plcRepository;
    }

    public class FindParams
    {
      public int plc_id { get; set; }
    }

    [HttpPost]
    [Route("run")]
    public object Run([FromBody] FindParams param)
    {
      var plc = _plcRepository.EnsureFind(param.plc_id);
      var worker = PlcBuilder.Build(plc);

      if (_plcManager.Run(worker)) {
        return SuccessOperation("PLC 开始运行");
      } else {
        return FailureOperation("PLC 正在运行中");
      }
    }

    [HttpPost]
    [Route("stop")]
    public object Stop([FromBody] FindParams param)
    {
      if (_plcManager.Stop(param.plc_id)) {
        return SuccessOperation("PLC 停止运行");
      } else {
        return FailureOperation("PLC 未运行");
      }
    }

    [HttpPost]
    [Route("test")]
    public object Test([FromBody] FindParams param)
    {
      var plc = _plcRepository.EnsureFind(param.plc_id);
      var worker = PlcBuilder.Build(plc);
      
      if (worker.Test()) {
        return SuccessOperation("PLC 连接测试成功");
      } else {
        return FailureOperation("PLC 连接测试失败");
      }
    }

    [HttpPost]
    [Route("is-running")]
    public object IsRunning([FromBody] FindParams param)
    {
      try {
        _plcManager.Get(param.plc_id);

        return new { is_running = true };
      } catch {
        return new { is_running = false };
      }
    }

    [HttpPost]
    [Route("current-values")]
    public Dictionary<string, string> GetCurrentValue([FromBody] FindParams param)
    {
      try {
        return _plcManager.Get(param.plc_id).GetCurrentStateValues();
      } catch {
        throw new HttpException("数据不存在", 400);
      }
    }

    public abstract class SetValue<T>
    {
      public int plc_id { get; set; }

      public int state_id { get; set; }

      public T value { get; set; }
    }

    public class SetUShortValueParams: SetValue<ushort>
    {

    }

    [HttpPost]
    [Route("states/uint16/set-by-id")]
    public object SetUShortValue([FromBody] SetUShortValueParams param)
    {
      _plcManager.Get(param.plc_id).UShort(param.state_id).Set(param.value);

      return SuccessOperation("数据已写入");
    }

    public class SetStringValueParams: SetValue<string>
    {

    }

    [HttpPost]
    [Route("states/string/set-by-id")]
    public object SetStringValue([FromBody] SetStringValueParams param)
    {
      _plcManager.Get(param.plc_id).String(param.state_id).Set(param.value);

      return SuccessOperation("数据已写入");
    }

  }
}
