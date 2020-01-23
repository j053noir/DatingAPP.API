namespace DatinApp.API.Helpers
{
    public class UsersPaginationParams : PaginationParams
    {
        public int? UserId { get; set; }
        public string Gender { get; set; }
    }
}
