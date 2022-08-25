namespace UserPortal.Dtos
{
    public class UserActivisionDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; } = false;
        public string Event { get; set; }
    }
}
