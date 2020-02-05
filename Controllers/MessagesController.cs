using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IDatingRepository _repo;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            this._mapper = mapper;
            this._repo = repo;
        }

        [HttpGet("", Name = "GetMessages")]
        public async Task<IActionResult> GetMesagesForUser(
            int userId,
            [FromQuery] MessagePaginationParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            messageParams.UserId = userId;

            var messagesFromRepo = await this._repo.GetMessagesForUser(messageParams);

            var messages = this._mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage,
                                   messagesFromRepo.PageSize,
                                   messagesFromRepo.TotalRecords,
                                   messagesFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMesage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var messageFromRepo = await this._repo.GetMessage(id);

            if (messageFromRepo == null)
            {
                return NotFound();
            }

            return Ok(messageFromRepo);
        }

        [HttpGet("threads/{recipientId}", Name = "GetMessageThread")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var messagesFromRepo = await this._repo.GetMessageThread(userId, recipientId);

            foreach (var message in messagesFromRepo.Where(m => m.IsRead == false &&
                                                                m.RecipientId == userId))
            {
                message.IsRead = true;

            }

            await this._repo.SaveAll();

            var messageThread = this._mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            return Ok(messageThread);
        }

        [HttpPost(Name = "CreateMessage")]
        public async Task<IActionResult> CreateMesage(int userId,
                                                      MessageForCreationDto messageForCreationDto)
        {
            var sender = await this._repo.GetUser(userId);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            messageForCreationDto.SenderId = userId;

            var recipient = await this._repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
            {
                return BadRequest("Couldn't find recipient user");
            }

            var message = this._mapper.Map<Message>(messageForCreationDto);

            this._repo.Add(message);

            if (await this._repo.SaveAll())
            {
                var messageToReturn = this._mapper.Map<MessageToReturnDto>(message);

                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);
            }

            return BadRequest("Failed on saving message");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var messageFromRepo = await this._repo.GetMessage(id);

            if (messageFromRepo.SenderId != userId && messageFromRepo.RecipientId != userId)
            {
                return Forbid();
            }

            if (messageFromRepo.SenderId == userId)
            {
                messageFromRepo.SenderDeleted = true;
            }
            else if (messageFromRepo.RecipientId == userId)
            {
                messageFromRepo.RecipientDeleted = true;
            }

            if (messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
            {
                this._repo.Delete(messageFromRepo);
            }

            if (await this._repo.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Message couldn't be deleted.");
        }

        [HttpGet("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid();
            }

            var message = await this._repo.GetMessage(id);

            if (message.RecipientId != userId)
            {
                return Forbid();
            }

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            if (await this._repo.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Could not mark message as read.");
        }
    }
}
