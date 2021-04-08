using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table("Fantasy")]
    public class Fantasy
    {
        [Key]
        public string FantasyId { get; set; }

        public string Description { get; set; }
        public ICollection<AppUser> UserFantasy { get; set; }
    }
}
