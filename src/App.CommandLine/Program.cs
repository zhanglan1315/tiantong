using System;
using Tiantong.Iot;

namespace App.CommandLine
{
  class Program
  {
    static void Main()
    {
      var plc = new PlcWorker();
      var random = new Renet.Random();

      plc.Name("测试200smart").UseS7200Smart("192.168.20.10", 102);

      plc.Define("心跳").UShort("D1.100")
        .Heartbeat(1).Collect(1)
        .Watch(value => {
          Console.WriteLine(value);
          plc.String("扫码器").Set(random.String(10));
        });

      plc.Define("扫码器").String("D1.120", 10).Collect(1)
        .Watch(value => {
          if (value == "0000000000") return;

          Console.WriteLine("扫码数据: " + value);
          plc.String("扫码器").Set("0000000000");
        });

      plc.Run();
    }
  }
}
