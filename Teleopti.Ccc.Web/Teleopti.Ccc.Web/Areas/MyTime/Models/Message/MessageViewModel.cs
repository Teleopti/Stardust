using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Teleopti.Ccc.UserTexts;
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
		public IList<string> ReplyOptions { get; set; }
		public int MessageType { get; set; }
	}

	public class ConfirmMessageViewModel
	{
		public Guid Id { get; set; }
		
		[StringLength(255, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "MessageTooLong")]
		public string Reply { get; set; }
		public string ReplyOption { get; set; }
	}

	public class DialogueMessageViewModel
	{
		public string Text { get; set; }
		public Guid SenderId { get; set; }
		public string Sender { get; set; }
		public string Created { get; set; }
	}
}