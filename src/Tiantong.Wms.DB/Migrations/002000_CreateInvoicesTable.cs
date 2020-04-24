using DBCore;

namespace Tiantong.Wms.DB
{
  public class CreateInvoicesTable : IMigration
  {
    public void Up(DbContext db)
    {
      db.ExecuteFromSql("Migration.002000_CreateInvoicesTable");
    }

    public void Down(DbContext db)
    {
      db.ExecuteSql("drop table if exists invoices");
    }
  }
}