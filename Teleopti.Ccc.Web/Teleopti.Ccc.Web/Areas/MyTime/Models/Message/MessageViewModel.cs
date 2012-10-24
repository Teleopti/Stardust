using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Message
{
    public class MessageViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Date { get; set; }
        public string MessageId { get; set; }
        public bool IsRead { get; set; }
		public bool AllowDialogueReply { get; set; }
		public IList<DialogueMessageViewModel> DialogueMessages { get; set; }
    }

	public class DialogueMessageViewModel
	{
		public string Text { get; set; }
	}
}