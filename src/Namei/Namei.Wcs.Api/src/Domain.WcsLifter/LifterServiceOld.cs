using System;
using System.Linq;
using System.Collections.Generic;
using Tiantong.Iot.Utils;

namespace Namei.Wcs.Api
{
  public interface ILifterServiceFactory
  {
    ILifterService Get(string id);

    Dictionary<string, ILifterService> All();
  }

  public class LifterServiceManager: ILifterServiceFactory
  {
    private readonly Dictionary<string, ILifterService> _lifters = new();

    public LifterServiceManager(
      FirstLifterService firstLifter,
      SecondLifterService secondLifter,
      ThirdLifterService thirdLifter
    ) {
      _lifters.Add("1", firstLifter);
      _lifters.Add("2", secondLifter);
      _lifters.Add("3", thirdLifter);
    }

    public ILifterService Get(string id)
    {
      if (!_lifters.ContainsKey(id)) {
        throw KnownException.Error($"lifter_id: {id} 设备不存在", 400);
      }

      return _lifters[id];
    }

    public Dictionary<string, ILifterService> All() => _lifters;
  }

  public class LifterState
  {
    public bool IsConnected { get; set; } = true;

    public bool IsWorking { get; set; } = false;

    public bool IsAlerting { get; set; } = false;

    public string PalletCodeA { get; set; } = "";

    public string PalletCodeB { get; set; } = "";

    public List<LifterFloorState> Floors { get; set; }
      = Enumerable.Range(1, 4)
        .Select(floor => new LifterFloorState() { Index = floor.ToString() })
        .ToList();
  }

  public class LifterFloorState
  {
    public string Index { get; set; }

    public string Destination { get; set; } = "0";

    // A 段托盘码
    public string PalletCodeA { get; set; } = "";

    // B 段托盘码
    public string PalletCodeB { get; set; } = "";

    // 允许放货
    public bool IsImportAllowed { get; set; } = false;

    // 请求取货
    public bool IsExported { get; set; } = false;
  }

  public interface ILifterService
  {
    Dictionary<string, DateTime> ExportedAt { get; }

    void Import(string floor, string destination, string barcode);

    void SetImported(string floor, bool value);

    void SetPickuped(string floor, bool value);

    string GetPalletCode(string floor);

    string GetDestination(string floor);

    string GetTaskDestination(string floor);

    void SetPalletCode(string floor, string code);

    void SetDestination(string from, string to);

    bool IsImportAllowed(string floor);

    bool IsRequestingPickup(string floor);

    LifterState GetStates();
  }

  public abstract class LifterService: ILifterService
  {
    public Dictionary<string, DateTime> ExportedAt
    {
      get => new() {
        { "1", DateTime.MinValue },
        { "2", DateTime.MinValue },
        { "3", DateTime.MinValue },
        { "4", DateTime.MinValue },
      };
    }

    public abstract void Import(string floor, string destination, string barcode);

    // 放货完成
    public abstract void SetImported(string floor, bool value);

    // 取货完成
    public abstract void SetPickuped(string floor, bool value);

    // 获取托盘码
    public abstract string GetPalletCode(string floor);

    public abstract string GetDestination(string floor);

    public abstract string GetTaskDestination(string floor);

    // 设置托盘码
    public abstract void SetPalletCode(string floor, string code);

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
    public static bool GetIsSpare(string data)
      => !MelsecStateHelper.GetBit(data, 1) && !MelsecStateHelper.GetBit(data, 3);

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

    public static bool IsSpare(string data, string oldData)
      => GetIsSpare(data) && !GetIsSpare(oldData);

    private readonly PlcStateService _plc;

    public FirstLifterService(PlcStateService plc, Config config)
    {
      _plc = plc;
      _plc.Configure(config.PlcUrl, "改造货梯");
    }

    public override void SetImported(string floor, bool value)
      => _plc.Set($"{floor}F - 放货完成", value ? "1" : "0");

    public override void SetPickuped(string floor, bool value)
      => _plc.Set($"{floor}F - 取货完成", value ? "1" : "0");

    public override void SetDestination(string from, string to)
      => _plc.Set($"{from}F - 目的楼层", to);

    public override string GetPalletCode(string floor)
      => _plc.Get($"{floor}F - A 段 - 托盘码");

