using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExistingForecastRepository : IExistingForecastRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ExistingForecastRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<Tuple<string, IEnumerable<DateOnlyPeriod>>> ExistingForecastForAllSkills(DateOnlyPeriod range,
			IScenario scenario)
		{
			var existingForecastPerSkills = session(_currentUnitOfWork.Current()).GetNamedQuery("ExistingForecast")
				.SetEntity("scenario", scenario)
				.SetEntity("businessUnit", CurrentBusinessUnit.InstanceForEntities.Current())
				.SetDateOnly("startDate", range.StartDate)
				.SetDateOnly("endDate", range.EndDate)
				.SetString("longtermKey", TemplateReference.LongtermTemplateKey)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(ExistingForecastPerSkill)))
				.List<ExistingForecastPerSkill>();

			var datesToPeriod = new DatesToPeriod();
			var group = existingForecastPerSkills.GroupBy(x => new {x.Name, x.Id});
			return
				group.Select(
					x =>
						new Tuple<string, IEnumerable<DateOnlyPeriod>>(x.Key.Name,
							datesToPeriod.Convert(x.Where(d => d.CurrentDate.HasValue).Select(y => new DateOnly(y.CurrentDate.Value)))));
		}

		private static ISession session(IUnitOfWork uow)
		{
			return ((NHibernateUnitOfWork) uow).Session;
		}
	}

	class ExistingForecastPerSkill
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public DateTime? CurrentDate { get; set; }
	}
}
