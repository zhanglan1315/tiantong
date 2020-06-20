using DBCore;

namespace Tiantong.Wms.DB
{
  public class CreateGoodsTable : IMigration
  {
    public void Up(DbContext db)
    {
      db.ExecuteFromSql("Migration.004100_CreateGoodsTable");
    }

    public void Down(DbContext db)
    {
      db.ExecuteSql("drop table if exists goods");
    }
  }
}