using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RepositoryLayer.Entity
{
    public class AddressBookEntry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; }=string.Empty;
        [Required]
        public string EmailAddress { get; set; }=string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        public int UserId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

    }

}
