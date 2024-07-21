using System.ComponentModel.DataAnnotations;

namespace WebApplication6.Models
{
    public class Operation
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string Action { get; set; }
    }
}
