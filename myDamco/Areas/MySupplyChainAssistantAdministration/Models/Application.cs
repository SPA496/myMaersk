using System.ComponentModel.DataAnnotations;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Models
{
    public class Application
    {
        public int ApplicationId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(8)]
        public string Abbreviation { get; set; }
    }
}