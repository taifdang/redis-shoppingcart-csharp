using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.API.Models
{
    public class Products
    {
        [Key]
        public Guid id { get; set; }
        [Required]
        public string name { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal price { get; set; }
        [Required]
        public int stock { get; set; }
    }
}
