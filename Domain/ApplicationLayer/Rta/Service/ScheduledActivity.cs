using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IScheduleReader
	{
		IEnumerable<ScheduledActivity> GetCurrentSchedule(DateTime utcNow, Guid personId);
		IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<Guid> personIds);
	}

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

	}

	public class FromPersonAssignment : IScheduleReader
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentScenario _currentScenario;

		public FromPersonAssignment(
			IScheduleStorage scheduleStorage, 
			IPersonRepository personRepository, 
			ICurrentScenario currentScenario)
		{
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_currentScenario = currentScenario;
		}

		[LogInfo]
		[FullPermissions]
		public virtual IEnumerable<ScheduledActivity> GetCurrentSchedule(DateTime utcNow, Guid personId)
		{
			var from = new DateOnly(utcNow.Date.AddDays(-1));
			var to = new DateOnly(utcNow.Date.AddDays(1));
			return MakeScheduledActivities(
				_currentScenario,
				_scheduleStorage,
				new[] {_personRepository.Get(personId)},
				new DateOnlyPeriod(from.AddDays(-1), to.AddDays(1))
				)
				.Where(x => x.BelongsToDate >= from && x.BelongsToDate <= to)
				.ToArray();
		}

		[LogInfo]
		[FullPermissions]
		public virtual IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<Guid> personIds)
		{
			var from = new DateOnly(utcNow.Date.AddDays(-1));
			var to = new DateOnly(utcNow.Date.AddDays(1));
			var persons = personIds.Select(x => _personRepository.Get(x));
			return MakeScheduledActivities(
				_currentScenario,
				_scheduleStorage,
				persons,
				new DateOnlyPeriod(from.AddDays(-1), to.AddDays(1))
				)
				.Where(x => x.BelongsToDate >= from && x.BelongsToDate <= to)
				.ToArray();
		}
		
		public static IEnumerable<ScheduledActivity> MakeScheduledActivities(
			ICurrentScenario scenario,
			IScheduleStorage scheduleStorage,
			IEnumerable<IPerson> people,
			DateOnlyPeriod period)
		{
			var defaultScenario = scenario.Current();
			if (defaultScenario == null)
				return Enumerable.Empty<ScheduledActivity>();
			
			var schedules = scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				people,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				defaultScenario);

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