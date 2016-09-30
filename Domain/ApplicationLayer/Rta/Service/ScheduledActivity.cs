using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IScheduleReader
	{
		[RemoveMeWithToggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
		IEnumerable<ScheduledActivity> GetCurrentSchedule(DateTime utcNow, Guid personId);
		[RemoveMeWithToggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
		IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<Guid> personIds);

		IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<PersonBusinessUnit> persons);
	}

	public class PersonBusinessUnit
	{
		public Guid PersonId;
		public Guid BusinessUnitId;
	}

	[RemoveMeWithToggle(Toggles.RTA_FasterUpdateOfScheduleChanges_40536)]
	public class FromReadModel : IScheduleReader
	{
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjection;

		public FromReadModel(IScheduleProjectionReadOnlyPersister scheduleProjection)
		{
			_scheduleProjection = scheduleProjection;
		}

		public IEnumerable<ScheduledActivity> GetCurrentSchedule(DateTime utcNow, Guid personId)
		{
			var from = new DateOnly(utcNow.Date.AddDays(-1));
			var to = new DateOnly(utcNow.Date.AddDays(1));
			return _scheduleProjection.ForPerson(from, to, personId);
		}

		public IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<Guid> personIds)
		{
			var from = new DateOnly(utcNow.Date.AddDays(-1));
			var to = new DateOnly(utcNow.Date.AddDays(1));
			return _scheduleProjection.ForPersons(from, to, personIds);
		}

		public IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<PersonBusinessUnit> persons)
		{
			return GetCurrentSchedules(utcNow, persons.Select(x => x.PersonId).ToArray());
		}
	}

	public class FromPersonAssignment : IScheduleReader
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarios;
		private readonly IBusinessUnitRepository _businessUnits;

		public FromPersonAssignment(
			IScheduleStorage scheduleStorage, 
			IPersonRepository personRepository, 
			IScenarioRepository scenarios,
			IBusinessUnitRepository businessUnits)
		{
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_scenarios = scenarios;
			_businessUnits = businessUnits;
		}

		public IEnumerable<ScheduledActivity> GetCurrentSchedule(DateTime utcNow, Guid personId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<Guid> personIds)
		{
			throw new NotImplementedException();
		}

		[LogInfo]
		[FullPermissions]
		public virtual IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<PersonBusinessUnit> persons)
		{
			var from = new DateOnly(utcNow.Date.AddDays(-1));
			var to = new DateOnly(utcNow.Date.AddDays(1));
			return persons
				.GroupBy(x => x.BusinessUnitId, x => x.PersonId)
				.SelectMany(x =>
				{
					var scenario = _scenarios.LoadDefaultScenario(_businessUnits.Load(x.Key));
					var people = x.Select(id => _personRepository.Load(id));
					return MakeScheduledActivities(
							scenario,
							_scheduleStorage,
							people,
							new DateOnlyPeriod(from.AddDays(-1), to.AddDays(1))
						)
						.Where(a => a.BelongsToDate >= from && a.BelongsToDate <= to)
						.ToArray();

				});
		}

		public static IEnumerable<ScheduledActivity> MakeScheduledActivities(
			IScenario scenario,
			IScheduleStorage scheduleStorage,
			IEnumerable<IPerson> people,
			DateOnlyPeriod period)
		{
			if (scenario == null)
				return Enumerable.Empty<ScheduledActivity>();
			
			var schedules = scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				people,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				scenario);

			return (
				from person in people
				from scheduleDay in schedules[person].ScheduledDayCollection(period)
				let date = scheduleDay.DateOnlyAsPeriod.DateOnly
				from layer in scheduleDay.ProjectionService().CreateProjection()
				select new ScheduledActivity
				{
					BelongsToDate = date,
					DisplayColor = layer.DisplayColor().ToArgb(),
					EndDateTime = layer.Period.EndDateTime,
					Name = layer.DisplayDescription().Name,
					PayloadId = layer.Payload.Id.Value,
					PersonId = person.Id.Value,
					ShortName = layer.DisplayDescription().ShortName,
					StartDateTime = layer.Period.StartDateTime
				})
				.ToArray();
		}
	}

	public static class ScheduledActivityExtensions
	{
		public static int CheckSum(this IEnumerable<ScheduledActivity> activities)
		{
			unchecked
			{
				return activities.Aggregate(0, (cs, a) => cs*31 + a.CheckSum());
			}
		}
	}

	public class ScheduledActivity
    {
		public Guid PersonId { get; set; }
		public Guid PayloadId { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int DisplayColor { get; set; }

		public Color TheColor()
        {
            return Color.FromArgb(DisplayColor);
        }

        public DateTimePeriod Period()
        {
            return new DateTimePeriod(StartDateTime, EndDateTime);
        }




		public int CheckSum()
		{
			unchecked
			{
				var hashCode = PayloadId.GetHashCode();
				hashCode = (hashCode * 397) ^ BelongsToDate.GetHashCode();
				hashCode = (hashCode * 397) ^ StartDateTime.GetHashCode();
				hashCode = (hashCode * 397) ^ EndDateTime.GetHashCode();
				hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (ShortName?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ DisplayColor;
				return hashCode;
			}
		}
		
    }
}