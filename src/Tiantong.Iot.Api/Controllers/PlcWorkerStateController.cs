using Microsoft.AspNetCore.Mvc;
using Renet.Web;

namespace Tiantong.Iot.Api
{
  [Route("/plc-workers/states")]
  public class PlcWorkerStateController: BaseController
  {
    private PlcManager _manager;

    private PlcStateLogger _logger;

    public PlcWorkerStateController(PlcManager manager, PlcStateLogger logger)
    {
      _manager = manager;
      _logger = logger;
    }

    private void HandleSet<T>(string plc, string state, T value)
    {
      _manager.Get(plc).Set<T>(state, value);
    }

    private T HandleGet<T>(string plc, string state)
    {
      return _manager.Get(plc).Get<T>(state);
    }

    public class GetParams<T>
    {
      public string plc { get; set; }

      public string state { get; set; }
    }

    public class SetParams<T>: GetParams<T>
    {
      public T value { get; set; }
    }

    [HttpPost]
    [Route("uint16/get")]
    public object GetUInt16([FromBody] GetParams<ushort> param)
    {
      return new {
        value = HandleGet<ushort>(param.plc, param.state)
      };
    }

    [HttpPost]
    [Route("uint32/get")]
    public object GetInt32([FromBody] GetParams<int> param)
    {
      return new {
        value = HandleGet<int>(param.plc, param.state)
      };
    }

    [HttpPost]
    [Route("string/get")]
    public object GetString([FromBody] GetParams<string> param)
    {
      return new {
        value = HandleGet<string>(param.plc, param.state)
      };
    }

    //

    [HttpPost]
    [Route("uint16/set")]
    public object SetUInt16([FromBody] SetParams<ushort> param)
    {
      HandleSet<ushort>(param.plc, param.state, param.value);

      return SuccessOperation("数据已写入");
    }

    [HttpPost]
    [Route("uint32/set")]
    public object SetInt32([FromBody] SetParams<int> param)
    {
      HandleSet<int>(param.plc, param.state, param.value);

      return SuccessOperation("数据已写入");
    }

    [HttpPost]
    [Route("string/set")]
    public object SetString([FromBody] SetParams<string> param)
    {
      HandleSet<string>(param.plc, param.state, param.value);

      return SuccessOperation("数据已写入");
    }

  }

}