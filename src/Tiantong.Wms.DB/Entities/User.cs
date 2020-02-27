using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Tiantong.Wms.DB
{
  [Table("users")]
  public class User
  {
    [Key]
    public int id { get; set; }

    public string email { get; set; }

    [JsonIgnore]
    public string password { get; set; }

    public string[] roles { get; set; }

    public string name { get; set; }

    public DateTime created_at { get; set; }

    public User()
    {
      created_at = DateTime.Now;
    }
  }
}
