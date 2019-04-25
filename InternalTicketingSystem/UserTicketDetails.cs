using System;

namespace InternalTicketingSystem
{
    class UserTicketDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IssueHeader { get; set; }
        public DateTime Date { get; set; }
        public string IssueDescription { get; set; }
        public int TicketID { get; set; }
        public int UserID { get; set; }
        public int TicketClosedFlag { get; set; }
    }
}
