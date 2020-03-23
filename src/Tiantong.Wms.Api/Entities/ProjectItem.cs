using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tiantong.Wms.Api
{
  [Table("project_items")]
  public class ProjectItem : Entity
  {
    [Key]
    public int id { get; set; }

    public int warehouse_id { get; set; }

    public int project_id { get; set; }

    public int item_id { get; set; }

    public int order_id { get; set; }

    public int supplier_id { get; set; }

    public int quantity { get; set; }

  }
}