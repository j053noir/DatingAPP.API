using System.Threading.Tasks;
using DatinApp.API.Data;
using DatinApp.API.DTOs;
using DatinApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatinApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
            this._repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // TODO: validate request

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await this._repo.UserExists(userForRegisterDto.Username))
            {
                return BadRequest("Username is already taken");
            }

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };

            var createdUser = await this._repo.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }
    }
}
