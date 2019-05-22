using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Qwiz.Models
{
    public class Group
    {
        public Group() {}
        public Group(string name, ApplicationUser owner, string region, bool isPublic)
        {
            Name = name;
            Owner = owner;
            Region = region;
            IsPublic = isPublic;
            OwnerUsername = owner.UserName;
            
            Members.Add(new GroupMember(0, owner, this));
        }
        
        public int Id { get; set; }
        [Required]
        [MaxLength(64)]
        public string Name { get; set; }
        [Required]
        [MaxLength(64)]
        public string Region { get; set; }
        [Required]
        public string OwnerUsername { get; set; }
        [Required]
        public bool IsPublic { get; set; }
        [Required]
        public ApplicationUser Owner { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();
        public List<PendingMember> PendingInvites { get; set; } = new List<PendingMember>();
    }
}