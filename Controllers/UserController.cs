using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatinApp.API.Data;
using DatinApp.API.DTOs;
using DatinApp.API.Helpers;
using DatinApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatinApp.API.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(LogUserActivityFilter))]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IDatingRepository _repo;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            this._mapper = mapper;
            this._repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UsersPaginationParams paginationParams)
        {
            if (paginationParams.SkipCurrentUser)
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var userFromRepo = await this._repo.GetUser(currentUserId);

                paginationParams.UserId = currentUserId;

                if (string.IsNullOrEmpty(paginationParams.Gender))
                {
                    paginationParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
                }
            }

            var users = await this._repo.GetUsers(paginationParams);

            var usersResponse = this._mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage,
                                   users.PageSize,
                                   users.TotalRecords,
                                   users.TotalPages);

            return Ok(usersResponse);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await this._repo.GetUser(id);

            var userResponse = this._mapper.Map<UserForDetailDto>(user);

            return Ok(userResponse);
        }

        [HttpPost("{id}/like/{likeeId}")]
        public async Task<ActionResult> LikeUser(int id, int likeeId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var like = await this._repo.GetLike(id, likeeId);

            if (like != null)
            {
                return BadRequest("You already liked this user.");
            }

            if (await this._repo.GetUser(likeeId) == null)
            {
                return BadRequest($"User {id} couldn't be liked.");
            }

            like = new Models.Like
            {
                LikeeId = likeeId,
                LikerId = id
            };

            this._repo.Add<Like>(like);

            if (await this._repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest($"Failed to like user {likeeId}.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var userFromRepo = await this._repo.GetUser(id);

            this._mapper.Map(userForUpdateDto, userFromRepo);

            if (await this._repo.SaveAll())
            {
                return NoContent();
            }

            throw new System.Exception($"Updating user with {id} failed on save");
        }
    }
}
