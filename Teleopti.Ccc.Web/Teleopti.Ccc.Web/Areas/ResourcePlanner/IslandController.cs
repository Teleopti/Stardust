using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class IslandController : ApiController
	{
		private readonly IslandModelFactory _islandModelFactory;
		private readonly ISkillRepository _skillRepository;

		public IslandController(IslandModelFactory islandModelFactory, ISkillRepository skillRepository)
		{
			_islandModelFactory = islandModelFactory;
			_skillRepository = skillRepository;
		}

		[UnitOfWork]
		[HttpGet, Route("api/ResourcePlanner/Islands")]
		public virtual IHttpActionResult Islands()
		{
			_skillRepository.LoadAllSkills();//perf - avoid billions of proxy calls on skills
			return Json(_islandModelFactory.Create());
		}
	}
}