using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
    public class MessageController : Controller
    {
        private readonly IMessageViewModelFactory _messageViewModelFactory;

        public MessageController(IMessageViewModelFactory viewModelFactory)
        {
            _messageViewModelFactory = viewModelFactory;
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
    }
}