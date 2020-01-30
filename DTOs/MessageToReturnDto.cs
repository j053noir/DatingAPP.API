using System;

namespace DatinApp.API.DTOs
{
    public class MessageToReturnDto
    {
        public int Id { get; set; }
        public UserForReturnDto Sender { get; set; }
        public UserForReturnDto Recipient { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime DateSent { get; set; }
    }
}
