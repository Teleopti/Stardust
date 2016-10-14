using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class ScenarioController : ApiController
	{
		private readonly IScenarioRepository _scenarioRepository;

		public ScenarioController(IScenarioRepository scenarioRepository)
		{
			_scenarioRepository = scenarioRepository;
		}

		[UnitOfWork, Route("api/Global/Scenario"), HttpGet]
		public virtual ScenarioViewModel[] Scenarios()
		{
			var scenarios = _scenarioRepository.FindAllSorted();
			return scenarios.Select(x => new ScenarioViewModel
			{
				Id = x.Id.GetValueOrDefault(),
				Name = x.Description.Name,
				DefaultScenario = x.DefaultScenario
			}).ToArray();
		}
	}
}