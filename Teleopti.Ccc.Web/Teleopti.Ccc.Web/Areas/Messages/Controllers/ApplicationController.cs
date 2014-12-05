using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Messages.Controllers
{
    public class ApplicationController : Controller
    {
	    private readonly IPersonRepository _personRepository;

	    public ApplicationController(IPersonRepository personRepository)
	    {
		    _personRepository = personRepository;
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

		[HttpGet,OutputCache(Duration = 0,NoStore = true)]
		public ActionResult Resources()
		{
            var path = Request.MapPath("~/Areas/Messages/Content/Translation/TranslationTemplate.txt");
			var template = System.IO.File.ReadAllText(path);

			var userTexts = JsonConvert.SerializeObject(new
			{
				UserTexts.Resources.Messages,
				UserTexts.Resources.Receivers
			}, Formatting.Indented);

			template = string.Format(template, userTexts);

			return new ContentResult { Content = template, ContentType = "text/javascript" };
		}
    }

	public class PersonViewModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
	}

	public class SendMessageViewModel
	{
		public IEnumerable<PersonViewModel> People { get; set; }
	}
}
