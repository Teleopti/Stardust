using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
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
	    private readonly IPrincipalAuthorization _principalAuthorization;

	    public ApplicationController(IPersonRepository personRepository, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal, IPrincipalAuthorization principalAuthorization)
	    {
		    _personRepository = personRepository;
		    _currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		    _principalAuthorization = principalAuthorization;
	    }

	    [HttpGet]
	    public ViewResult Index()
	    {
			return View();
	    }

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult GetPersons(string ids)
	    {
			try
			{
				var personIds = ids.Split(',').Select(x => new Guid(x));
				var persons = _personRepository.FindPeople(personIds);
				var vm = new SendMessageViewModel
				{
					People = persons.Select(p => new PersonViewModel { Name = p.Name.ToString(), Id = p.Id.Value })
				};
				return Json(vm, JsonRequestBehavior.AllowGet);
			}
			catch (FormatException e)
			{
				throw new HttpException(400, e.Message);
			}
	    }

		[UnitOfWorkAction, HttpGet, OutputCache(NoStore = true, Duration = 0)]
		public JsonResult NavigationContent()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			return Json(new
			{
				UserName = principal.Identity.Name,
				PersonId = ((IUnsafePerson)principal).Person.Id,
				IsMyTimeAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb),
				IsAnywhereAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere),
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
				UserTexts.Resources.SignOut
			}, Formatting.Indented);

			template = string.Format(template, userTexts);

			return new ContentResult { Content = template, ContentType = "text/javascript" };
		}
    }
}
