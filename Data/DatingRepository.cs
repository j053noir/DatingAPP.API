using System;
using System.Collections.Generic;
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

        public async Task<Like> GetLike(int likerId, int likeeId)
        {
            return await this._context.Likes
                            .FirstOrDefaultAsync(u => u.LikerId == likerId && u.LikeeId == likeeId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            var photo = await this._context.Photos
                                    .FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);

            return photo;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this._context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            throw new NotImplementedException();
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

            if (paginationParams.UserId != null && paginationParams.Likers)
            {
                var userLikers = await GetUserLikers(paginationParams.UserId.Value);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            else if (paginationParams.UserId != null && paginationParams.Likees)
            {
                var userLikees = await GetUserLikees(paginationParams.UserId.Value);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            switch (paginationParams.OrderBy)
            {
                case "created":
                    users = paginationParams.OrderDirection == "descending" ?
                             users.OrderByDescending(u => u.Created) :
                             users.OrderBy(u => u.Created);
                    break;
                case "known_as":
                    users = paginationParams.OrderDirection == "descending" ?
                             users.OrderByDescending(u => u.KnownAs) :
                             users.OrderBy(u => u.KnownAs);
                    break;
                case "username":
                    users = paginationParams.OrderDirection == "descending" ?
                             users.OrderByDescending(u => u.Username) :
                             users.OrderBy(u => u.Username);
                    break;
                default:
                    users = paginationParams.OrderDirection == "descending" ?
                             users.OrderByDescending(u => u.LastActive) :
                             users.OrderBy(u => u.LastActive);
                    break;
            }

            return await PagedList<User>.CreateASync(users,
                                                     paginationParams.PageNumber,
                                                     paginationParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikers(int userId)
        {
            var user = await this._context.Users.Include(x => x.Likers)
                                                .Include(x => x.Likees)
                                                .FirstOrDefaultAsync(u => u.Id == userId);

            return user.Likers.Where(u => u.LikeeId == userId).Select(u => u.LikerId);
        }

        private async Task<IEnumerable<int>> GetUserLikees(int userId)
        {
            var user = await this._context.Users.Include(x => x.Likers)
                                                .Include(x => x.Likees)
                                                .FirstOrDefaultAsync(u => u.Id == userId);

            return user.Likees.Where(u => u.LikerId == userId).Select(u => u.LikeeId);
        }

        public async Task<bool> SaveAll()
        {
            return await this._context.SaveChangesAsync() > 0;
        }
    }
}
