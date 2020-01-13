using System.Collections.Generic;
using System.Threading.Tasks;
using DatinApp.API.Models;

namespace DatinApp.API.Data
{
    public interface IDatingRepository
    {
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<User> GetUser(int Id);
        Task<IEnumerable<User>> GetUsers();
        Task<bool> SaveAll();
    }
}
