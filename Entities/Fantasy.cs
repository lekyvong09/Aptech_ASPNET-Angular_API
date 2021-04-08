using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Fantasy
    {
        [Key]
        public string FantasyId { get; set; }

        public string Description { get; set; }
        public ICollection<AppUser> UserFantasy { get; set; }
    }
}
