using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Message
{
    public class MessageViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        //public bool AllowDialogueReply { get; set; }
        //public bool TranslateMessage { get; set; }
        public string Sender { get; set; }
        public DateTime Date { get; set; }
    }
}