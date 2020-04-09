using DBCore;

namespace Tiantong.Wms.DB
{
  public class CreateDepartmentUsersTable : IMigration
  {
    public void Up(DbContext db)
    {
      db.ExecuteFromSql("Migration.0014_CreateDepartmentUsersTable");
    }

    public void Down(DbContext db)
    {
      db.ExecuteSql("drop table if exists department_users");
    }
  }
}
