using Renet.Web;

namespace Tiantong.Wms.Api
{
  public class WebRoutes : RouterProvider
  {
    public override void Define()
    {
      Get("/", "App.Home");
      Post("/", "App.Home");

      Post("/dev/initialize", "App.Initialize");
      Post("/dev/restore", "App.Restore");
      Post("/dev/seed", "App.InsertTestData");

      Post("/users/search", "User.Search");

      Post("/users/register", "User.Register");
      Post("/users/person/update", "User.UpdatePerson");
      Post("/users/person/profile", "User.GetPersonProfile");

      Post("/auth/email", "User.AuthByEmail");
      Post("/auth/token/refresh", "User.RefreshToken");

      Post("/warehouses/create", "Warehouse.Create");
      Post("/warehouses/delete", "Warehouse.Delete");
      Post("/warehouses/update", "Warehouse.Update");
      Post("/warehouses/search", "Warehouse.Search");
      Post("/warehouses/find", "Warehouse.Find");

      Post("/areas/create", "Area.Create");
      Post("/areas/delete", "Area.Delete");
      Post("/areas/update", "Area.Update");
      Post("/areas/search", "Area.Search");

      Post("/locations/create", "Location.Create");
      Post("/locations/delete", "Location.Delete");
      Post("/locations/update", "Location.Update");
      Post("/locations/search", "Location.Search");

      Post("/projects/create", "Project.Create");
      Post("/projects/delete", "Project.Delete");
      Post("/projects/update", "Project.Update");
      Post("/projects/search", "Project.Search");

      Post("/items/categories/create", "ItemCategory.Create");
      Post("/items/categories/delete", "ItemCategory.Delete");
      Post("/items/categories/update", "ItemCategory.Update");
      Post("/items/categories/search", "ItemCategory.Search");

      Post("/orders/categories/create", "OrderCategory.Create");
      Post("/orders/categories/delete", "OrderCategory.Delete");
      Post("/orders/categories/update", "OrderCategory.Update");
      Post("/orders/categories/search", "OrderCategory.Search");

      Post("/items/create", "Item.Create");
      Post("/items/delete", "Item.Delete");
      Post("/items/update", "Item.Update");
      Post("/items/search", "Item.Search");

      Post("/suppliers/create", "Supplier.Create");
      Post("/suppliers/delete", "Supplier.Delete");
      Post("/suppliers/update", "Supplier.Update");
      Post("/suppliers/search", "Supplier.Search");

      Post("/suppliers/create", "Supplier.Create");
      Post("/suppliers/delete", "Supplier.Delete");
      Post("/suppliers/update", "Supplier.Update");
      Post("/suppliers/search", "Supplier.Search");

      Post("/orders/search", "OrderInboundComplete.Search");
      Post("/orders/complete", "OrderInboundComplete.Handle");

      // Post("/orders/inbounds/create", "OrderInbound.Create");
      // Post("/orders/inbounds/search", "OrderInbound.Search");
      // Post("/orders/inbounds/submit", "OrderInbound.Submit");

      // Post("/orders/outbounds/create", "Order.OutboundCreate");
      // Post("/orders/outbounds/search", "Order.OutboundSearch");
      // Post("/orders/outbounds/submit", "Order.OutboundSubmit");

      // Post("/orders/inventories/create", "Order.InventoryCreate");
      // Post("/orders/inventories/search", "Order.InventorySearch");
      // Post("/orders/inventories/submit", "Order.InventorySubmit");

      // Post("/orders/returns/create", "Order.ReturnCreate");
      // Post("/orders/returns/search", "Order.ReturnSearch");
      // Post("/orders/returns/submit", "Order.ReturnSubmit");

      Post("/stocks/search", "Stock.Search");
      Post("/stocks/records/search", "StockRecord.Search");

    }
  }
}
