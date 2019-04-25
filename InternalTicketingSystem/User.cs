namespace InternalTicketingSystem
{
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Salt { get; set; }
        public string Password { get; set; }
        public int UserID { get; set; }
        public int AdminFlag { get; set; }
    }
}
