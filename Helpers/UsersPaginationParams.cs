namespace DatinApp.API.Helpers
{
    public class UsersPaginationParams : PaginationParams
    {
        public bool SkipCurrentUser { get; set; } = true;
        public int? UserId { get; set; }
        public string Gender { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
    }
}
