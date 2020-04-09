using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renet.Web;

namespace Tiantong.Wms.Api
{
  public class GoodController : BaseController
  {
    private IAuth _auth;

    private GoodRepository _goods;

    private ItemRepository _items;

    private StockRepository _stocks;

    private LocationRepository _locations;

    // private OrderItemRepository _orderItems;

    private WarehouseRepository _warehouses;

    private GoodCategoryRepository _goodCategories;

    public GoodController(
      IAuth auth,
      GoodRepository goods,
      ItemRepository items,
      StockRepository stocks,
      LocationRepository locations,
      WarehouseRepository warehouses,
      GoodCategoryRepository itemCategories
    ) {
      _auth = auth;
      _goods = goods;
      _items = items;
      _stocks = stocks;
      _locations = locations;
      _warehouses = warehouses;
      _goodCategories = itemCategories;
    }

    public class CreateParams: Good
    {

    }

    public object Create([FromBody] CreateParams param)
    {
      _auth.EnsureOwner();
      _warehouses.EnsureOwner(param.warehouse_id, _auth.User.id);

      if (param.number != null) {
        _goods.EnsureNumberUnique(param.warehouse_id, param.number);
      }

      _goods.Add(param);

      _items.UnitOfWork.SaveChanges();

      return SuccessOperation("货品已创建", param.id);
    }

    public class AddGoodItemParams
    {
      public int warehouse_id { get; set; }

      public string good_name { get; set; }

      public string item_name { get; set; }

      public string item_unit { get; set; }
    }

    public object AddGoodItem([FromBody] AddGoodItemParams param)
    {
      _warehouses.EnsureOwner(param.warehouse_id, _auth.User.id);

      var good = _goods.Table
        .Where(entity => entity.name == param.good_name)
        .FirstOrDefault();
      var item = new Item();
      item.name = param.item_name;
      item.unit = param.item_unit;

      if (good == null) {
        good = new Good();
        good.warehouse_id = param.warehouse_id;
        good.name =  param.good_name;
        good.items = new List<Item> { item };
      } else {
        _items.EnsureUnique(item);
        good.items = new List<Item> { item };
      }
      _goods.Update(good);
      _goods.UnitOfWork.SaveChanges();

      return SuccessOperation("货品规格已添加");
    }

    public class DeleteParams
    {
      [Nonzero]
      public int id { get; set; }
    }

    public object Delete([FromBody] DeleteParams param)
    {
      _auth.EnsureOwner();
      var good = _goods.EnsureGetByOwner(param.id, _auth.User.id);

      good.is_deleted = true;
      _goods.UnitOfWork.SaveChanges();

      return SuccessOperation("货品已删除");
    }

    public class UpdateParams: Good
    {

    }

    public object Update([FromBody] UpdateParams param)
    {
      _auth.EnsureOwner();
      _warehouses.EnsureOwner(param.warehouse_id, _auth.User.id);
      _goods.Update(param);
      _goods.UnitOfWork.SaveChanges();

      return SuccessOperation("货品信息已保存");
    }

    public class SearchParams : BaseSearchParams
    {
      [Nonzero]
      public int warehouse_id { get; set; }
    }

    public IPagination<Good> Search([FromBody] SearchParams param)
    {
      _auth.EnsureOwner();
      var warehouseId = (int) param.warehouse_id;
      _warehouses.EnsureOwner(warehouseId, _auth.User.id);

      var goods = _goods.Table
        .Include(good => good.items)
          .ThenInclude(item => item.stocks)
        .Where(good =>
          good.warehouse_id == param.warehouse_id && 
          good.is_deleted == false && (
            param.search == null ? true :
            good.name.Contains(param.search) ||
            good.number.Contains(param.search)
          )
        )
        .OrderBy(good => good.name)
        .Paginate<Good>(param.page, param.page_size);

      return goods;
    }

    public class FindParams
    {
      public int id { get; set; }
    }

    public Good Find([FromBody] FindParams param)
    {
      _auth.EnsureOwner();

      var good = _goods.EnsureGetByOwner(param.id, _auth.User.id);

      return good;
    }
  }
}
