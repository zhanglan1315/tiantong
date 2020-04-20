using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Tiantong.Wms.Api
{
  [Table("users")]
  public class User : Entity
  {
    [EmailAddress(ErrorMessage = "邮箱地址无效")]
    public virtual string email { get; set; }

    [JsonIgnore]
    public virtual string type { get; set; }

    [JsonIgnore]
    public virtual string password { get; set; }

    [StringRange(2, 50, ErrorMessage = "姓名长度必须在2～50之间")]
    public virtual string name { get; set; } = "";

    public virtual bool is_enabled { get; set; } = true;

    public virtual DateTime created_at { get; set; } = DateTime.Now;

  }
}
