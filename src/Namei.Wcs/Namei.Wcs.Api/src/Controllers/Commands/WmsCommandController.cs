using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Renet.Web;

namespace Namei.Wcs.Api
{
  public class WmsServiceController: BaseController
  {
    private ICapPublisher _cap;

    private WmsService _wms;

    public WmsServiceController(
      ICapPublisher cap,
      WmsService wms
    ) {
      _cap = cap;
      _wms = wms;
    }

    public class LifterNotify
    {
      public string method { get; set; }

      public string liftCode { get; set; }

      public string floor { get; set; }

      public long taskCode { get; set; }

      public string barCode { get; set; }

      public string destination { get; set; }
    }

    [Route("/finish")]
    public object LiftersNotify([FromBody] LifterNotify param)
    {
      var message = "指令未识别";

      if (param.method == "deliver") {
        _cap.Publish(LifterTaskImportedEvent.Message, new LifterTaskImportedEvent(
          param.liftCode,
          param.floor,
          param.taskCode.ToString(),
          param.barCode,
          param.destination
        ));
        message = "收到放货完成指令";
      } else if (param.method == "pick") {
        _cap.Publish(LifterTaskTakenEvent.Message, new LifterTaskTakenEvent(param.liftCode, param.floor, param.taskCode.ToString()));
        message = "收到取货完成指令";
      } else {
        message = "放取货信号接收异常";

        _cap.Publish(LifterTaskExceptionEvent.Message, new LifterTaskExceptionEvent(param.liftCode, param.floor, message));
      }

      return Success(new {
        message = message,
        liftCode = param.liftCode,
        floor = param.floor,
      });
    }
  }
}
