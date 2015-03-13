using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class ForecastController : ApiController
	{
		private readonly IQuickForecastCreator _quickForecastCreator;
		private readonly ICurrentIdentity _currentIdentity;
		private readonly ISkillRepository _skillRepository;

		public ForecastController(IQuickForecastCreator quickForecastCreator, ICurrentIdentity currentIdentity, ISkillRepository skillRepository)
		{
			_quickForecastCreator = quickForecastCreator;
			_currentIdentity = currentIdentity;
			_skillRepository = skillRepository;
		}

		[HttpPost, Route("api/Forecasting/MeasureForecast"), UnitOfWork]
		public virtual Task<ForecastingAccuracy[]> MeasureForecast([FromBody] QuickForecastInputModel model)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			return Task.FromResult(_quickForecastCreator.MeasureForecastForAllSkills(futurePeriod));
		}

		[UnitOfWork, Route("api/Forecasting/Skills"), HttpGet]
		public virtual IEnumerable<SkillViewModel> Skills()
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			return skills.Select(
				skill => new SkillViewModel
				{
					Id = skill.Id.Value,
					Name = skill.Name,
					Workloads = skill.WorkloadCollection.Select(x => new WorkloadViewModel {Id = x.Id.Value, Name = x.Name}).ToArray()
				});
		}
	}

	public class SkillViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public WorkloadViewModel[] Workloads { get; set; }
	}

	public class WorkloadViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}