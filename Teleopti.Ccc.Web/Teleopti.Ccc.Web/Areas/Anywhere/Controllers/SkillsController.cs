using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SkillsController : ApiController
	{
		private readonly SkillViewModelBuilder _viewModelBuilder;

		public SkillsController(SkillViewModelBuilder viewModelBuilder)
		{
			_viewModelBuilder = viewModelBuilder;
		}

		[UnitOfWork, HttpGet, Route("api/Skills")]
		public virtual IHttpActionResult Index()
		{
			return Ok(_viewModelBuilder.Build());
		}
	}
}