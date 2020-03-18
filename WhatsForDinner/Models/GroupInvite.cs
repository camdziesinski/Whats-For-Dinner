using System;
using System.Collections.Generic;

namespace WhatsForDinner.Models
{
    public partial class GroupInvite
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public Guid GroupId { get; set; }

        public virtual Groups Group { get; set; }
        public virtual AspNetUsers User { get; set; }
    }
}
