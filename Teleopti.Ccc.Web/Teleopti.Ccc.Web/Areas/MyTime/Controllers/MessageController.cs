using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core;

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
		[UnitOfWork]
		public virtual JsonResult Messages(Paging paging)
		{
			return Json(_messageViewModelFactory.CreatePageViewModel(paging), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPostOrPut]
		public virtual JsonResult Reply(ConfirmMessageViewModel messageViewModel)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_pushMessageDialoguePersister.PersistMessage(messageViewModel));
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult MessagesCount()
		{
			return Json(new MessagesInformationViewModel {UnreadMessagesCount = _pushMessageProvider.UnreadMessageCount}, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpGet]
		public virtual JsonResult Message(Guid messageId)
		{
			return Json(_messageViewModelFactory.CreateMessagesInformationViewModel(messageId), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		public virtual void Send(string title, string message)
		{
			_pushMessageDialoguePersister.SendNewPushMessageToLoggedOnUser(title,message);
		}
	}
}