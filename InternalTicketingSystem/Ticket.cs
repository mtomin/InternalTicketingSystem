using System;

namespace InternalTicketingSystem
{
    class Ticket
    {
        public int TicketID { get; set; }
        public int UserID { get; set; }
        public string IssueDescription { get; set; }
        public DateTime Date { get; set; }
        public string IssueHeader { get; set; }
        public int TicketClosedFlag { get; set; }
    }
}
