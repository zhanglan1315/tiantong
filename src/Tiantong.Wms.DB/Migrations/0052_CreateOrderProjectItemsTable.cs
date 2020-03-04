using DBCore;

namespace Tiantong.Wms.DB
{
  public class CreateOrderProjectItemsTable : IMigration
  {
    public void Up(DbContext db)
    {
      db.ExecuteFromSql("Migration.0052_CreateOrderProjectItemsTable");
    }

    public void Down(DbContext db)
    {
      db.ExecuteSql("drop table if exists order_project_items");
    }
  }
}
