using System.Linq;
using Renet.Web;

namespace Tiantong.Wms.Api
{
  public class WarehouseRepository : Repository<Warehouse, int>
  {
    private IAuth _auth;

    public WarehouseRepository(DbContext db, IAuth auth) : base(db)
    {
      _auth = auth;
    }

    public Warehouse[] All()
    {
      return Table.OrderBy(wh => wh.id).ToArray();
    }

    public Warehouse[] Search(int userId)
    {
      return Table
        .Where(wh => wh.owner_user_id == userId)
        .OrderBy(wh => wh.number)
        .OrderBy(wh => wh.id)
        .ToArray();
    }

    public Warehouse Find(int id, int userId)
    {
      try {
        return Table.Where(wh => wh.id == id && wh.owner_user_id == userId).First();
      } catch {
        throw new FailureOperation("仓库不存在");
      }
    }

    public bool HasId(int id)
    {
      return Table.Any(wh => wh.id == id);
    }

    public bool IsOwner(int warehouseId, int userId)
    {
      return Table.Any(wh => wh.id == warehouseId && wh.owner_user_id == userId);
    }

    public bool HasOwner(int id, int userId)
    {
      return Table.Any(wh => wh.id == id && wh.owner_user_id == userId);
    }

    public Warehouse EnsureGet(int id)
    {
      var warehouse = Get(id);

      if (warehouse == null) {
        throw new FailureOperation("仓库不存在");
      }

      return warehouse;
    }

    public Warehouse EnsureGet(int whid, int userId)
    {
      var warehouse = Table.SingleOrDefault(w => w.id == whid && w.owner_user_id == userId);

      if (warehouse == null) {
        throw new FailureOperation("仓库不存在");
      }

      return warehouse;
    }

    public Warehouse EnsureGetByOwner(int warehouseId, int userId)
    {
      var warehouse = EnsureGet(warehouseId);

      if (warehouse.owner_user_id != userId) {
        throw new FailureOperation("仓库认证失败");
      }

      return warehouse;
    }

    public void EnsureOwner(int warehouseId, int userId)
    {
      if (!HasOwner(warehouseId, userId)) {
        throw new FailureOperation("仓库认证失败");
      }
    }

    public void EnsureOwnership(int warehouseId)
    {
      if (
        !Table.Any(w =>
          w.id == warehouseId &&
          w.owner_user_id == _auth.User.id
        )
      ) {
        throw new FailureOperation("仓库身份认证失败");
      }
    }

  }
}
