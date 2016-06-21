using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SkillsController : ApiController
	{
		private readonly ISkillRepository _skillRepository;

		public SkillsController(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		[UnitOfWork, HttpGet, Route("api/Skills")]
		public virtual IHttpActionResult Index()
		{
			return Ok(_skillRepository.LoadAll());
		}
	}
}