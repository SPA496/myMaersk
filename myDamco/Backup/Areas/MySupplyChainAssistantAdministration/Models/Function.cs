using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace myDamco.Areas.MySupplyChainAssistantAdministration.Models
{
    public class Function
    {
        public int functionId { get; set; }

        [Required]
        [StringLength(255)]
        public string urlFormat { get; set; }

        [Required]
        [StringLength(255)]
        public string fallbackUrl { get; set; }


        [Required]
        [StringLength(16)]
        public string name { get; set; }

        [Required]
        public string description { get; set; }


        public string protocol { get; set; }

        [Required]
        public string host { get; set; }

        public string port { get; set; }

        [Required]
        public string path { get; set; }

        public IList<Argument> arguments { get; set; }

        public IList<int> references { get; set; }

        public IList<EntityIdentifier> entityIdentifiers { get; set; }

        public IList<EventHook> hooks { get; set; }

        [Required]
        public int applicationId { get; set; }
    }
}