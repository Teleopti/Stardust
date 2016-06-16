using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	[EnabledBy(Toggles.Intraday_ResourceCalculateReadModel_39200)]
	public class ResourceCalculateReadModelUpdater : IHandleEvent<UpdateResourceCalculateReadModelEvent>, IRunOnStardust
	{
		private readonly CalculateForReadModel _calculateForReadModel;
		private IPersonRepository _personRepository;

		public ResourceCalculateReadModelUpdater(CalculateForReadModel calculateForReadModel, IPersonRepository personRepository)
		{
			_calculateForReadModel = calculateForReadModel;
			_personRepository = personRepository;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateResourceCalculateReadModelEvent @event)
		{
			//get the scheduled period or inside the method below
			//may be have another smarter way of loading all
			var allPeople = _personRepository.LoadAll();

			var maxPublishedDate =
				allPeople.Where(x => x.WorkflowControlSet != null)
					.Select(y => y.WorkflowControlSet)
					.Where(w => w.SchedulePublishedToDate != null)
					.Max(u => u.SchedulePublishedToDate);
			var publishedPeriod = new DateOnlyPeriod(DateOnly.Today, new DateOnly(maxPublishedDate.Value));

			foreach (var day in publishedPeriod.DayCollection())
			{
				var model = _calculateForReadModel.ResourceCalculatePeriod(new DateOnlyPeriod(day,day.AddDays(1)));
			}
			
			//save model to DB
		}
	}

	public class UpdateResourceCalculateReadModelEvent : EventWithInfrastructureContext
	{
		
	}
}
