using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExistingForecastRepository : IExistingForecastRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ExistingForecastRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<SkillMissingForecast> ExistingForecastForAllSkills(DateOnlyPeriod range,
			IScenario scenario)
		{
			var existingForecastPerSkills = _currentUnitOfWork.Current().Session().GetNamedQuery("ExistingForecast")
				.SetEntity("scenario", scenario)
				.SetEntity("businessUnit", scenario.GetOrFillWithBusinessUnit_DONTUSE())
				.SetDateOnly("startDate", range.StartDate)
				.SetDateOnly("endDate", range.EndDate)
				.SetString("longtermKey", TemplateReference.LongtermTemplateKey)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(ExistingForecastPerSkill)))
				.List<ExistingForecastPerSkill>();

			var datesToPeriod = new DatesToPeriod();
			var group = existingForecastPerSkills.GroupBy(x => new {x.Name, x.Id});
			return group.Select(x => new SkillMissingForecast
			{
				SkillName = x.Key.Name,
				SkillId = x.Key.Id,
				Periods = datesToPeriod.Convert(x.Where(d => d.CurrentDate.HasValue).Select(y => new DateOnly(y.CurrentDate.Value)))
			});
		}
	}
	
	class ExistingForecastPerSkill
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public DateTime? CurrentDate { get; set; }
	}
}
