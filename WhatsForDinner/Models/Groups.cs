using System;
using System.Collections.Generic;

namespace WhatsForDinner.Models
{
    public partial class Groups
    {
        public Groups()
        {
            GroupInvite = new HashSet<GroupInvite>();
            UserGroups = new HashSet<UserGroups>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public virtual ICollection<GroupInvite> GroupInvite { get; set; }
        public virtual ICollection<UserGroups> UserGroups { get; set; }
    }
}
