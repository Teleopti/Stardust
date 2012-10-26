using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
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
		
		[HttpGet]
		[UnitOfWorkAction]
		public JsonResult Messages(Paging paging)
		{
			return Json(_messageViewModelFactory.CreatePageViewModel(paging), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpPostOrPut]
		public JsonResult Reply(ConfirmMessageViewModel messageViewModel)
		{
			return Json(_pushMessageDialoguePersister.PersistMessage(messageViewModel));
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult MessagesCount()
		{
			return Json(new MessagesInformationViewModel {UnreadMessagesCount = _pushMessageProvider.UnreadMessageCount}, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult Message(Guid messageId)
		{
			return Json(_messageViewModelFactory.CreateMessagesInformationViewModel(messageId), JsonRequestBehavior.AllowGet);
		}
	}
}