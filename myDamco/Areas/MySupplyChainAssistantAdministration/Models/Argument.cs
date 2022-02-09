using System.ComponentModel.DataAnnotations;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Models
{
    public class Argument
    {
        [Required]
        [StringLength(32)]
        public string id { get; set; }

        [Required]
        [StringLength(32)]
        public string alias { get; set; }

        [Required]
        [StringLength(255)]
        public string matcher { get; set; }
    }
}