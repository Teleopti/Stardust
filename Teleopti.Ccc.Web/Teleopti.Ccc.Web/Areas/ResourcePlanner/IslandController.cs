using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class IslandController : ApiController
	{
		private readonly IslandModelFactory _islandModelFactory;
		private readonly ISkillRepository _skillRepository;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public IslandController(IslandModelFactory islandModelFactory, ISkillRepository skillRepository, ICurrentUnitOfWork currentUnitOfWork)
		{
			_islandModelFactory = islandModelFactory;
			_skillRepository = skillRepository;
			_currentUnitOfWork = currentUnitOfWork;
		}

		[UnitOfWork]
		[HttpGet, Route("api/ResourcePlanner/Islands")]
		public virtual IHttpActionResult Islands()
		{
			_skillRepository.LoadAllSkills();//hack! perf - avoid billions of proxy calls on skills. Leave it for now until we use this "for real"
			var res =  Json(_islandModelFactory.Create());
			_currentUnitOfWork.Current().Clear(); //hack! perf - avoid finding dirty entities. Maybe new UnitOfWork-attr instead? Leave it for now until we use this "for real"
			return res;
		}
	}
}