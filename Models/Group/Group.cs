using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Qwiz.Models
{
    public class Group
    {
        public Group() {}
        public Group(string name, ApplicationUser owner, string region, bool isPublic, bool requiresDomain)
        {
            Name = name;
            Owner = owner;
            Region = region;
            IsPublic = isPublic;
            RequiresDomain = requiresDomain;
            OwnerUsername = owner.UserName;
            
            Members.Add(new GroupMember(0, owner, this));
            if (requiresDomain) RequiredDomain = owner.Email.Split("@")[1];
        }
        
        public int Id { get; set; }
        [Required]
        [MaxLength(64), MinLength(3)]
        public string Name { get; set; }
        [Required]
        [MaxLength(64), MinLength(3)]
        public string Region { get; set; }
        public string OwnerUsername { get; set; }
        [Required]
        public bool IsPublic { get; set; }
        // TODO: Needs regex magic to confirm the string to be domain
        [Required]
        public bool RequiresDomain { get; set; }
        [StringLength(64), MinLength(3)]
        public string RequiredDomain { get; set; }
        public ApplicationUser Owner { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();
        public List<PendingMember> PendingInvites { get; set; } = new List<PendingMember>();
    }
}