using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlaElTalab.Models
{
    public class Service
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ServiceId { get; set; }
        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot be longer than 100 characters")]
        public string Name { get; set; }

        //navigation properties 
        public virtual ICollection<ServiceProvider> Providers { get; set; }
    }
}
