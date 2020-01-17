using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatinApp.API.Data;
using DatinApp.API.DTOs;
using DatinApp.API.Helpers;
using DatinApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatinApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repository,
                                IMapper mapper,
                                IOptions<CloudinarySettings> cloudinaryConfig)
        {
            this._cloudinaryConfig = cloudinaryConfig;
            this._mapper = mapper;
            this._repo = repository;


            Account accCloudinary = new Account(
                this._cloudinaryConfig.Value.CloudName,
                this._cloudinaryConfig.Value.ApiKey,
                this._cloudinaryConfig.Value.ApiSecret
            );

            this._cloudinary = new Cloudinary(accCloudinary);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await this._repo.GetPhoto(id);

            var photo = this._mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId,
                                                         [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var userFromRepo = await this._repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResults = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500)
                        .Height(500)
                        .Crop("fill")
                        .Gravity("face")
                    };

                    uploadResults = this._cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResults.Uri.ToString();
            photoForCreationDto.PublicId = uploadResults.PublicId;

            var photo = this._mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);

            if (await this._repo.SaveAll())
            {
                var photoToReturn = this._mapper.Map<PhotoForReturnDto>(photo);

                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Couldn't not add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var userFromRepo = await this._repo.GetUser(userId);

            if (!userFromRepo.Photos.Any(p => p.Id == id))
            {
                return Forbid();
            }

            var photoFromRepo = await this._repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
            {
                return BadRequest("The selected photo is already set as main.");
            }

            var curretMainPhoto = await this._repo.GetMainPhotoForUser(userId);

            curretMainPhoto.IsMain = false;
            photoFromRepo.IsMain = true;

            if (await this._repo.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Couldn't set selected photo as main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var userFromRepo = await this._repo.GetUser(userId);

            if (!userFromRepo.Photos.Any(p => p.Id == id))
            {
                return Forbid();
            }

            var photoFromRepo = await this._repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
            {
                return BadRequest("You cannot delete your main photo.");
            }

            if (!string.IsNullOrEmpty(photoFromRepo.PublicId))
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                var result = this._cloudinary.Destroy(deleteParams);

                if (result.Result.ToLower() == "ok")
                {
                    this._repo.Delete(photoFromRepo);
                }
            }
            else
            {
                this._repo.Delete(photoFromRepo);
            }

            if (await this._repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Couldn't delete the photo");
        }
    }
}
