using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Namei.Common.Api
{
  public class RcsController: BaseController
  {
    const string LocationMethod = "location";

    const string AreaMethod = "area";

    static readonly Dictionary<string, bool> NotAllowedArea = new() {
      { "201", true },
      { "216", true },
      { "217", true },
      { "220", true },
      { "221", true },
      { "316", true },
      { "317", true },
      { "401", true },
    };

    private readonly RcsContext _rcs;

    private readonly RcsHttpService _rcsHttp;

    public RcsController(RcsContext rcs, RcsHttpService rcsHttp)
    {
      _rcs = rcs;
      _rcsHttp = rcsHttp;
    }
    public class ConvertParams
    {
      public string Value { get; set; }
    }

    public class ConvertResult
    {
      public string Value { get; set; }
    }

    [HttpPost("/rcs/mapDataName/find")]
    public object Convert([FromBody] ConvertParams param)
    {
      var result = new ConvertResult();

      if (param.Value == null || param.Value == "") {
        result.Value = param.Value;

        return result;
      }

      var dataName = _rcs.Set<MapData>()
        .FirstOrDefault(data => data.DataName == param.Value)
        ?.DataName;

      if (dataName == null) {
        dataName = _rcs.Set<MapData>()
          .FirstOrDefault(data => data.MapDataCode == param.Value)
          ?.DataName;
      }

      result.Value = dataName ?? param.Value;

      return result;
    }

    public class SearchParams
    {
      public string MapCode { get; set; }

      public string MapDataCode { get; set; }

      public string AreaCode { get; set; }

      public string PodCode { get; set; }

      public string DataName { get; set; }

      public bool? IsPodBound { get; set; }

      public int Page { get; set; } = 1;

      public int PageSize { get; set; } = 20;
    }

    [HttpPost("/public/rcs/mapData/search")]
    public Pagination<MapData> Search([FromBody] SearchParams param)
    {
      var query = _rcs.MapData.AsQueryable();

      if (param.IsPodBound == true) {
        query = query.Where(map => map.PodCode != null);
      } else if (param.IsPodBound == false) {
        query = query.Where(map => map.PodCode == null);
      }

      if (param.MapDataCode != null) {
        query = query.Where(map => map.MapDataCode == param.MapDataCode);
      }

      if (param.MapCode != null) {
        query = query.Where(map => map.MapCode == param.MapCode);
      }

      if (param.AreaCode != null) {
        query = query.Where(map => map.AreaCode == param.AreaCode);
      }

      if (param.PodCode != null) {
        query = query.Where(map => map.PodCode == param.PodCode);
      }

      if (param.DataName != null) {
        query = query.Where(map => map.DataName == param.DataName);
      }

      return query.Paginate(param.Page, param.PageSize);
    }

    public class BindPodRequest
    {
      public string LocationCode { get; set; }

      public string PodCode { get; set; }
    }

    [HttpPost("/public/rcs/bindPodAndBerth")]
    public async Task<Result> BindPod([FromBody] BindPodRequest request)
    {
      var result = new Result();
      var response = await _rcsHttp.BindPodAndBerth(
        new BindPodAndBerthRquest(request.PodCode, request.LocationCode, "1")
      );

      if (response.code != "0") {
        result.SetError(response.message, "99");
      } else {
        result.Message = "????????????";
      }

      return result;
    }

    public class UnbindPodRequest
    {
      public string Method { get; set; }

      public string LocationCode { get; set; }
    }

    public class BindResult
    {
      public string MapDataCode { get; set; }

      public string DataName { get; set; }

      public string PodCode { get; set; }

      public string AreaCode { get; set; }

      public string Message { get; set; }

      public BindResult(string mapCode, string dataName, string podCode, string areaCode)
      {
        MapDataCode = mapCode;
        DataName = dataName;
        PodCode = podCode;
        AreaCode = areaCode;
      }
    }

    public class UnbindPodResult: Result
    {
      public List<BindResult> FailedTasks { get; set; } = new List<BindResult>();

      public List<BindResult> ExecutedTasks { get; set; } = new List<BindResult>();
    }

    [HttpPost("/public/rcs/unbindPodAndBerth")]
    public object UnbundlePod([FromBody] UnbindPodRequest param)
    {
      var result = new UnbindPodResult();
      var map = _rcs.MapData.FirstOrDefault(
        map => map.MapDataCode == param.LocationCode || map.DataName == param.LocationCode
      );

      result.Message = "????????????????????????";

      if (map == null) {
        result.SetError($"?????????????????????: {param.LocationCode}", "10");

        return result;
      }

      var maps = new List<MapData>();

      if (param.Method == LocationMethod) {
        maps = _rcs.MapData.Where(item => item.PodCode == map.PodCode && item.PodCode != null).ToList();
      } else if (param.Method == AreaMethod) {
        if (map.AreaCode == null) {
          result.SetError("??????????????????????????????", "11");

          return result;
        } else if (NotAllowedArea.ContainsKey(map.AreaCode)) {
          result.SetError($"??????????????????????????????: {param.LocationCode}", "12");

          return result;
        }

        maps = _rcs.MapData.Where(item => item.AreaCode == map.AreaCode && item.PodCode != null).ToList();
      } else {
        result.SetError($"?????????????????????: {param.Method}", "13");

        return result;
      }

      foreach (var item in maps) {
        var req = new BindPodAndBerthRquest(item.PodCode, item.MapDataCode, "0");
        var rcsResult = _rcsHttp.BindPodAndBerth(req).GetAwaiter().GetResult();
        var bindResult = new BindResult(item.MapDataCode, item.DataName, item.PodCode, item.AreaCode);

        if (rcsResult.code != "0") {
          bindResult.Message = rcsResult.message;
          result.FailedTasks.Add(bindResult);
          result.SetError("????????????????????????????????????????????????", "30");
        } else {
          bindResult.Message = "????????????";
          result.ExecutedTasks.Add(bindResult);
        }
      }

      return result;
    }
  }
}
