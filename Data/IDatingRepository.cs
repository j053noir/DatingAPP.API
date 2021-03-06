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
        Task<Like> GetLike(int userId, int recipientId);
        Task<Photo> GetMainPhotoForUser(int userId);
        Task<Message> GetMessage(int id);
        Task<PagedList<Message>> GetMessagesForUser(MessagePaginationParams messageParams);
        Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId);
        Task<Photo> GetPhoto(int id);
        Task<User> GetUser(int id);
        Task<PagedList<User>> GetUsers(UsersPaginationParams paginationParams);
        Task<bool> SaveAll();
    }
}
