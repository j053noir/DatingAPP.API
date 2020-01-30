using System;

namespace DatinApp.API.DTOs
{
    public class MessageForCreationDto
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public DateTime? MessageSent { get; set; }
        public string Content { get; set; }

        public MessageForCreationDto()
        {
            if (this.MessageSent == null)
            {
                this.MessageSent = DateTime.Now;
            }
        }
    }
}
