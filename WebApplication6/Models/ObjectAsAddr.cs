using System.ComponentModel.DataAnnotations;

namespace WebApplication6.Models
{
    public class ObjectAsAddr
    {
        [Key]
        public int Id { get; set; }
        [StringLength(500)]
        public string GuID { get; set; }

        [StringLength(500)]
        public string Name { get; set; }
        [StringLength(100)]
        public string Typename { get; set; }
        public int Level { get; set; }
        public int OperType_Id { get; set; }
        public bool? IsActive { get; set; }

        public Operation Operation { get; set; }
    }
}
