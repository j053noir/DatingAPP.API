namespace DatinApp.API.Helpers
{
    public class MessagePaginationParams : PaginationParams
    {
        public int UserId { get; set; }
        public string MessageContainer { get; set; } = "Unread";
    }
}
