using System;

namespace Qwiz.Models
{
    public class GroupMember
    {
        public GroupMember() {}
        public GroupMember(int role, ApplicationUser user, Group group)
        {
            Role = role;
            Member = user;
            Username = user.UserName;
            Group = group;

            if (role == 0)      RoleText = "Owner";
            else if (role == 1) RoleText = "Admin";
            else                RoleText = "Member";
            
            user.MyGroups.Add(group);
        }
        
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public int Role { get; set; }
        public string RoleText { get; set; }
        public string Username { get; set; }
        public ApplicationUser Member { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.Now;
    }
}