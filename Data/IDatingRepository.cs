using System.Collections.Generic;
using System.Threading.Tasks;
using DatinApp.API.Helpers;
using DatinApp.API.Models;

namespace DatinApp.API.Data
{
    public interface IDatingRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<Photo> GetMainPhotoForUser(int userId);
        Task<Photo> GetPhoto(int id);
        Task<User> GetUser(int id);
        Task<PagedList<User>> GetUsers(UsersPaginationParams paginationParams);
        Task<bool> SaveAll();
    }
}
