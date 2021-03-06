using Midos.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Tiantong.Iot.Entities;

namespace Tiantong.Iot.Api
{
  [Route("/")]
  public class AppController: BaseController
  {
    private Config _config;

    private IRandom _random;

    private PlcBuilder _builder;

    private PlcManager _plcManager;

    public AppController(Config config, IRandom random, PlcManager plcManager, PlcBuilder builder)
    {
      _config = config;
      _random = random;
      _builder = builder;
      _plcManager = plcManager;
    }

    // [HttpGet("/{*path}")]
    // public object RedirectAll(string path)
    // {
    //   return Redirect("/index.html/" + path);
    // }

    [HttpPost("/")]
    public object Home()
    {
      return new {
        message = _config.AppName,
        version = _config.AppVersion,
      };
    }

    [HttpPost("data")]
    public object Data([FromBody] object data)
    {
      return data;
    }

    [HttpPost("test")]
    public object TestRun()
    {
      var plc = new Plc {
        id = 1,
        name = "test",
        model = PlcModel.Test,
        host = "192.168.20.10",
        port = 102,
        comment = "test plc comment",
        states = new List<PlcState> {
          new PlcState {
            id = 1,
            plc_id = 1,
            type = PlcStateType.UInt16,
            length = 1,
            name = "心跳",
            address = "D1.100",
            is_heartbeat = true,
            heartbeat_interval = 1000,
            heartbeat_max_value = 10000,
            is_collect = true,
            collect_interval = 1000,
            state_http_pushers = new List<PlcStateHttpPusher> {
              new PlcStateHttpPusher {
                id = 1,
                state_id = 1,
                pusher_id = 1,
                pusher = new HttpPusher {
                  url = "http://localhost:5000/data",
                  field = "heartbeat",
                  header = "{}",
                  body = "{\"plc\": 1}",
                  to_string = false,
                }
              }
            }
          },
          new PlcState {
            id = 2,
            plc_id = 1,
            type = PlcStateType.String,
            length = 10,
            name = "扫码器",
            address = "D1.120",
            is_heartbeat = false,
            heartbeat_interval = 0,
            heartbeat_max_value = 0,
            is_collect = true,
            collect_interval = 1000,
            state_http_pushers = new List<PlcStateHttpPusher> {
              new PlcStateHttpPusher {
                id = 2,
                state_id = 2,
                pusher_id = 2,
                pusher = new HttpPusher {
                  url = "http://localhost:5000/data",
                  field = "scanner",
                  header = "{}",
                  body = "{\"plc\": 1}",
                  to_string = false,
                }
              }
            }
          }
        }
      };

      var worker = _builder.BuildWorker(plc);

      _plcManager.Run(worker);

      return SuccessOperation("Plc 运行中");
    }

    [HttpPost("test/stop")]
    public object TestStop()
    {
      _plcManager.Stop(1);

      return SuccessOperation("PLC 已停止");
    }
  }
}