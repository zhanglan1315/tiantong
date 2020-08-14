using DotNetCore.CAP;
using Renet;
using System.Linq;
using System.Collections.Generic;
using Tiantong.Iot.Utils;

namespace Namei.Wcs.Api
{
  public class LifterServiceManager
  {
    private Dictionary<string, LifterService> _lifters = new Dictionary<string, LifterService>();

    private ICapPublisher _cap;

    public LifterServiceManager(
      FirstLifterService firstLifter,
      SecondLifterService secondLifter,
      ThirdLifterService thirdLifter,
      ICapPublisher cap
    ) {
      _cap = cap;
      _lifters.Add("1", firstLifter);
      _lifters.Add("2", secondLifter);
      _lifters.Add("3", thirdLifter);
    }

    public LifterService Get(string id)
    {
      if (!_lifters.ContainsKey(id)) {
        throw KnownException.Error($"lifter_id: {id} 设备不存在", 400);
      }

      return _lifters[id];
    }
  }

  public class LifterState
  {
    public bool IsWorking { get; set; }

    public bool IsAlarming { get; set; }

    public List<LifterFloorState> Floors { get; set; }
  }

  public class LifterFloorState
  {
    public bool IsScanned { get; set; }

    public bool IsImportAllowed { get; set; }

    public bool IsExported { get; set; }

    public bool IsDoorOpened { get; set; }
  }

  public abstract class LifterService
  {
    // 放货完成
    public abstract void SetImported(string floor, bool value);

    // 取货完成
    public abstract void SetPickuped(string floor, bool value);

    // 获取托盘码
    public abstract string GetPalletCode(string floor);

    // 设置目的楼层
    public abstract void SetDestination(string from, string to);

    // 是否允许放货
    public abstract bool IsImportAllowed(string floor);

    // 是否允许取货
    public abstract bool IsRequestingPickup(string floor);

    public abstract LifterState GetStates();
  }

  public class FirstLifterService: LifterService
  {
    public static bool GetIsTaskScanned(string data)
      => MelsecStateHelper.GetBit(data, 4);

    public static bool GetIsImportAllowed(string data)
      => MelsecStateHelper.GetBit(data, 6);

    public static bool GetIsRequestingPickup(string data)
      => MelsecStateHelper.GetBit(data, 7);

    public static bool IsTaskScanned(string data, string oldData)
      => GetIsTaskScanned(data) && !GetIsTaskScanned(oldData);

    public static bool IsRequestingPickup(string data, string oldData)
      => GetIsRequestingPickup(data) && !GetIsRequestingPickup(oldData);

    public static bool IsImportAllowed(string data, string oldData)
      => GetIsImportAllowed(data) && !GetIsImportAllowed(oldData);

    private PlcStateService _plc;

    public FirstLifterService(PlcStateService plc)
    {
      _plc = plc;
      _plc.Configure("http://localhost:5101", "改造货梯");
    }

    public override void SetImported(string floor, bool value)
      => _plc.Set($"{floor}F - 放货完成", value ? "1" : "0");

    public override void SetPickuped(string floor, bool value)
      => _plc.Set($"{floor}F - 取货完成", value ? "1" : "0");

    public override string GetPalletCode(string floor)
      => _plc.Get($"{floor}F - A 段 - 托盘码");

    public override void SetDestination(string from, string to)
      => _plc.Set($"{from}F - 目的楼层", to);

    public override bool IsImportAllowed(string floor)
      => GetIsImportAllowed(_plc.Get($"{floor}F - A 段 - 输送机"));

    public override bool IsRequestingPickup(string floor)
      => GetIsRequestingPickup(_plc.Get($"{floor}F - A 段 - 输送机"));

    public bool IsTaskScanned(string floor)
      => GetIsTaskScanned(_plc.Get($"{floor}F - A 段 - 输送机"));

    public override LifterState GetStates()
    {
      var states = _plc.GetValues();

      return new LifterState() {
        IsWorking = states["升降平台状态"] != "0",
        IsAlarming = states["故障代码"] != "0",
        Floors = Enumerable.Range(1, 4).Select(floor => new LifterFloorState {
          IsScanned = GetIsTaskScanned(states[$"{floor}F - A 段 - 输送机"]),
          IsImportAllowed = MelsecStateHelper.GetBit(states[$"{floor}F - A 段 - 输送机"], 6),
          IsExported = MelsecStateHelper.GetBit(states[$"{floor}F - A 段 - 输送机"], 7),
          IsDoorOpened = false
        }).ToList()
      };
    }
  }

  public class StandardLifterService: LifterService
  {
    private PlcStateService _plc;

    public StandardLifterService(PlcStateService plc)
    {
      _plc = plc;
    }

    public override void SetImported(string floor, bool value)
      => _plc.Set($"{floor}F - A 段 - 放取货状态", value ? "3" : "0");

    public override void SetPickuped(string floor, bool value)
      => _plc.Set($"{floor}F - A 段 - 放取货状态", value ? "5" : "0");

    public override string GetPalletCode(string floor)
      => _plc.Get($"{floor}F - A 段 - 托盘码");

    public override void SetDestination(string from, string to)
      => _plc.Set($"{from}F - A 段 - 目标楼层", to);

    public override bool IsImportAllowed(string floor)
      => _plc.Get($"{floor}F - A 段 - 工位状态") == "2";

    public override bool IsRequestingPickup(string floor)
      => _plc.Get($"{floor}F - A 段 - 工位状态") == "3";

    public override LifterState GetStates()
    {
      var states = _plc.GetValues();

      return new LifterState() {
        IsWorking = states["升降平台状态"] != "0",
        IsAlarming = states["故障代码"] != "1",
        Floors = Enumerable.Range(1, 4).Select(floor => new LifterFloorState {
          IsScanned = states[$"{floor}F - A 段 - 读码状态"] == "1",
          IsImportAllowed = states[$"{floor}F - A 段 - 工位状态"] == "2",
          IsExported = states[$"{floor}F - A 段 - 工位状态"] == "3",
          IsDoorOpened = false
        }).ToList()
      };
    }
  }

  public class SecondLifterService: StandardLifterService
  {
    public SecondLifterService(PlcStateService plc): base(plc)
    {
      plc.Configure("http://localhost:5101", "提升机 - 1");
    }
  }

  public class ThirdLifterService: StandardLifterService
  {
    public ThirdLifterService(PlcStateService plc): base(plc)
    {
      plc.Configure("http://localhost:5101", "提升机 - 2");
    }
  }
}
