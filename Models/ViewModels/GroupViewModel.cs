namespace Qwiz.Models
{
    public class GroupViewModel
    {
        public GroupViewModel() {}

        public GroupViewModel(Group group, string roleText, int? role, int membersCount, bool waitingRequest)
        {
            Group = group;
            RoleText = roleText;
            MembersCount = membersCount;
            Role = role;
            WaitingRequest = waitingRequest;
        }
        
        public Group Group { get; set; }
        public string RoleText { get; set; }
        public int? Role { get; set; }
        public bool WaitingRequest { get; set; }
        public int MembersCount { get; set; }
    }
}