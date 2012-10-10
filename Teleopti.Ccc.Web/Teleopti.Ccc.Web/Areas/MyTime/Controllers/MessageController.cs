using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class MessageController : Controller
	{
		private readonly IMessageViewModelFactory _messageViewModelFactory;
		private readonly IPushMessageDialoguePersister _pushMessageDialoguePersister;
		private readonly IPushMessageProvider _pushMessageProvider;

		public MessageController(IMessageViewModelFactory viewModelFactory, IPushMessageDialoguePersister pushMessageDialoguePersister, IPushMessageProvider pushMessageProvider)
		{
			_messageViewModelFactory = viewModelFactory;
			_pushMessageDialoguePersister = pushMessageDialoguePersister;
			_pushMessageProvider = pushMessageProvider;
		}

		[EnsureInPortal]
		public ViewResult Index()
		{
			return View("MessagePartial");
		}
		
		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult Messages(Paging paging)
		{
			return Json(_messageViewModelFactory.CreatePageViewModel(paging), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult Reply(MessageForm form)
		{
			return Json(_pushMessageDialoguePersister.Persist(form.MessageId));
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult MessagesCount()
		{
			return Json(new MessageInfo {UnreadMessagesCount = _pushMessageProvider.UnreadMessageCount}, JsonRequestBehavior.AllowGet);
		}
	}

	public class MessageForm
	{
		public string MessageId { get; set; }
	}

	public class MessageInfo
	{
		public int UnreadMessagesCount { get; set; }
	}
}