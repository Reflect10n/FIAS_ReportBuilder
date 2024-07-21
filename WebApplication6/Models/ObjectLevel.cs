using System.ComponentModel.DataAnnotations;

namespace WebApplication6.Models
{
    public class ObjectLevel
    {
        [Key]
        public int Id { get; set; }
        public int Level { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
