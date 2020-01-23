using System;
using System.Linq;
using System.Threading.Tasks;
using DatinApp.API.Helpers;
using DatinApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatinApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            this._context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            this._context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            this._context.Remove(entity);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            var photo = await this._context.Photos
                                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);

            return photo;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await this._context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await this._context.Users
                            .Include(u => u.Photos)
                            .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UsersPaginationParams paginationParams)
        {
            var users = this._context.Users.Include(u => u.Photos).AsQueryable();

            if (paginationParams.UserId != null)
            {
                users = users.Where(u => u.Id != paginationParams.UserId);
            }

            if (!string.IsNullOrEmpty(paginationParams.Gender))
            {
                users = users.Where(u => u.Gender == paginationParams.Gender);
            }

            if (paginationParams.MaxAge.HasValue && paginationParams.MaxAge.Value >= 0)
            {
                var minDob = DateTime.Today.AddYears(-paginationParams.MaxAge.Value - 1);
                users = users.Where(u => u.DateOfBirth >= minDob);
            }

            if (paginationParams.MinAge.HasValue && paginationParams.MinAge.Value >= 0)
            {
                var maxDbo = DateTime.Today.AddYears(-paginationParams.MinAge.Value);
                users = users.Where(u => u.DateOfBirth <= maxDbo);
            }

            return await PagedList<User>.CreateASync(users,
                                                     paginationParams.PageNumber,
                                                     paginationParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await this._context.SaveChangesAsync() > 0;
        }
    }
}
