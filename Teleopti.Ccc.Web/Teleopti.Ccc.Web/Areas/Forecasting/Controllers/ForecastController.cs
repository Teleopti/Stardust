using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class ForecastController : ApiController
	{
		private readonly IQuickForecastEvaluator _quickForecastEvaluator;
		private readonly ISkillRepository _skillRepository;
		private readonly IQuickForecastCreator _quickForecastCreator;

		public ForecastController(IQuickForecastEvaluator quickForecastEvaluator, ISkillRepository skillRepository, IQuickForecastCreator quickForecastCreator)
		{
			_quickForecastEvaluator = quickForecastEvaluator;
			_skillRepository = skillRepository;
			_quickForecastCreator = quickForecastCreator;
		}

		[HttpGet, Route("api/Forecasting/MeasureForecast"), UnitOfWork]
		public virtual Task<ForecastingAccuracy[]> MeasureForecast()
		{
			return Task.FromResult(_quickForecastEvaluator.MeasureForecastForAllSkills());
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

		[HttpPost, Route("api/Forecasting/Forecast"), UnitOfWork]
		public virtual Task<bool> Forecast(QuickForecastInputModel model)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			_quickForecastCreator.CreateForecastForWorkloads(futurePeriod, model.Workloads);
			return Task.FromResult(true);
		}
	}
}