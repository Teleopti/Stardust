using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Messages.Models;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Messages.Controllers
{
    public class ApplicationController : Controller
    {
	    private readonly IPersonRepository _personRepository;
	    private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
	    private readonly IAuthorization _authorization;
	    private readonly INotifier _notifier;
	    private readonly ILicenseCustomerNameProvider _licenseCustomerNameProvider;
		
	    public ApplicationController(IPersonRepository personRepository, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IAuthorization authorization, INotifier notifier, ILicenseCustomerNameProvider licenseCustomerNameProvider)
	    {
		    _personRepository = personRepository;
		    _currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		    _authorization = authorization;
		    _notifier = notifier;
		    _licenseCustomerNameProvider = licenseCustomerNameProvider;
	    }

	    [HttpGet]
	    public ViewResult Index()
	    {
			return View();
	    }

		[UnitOfWork]
		[HttpGet]
		[License(DefinedLicenseOptionPaths.TeleoptiCccSmsLink)]
		public virtual JsonResult GetPersons(string ids)
	    {
			try
			{
				var personIds = ids.Split(',').Select(x => new Guid(x));
				var persons = _personRepository.FindPeople(personIds);
				var vm = new SendMessageViewModel
				{
					People = persons.Select(p => new PersonViewModel { Name = p.Name.ToString(), Id = p.Id.GetValueOrDefault() })
				};
				return Json(vm, JsonRequestBehavior.AllowGet);
			}
			catch (FormatException e)
			{
				throw new HttpException(400, e.Message);
			}
	    }

	    [UnitOfWork]
	    [HttpPost]
		[License(DefinedLicenseOptionPaths.TeleoptiCccSmsLink)]
		public virtual async Task<JsonResult> SendMessage(Guid[] receivers, string subject, string body)
	    {
			var persons = _personRepository.FindPeople(receivers).ToArray();
			INotificationMessage msg = new NotificationMessage
		    {
			    Subject = subject,
				CustomerName = _licenseCustomerNameProvider.GetLicenseCustomerName()
			};
		    msg.Messages.Add(body);
			await _notifier.Notify(msg, persons);
		    return Json("");
	    }

		[UnitOfWork, HttpGet, OutputCache(NoStore = true, Duration = 0)]
		public virtual JsonResult NavigationContent()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			return Json(new
			{
				UserName = principal.Identity.Name,
				PersonId = principal.PersonId,
				IsMyTimeAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb),
				IsAnywhereAvailable = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere)
			}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet,OutputCache(Duration = 0,NoStore = true)]
		public ActionResult Resources()
		{
            var path = Request.MapPath("~/Areas/Messages/Content/Translation/TranslationTemplate.txt");
			var template = System.IO.File.ReadAllText(path);

			var userTexts = JsonConvert.SerializeObject(new
			{
				UserTexts.Resources.Messages,
				UserTexts.Resources.Receivers,
				UserTexts.Resources.Send,
				UserTexts.Resources.SignOut,
				UserTexts.Resources.Close,
				UserTexts.Resources.Subject,
				UserTexts.Resources.Message,
				UserTexts.Resources.StatusColon,
				UserTexts.Resources.Sent,
				UserTexts.Resources.Pending,
				UserTexts.Resources.InsufficientPermission,
				UserTexts.Resources.TooManySelectedAgents

			}, Formatting.Indented);

			template = string.Format(template, userTexts);

			return new ContentResult { Content = template, ContentType = "text/javascript" };
		}
    }

	
}
