using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IPersonScheduleDayReadModelsCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		IList<PersonScheduleDayReadModel> GetReadModels(IScenario scenario, DateTimePeriod period, IPerson person);

		void SetInitialLoad(bool initialLoad);
	}

	public class PersonScheduleDayReadModelsCreator : IPersonScheduleDayReadModelsCreator
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonScheduleDayReadModelCreator _readModelCreator;
		private bool _initialLoad;

		public PersonScheduleDayReadModelsCreator(IScheduleRepository scheduleRepository, IPersonScheduleDayReadModelCreator readModelCreator)
		{
			_scheduleRepository = scheduleRepository;
			_readModelCreator = readModelCreator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IList<PersonScheduleDayReadModel> GetReadModels(IScenario scenario, DateTimePeriod period, IPerson person)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
			var schedule =
				_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] { person }) { DoLoadByPerson = true },
																   new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod.ToDateTimePeriod(timeZone), scenario);
			var ret = new List<PersonScheduleDayReadModel>();

			var range = schedule[person];

			DateTimePeriod? actualPeriod;
			if (_initialLoad)
			{
				actualPeriod = range.TotalPeriod();
			}
			else
			{
				actualPeriod = period;

			}

			if (!actualPeriod.HasValue) return ret;

			dateOnlyPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
			foreach (var scheduleDay in range.ScheduledDayCollection(dateOnlyPeriod))
			{
				if (scheduleDay.IsScheduled())
					ret.Add(_readModelCreator.TurnScheduleToModel(scheduleDay));
			}
			return ret;
		}

		public void SetInitialLoad(bool initialLoad)
		{
			_initialLoad = initialLoad;
		}
	}

	public interface IScheduleDayReadModelsCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		IList<ScheduleDayReadModel> GetReadModels(IScenario scenario, DateTimePeriod period, IPerson person);

		void SetInitialLoad(bool initialLoad);
	}

	public class ScheduleDayReadModelsCreator : IScheduleDayReadModelsCreator
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IScheduleDayReadModelCreator _readModelCreator;
		private bool _initialLoad;

		public ScheduleDayReadModelsCreator(IScheduleRepository scheduleRepository, IScheduleDayReadModelCreator readModelCreator)
		{
			_scheduleRepository = scheduleRepository;
			_readModelCreator = readModelCreator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IList<ScheduleDayReadModel> GetReadModels(IScenario scenario, DateTimePeriod period, IPerson person)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
			var schedule =
				_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] { person }) { DoLoadByPerson = true },
																   new ScheduleDictionaryLoadOptions(false, false), dateOnlyPeriod.ToDateTimePeriod(timeZone), scenario);
			var ret = new List<ScheduleDayReadModel>();

			var range = schedule[person];

			DateTimePeriod? actualPeriod;
			if (_initialLoad)
			{
				actualPeriod = range.TotalPeriod();
			}
			else
			{
				actualPeriod = period;

			}

			if (!actualPeriod.HasValue) return ret;

			dateOnlyPeriod = actualPeriod.Value.ToDateOnlyPeriod(timeZone);
			foreach (var scheduleDay in range.ScheduledDayCollection(dateOnlyPeriod))
			{
				if(scheduleDay.IsScheduled())
					ret.Add(_readModelCreator.TurnScheduleToModel(scheduleDay));
			}
			return ret;
		}

		public void SetInitialLoad(bool initialLoad)
		{
			_initialLoad = initialLoad;
		}
	}
}