    public override string GetDestination(string floor)
      => _plc.Get($"{floor}F - 目的楼层");

    public override string GetTaskDestination(string floor)
      => _plc.Get($"{floor}F - A 段 - 目的楼层");

    public override bool IsImportAllowed(string floor)
    {
      var state = _plc.Get($"{floor}F - A 段 - 输送机");

      return GetIsImportAllowed(state);
    }

    public override void Import(string floor, string destination, string barcode)
    {
      // SetDestination(floor, destination ?? "0");
      // SetPalletCode(floor, barcode ?? "0");
      SetImported(floor, true);
    }

    public override void SetPalletCode(string floor, string code)
    {
      // @Todo: Waiting
    }

    public override bool IsRequestingPickup(string floor)
      => GetIsRequestingPickup(_plc.Get($"{floor}F - A 段 - 输送机"));

    public override LifterState GetStates()
    {
      var result = new LifterState();

      try {
        var states = _plc.GetValues();

        result.IsWorking = states["升降平台状态"] != "0";
        result.IsAlerting = states["故障代码"] != "0";
        result.PalletCodeA = states["货梯 - A 段 - 托盘码"];
        result.PalletCodeB = states["货梯 - B 段 - 托盘码"];
        result.Floors.ForEach(floor => {
          floor.PalletCodeA = states[$"{floor.Index}F - A 段 - 托盘码"];
          floor.PalletCodeB = states[$"{floor.Index}F - B 段 - 托盘码"];
          floor.Destination = states[$"{floor.Index}F - A 段 - 目的楼层"];
          floor.IsImportAllowed = GetIsImportAllowed(states[$"{floor.Index}F - A 段 - 输送机"]);
          floor.IsExported = MelsecStateHelper.GetBit(states[$"{floor.Index}F - A 段 - 输送机"], 7);
        });
      } catch {
        result.IsConnected = false;
      }

      return result;
    }
  }

  public class StandardLifterService: LifterService
  {
    private readonly PlcStateService _plc;

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

    public override void SetPalletCode(string floor, string code)
      => _plc.Set($"{floor}F - 任务托盘码", code);

    public override void SetDestination(string from, string to)
      => _plc.Set($"{from}F - 目的楼层", to);

    public override string GetTaskDestination(string floor)
      => _plc.Get($"{floor}F - A 段 - 任务路径").Last().ToString();

    public override string GetDestination(string floor)
      => _plc.Get($"{floor}F - 目的楼层");

    public override bool IsImportAllowed(string floor)
      => _plc.Get($"{floor}F - A 段 - 工位状态") == "2";

    public override bool IsRequestingPickup(string floor)
      => _plc.Get($"{floor}F - A 段 - 工位状态") == "3";

    public override void Import(string floor, string destination, string barcode)
    {
      SetDestination(floor, destination ?? "0");
      SetPalletCode(floor, barcode ?? "0");
      SetImported(floor, true);
    }

    public override LifterState GetStates()
    {
      var result = new LifterState();

      try {
        var states = _plc.GetValues();
        
        result.IsWorking = states["升降平台状态"] != "0";
        result.IsAlerting = states["故障代码"] != "1";
        result.PalletCodeA = states["平台内托盘码"];
        result.PalletCodeB = "0";
        result.Floors.ForEach(floor => {
          floor.PalletCodeA = states[$"{floor.Index}F - A 段 - 托盘码"];
          floor.PalletCodeB = states[$"{floor.Index}F - B 段 - 托盘码"];
          floor.IsExported = states[$"{floor.Index}F - A 段 - 工位状态"] == "3";
          floor.IsImportAllowed = states[$"{floor.Index}F - A 段 - 工位状态"] == "2";
          floor.Destination = states[$"{floor.Index}F - A 段 - 任务路径"].Last().ToString();
        });
      } catch {
        result.IsConnected = false;
      }

      return result;
    }
  }

  public class SecondLifterService: StandardLifterService
  {
    public SecondLifterService(
      Config config,
      PlcStateService plc
    ): base(plc) {
      plc.Configure(config.PlcUrl, "提升机 - 1");
    }
  }

  public class ThirdLifterService: StandardLifterService
  {
    public ThirdLifterService(
      Config config,
      PlcStateService plc
    ): base(plc) {
      plc.Configure(config.PlcUrl, "提升机 - 2");
    }
  }
}
