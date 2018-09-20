﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Timers;
using System.Web.Http;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.All)]
	public class RtaTestDataController : ApiController
	{
		private readonly IPersonRepository _persons;
		private readonly IScheduleStorage _schedules;
		private readonly ICurrentScenario _scenario;
		private readonly IActivityRepository _activities;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IRtaEventStore _events;
		private readonly IRtaEventStoreSynchronizer _synchronizer;
		private readonly IKeyValueStorePersister _keyValueStore;


		public RtaTestDataController(
			IPersonRepository persons,
			IScheduleStorage schedules,
			ICurrentScenario scenario,
			IActivityRepository activities,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IRtaEventStore events, IRtaEventStoreSynchronizer synchronizer, IKeyValueStorePersister keyValueStore)
		{
			_persons = persons;
			_schedules = schedules;
			_scenario = scenario;
			_activities = activities;
			_differenceService = differenceService;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_events = events;
			_synchronizer = synchronizer;
			_keyValueStore = keyValueStore;
		}

		[HttpGet, Route("api/RtaTestData/MakeStuff")]
		public virtual HttpResponseMessage MakeStuff()
		{
			var log = CreateStuff();

			var timer = new Stopwatch();
			timer.Start();
			_synchronizer.Synchronize();
			timer.Stop();
			log.AppendLine($"Elapsed time: {timer.Elapsed}");

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(log.ToString(), System.Text.Encoding.UTF8, "text/plain")
			};
		}

		[UnitOfWork]
		public virtual StringBuilder CreateStuff()
		{
			var log = new StringBuilder();
			var date = DateOnly.Today.AddDays(-1);
			var period = new DateOnlyPeriod(date.AddDays(-5), date);
			var dates = period.DayCollection();
			var allPersons = _persons.LoadAll();

//			var team =
//			(
//				from p in allPersons
//				let pp = p.Period(date)
//				where pp != null
//				let t = pp.Team
//				where t != null
//				let externalLogon = pp.ExternalLogOnCollection.FirstOrDefault()
//				where externalLogon != null
//				select t
//			).First();

//			var threePersonsInTeam =
//			(
//				from p in allPersons
//				let pp = p.Period(date)
//				where pp != null
//				let t = pp.Team
//				where t == team
//				let externalLogon = pp.ExternalLogOnCollection.FirstOrDefault()
//				where externalLogon != null
//				select p
//			).Take(3).ToArray();
//			threePersonsInTeam.ForEach(p => log.AppendLine($"Found {p.Name}"));

			var persons =
			(
				from p in allPersons
				let pp = p.Period(date)
				where pp != null
				let externalLogon = pp.ExternalLogOnCollection.FirstOrDefault()
				where externalLogon != null
				select p
			).ToArray();
			persons.ForEach(p => log.AppendLine($"Found {p.Name}"));

//			var schedules = _schedules.FindSchedulesForPersonsOnlyInGivenPeriod(
//				threePersonsInTeam,
//				new ScheduleDictionaryLoadOptions(false, false),
//				period,
//				_scenario.Current()
//			);

			var schedules = _schedules.FindSchedulesForPersonsOnlyInGivenPeriod(
				persons,
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				_scenario.Current()
			);
			(schedules as ReadOnlyScheduleDictionary)?.MakeEditable();

			var phone =
			(
				from a in _activities.LoadAll()
				where a.Name == "Phone"
				select a
			).Single();

			persons
				.Select(x => schedules[x])
				.ForEach(range =>
				{
					log.AppendLine($"Creating schedule for {range.Person.Name}");

					dates
						.Select(range.ScheduledDay)
						.ForEach(day =>
						{
							log.AppendLine($"Creating schedule for {range.Person.Name} {day.DateOnlyAsPeriod.DateOnly}");

							var pa = day.PersonAssignment(true);
							var shiftStartTime = DateTime.SpecifyKind(pa.Date.Date.AddHours(8), DateTimeKind.Utc);
							var shiftEndTime = shiftStartTime.AddHours(8);
							var shiftTime = new DateTimePeriod(shiftStartTime, shiftEndTime);
							pa.Clear();
							pa.SetDayOff(null);
							pa.AddActivity(phone, shiftTime);

							schedules.Modify(day, new DoNothingScheduleDayChangeCallBack());
						});

					log.AppendLine($"Saving schedule for {range.Person.Name}");
					var diff = range.DifferenceSinceSnapshot(_differenceService);
					_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) range);
				});

			var moments = new[]
			{
				new Func<DateOnly, Guid, IEnumerable<IEvent>>((d, p) =>
				{
					return new IEvent[]
					{
						new PersonStateChangedEvent
						{
							PersonId = p,
							BelongsToDate = d,
							Timestamp = d.Date.AddHours(8).Utc(),
							StateName = "Logon",
							StateGroupId = null,
							ActivityName = null,
							ActivityColor = null,
							RuleName = "In",
							RuleColor = Color.Red.ToArgb(),
							Adherence = EventAdherence.In
						},
						new PersonRuleChangedEvent
						{
							PersonId = p,
							BelongsToDate = d,
							Timestamp = d.Date.AddHours(8).Utc(),
							StateName = "Logon",
							StateGroupId = null,
							ActivityName = "Phone",
							ActivityColor = Color.Green.ToArgb(),
							RuleName = "In",
							RuleColor = Color.Green.ToArgb(),
							Adherence = EventAdherence.In
						}
					};
				}),
				new Func<DateOnly, Guid, IEnumerable<IEvent>>((d, p) =>
				{
					var listOfEvents = new List<IEvent>();
					
					for (int i = 1; i < 80; i ++)
					{
						listOfEvents.AddRange(new IEvent[]
						{
							new PersonStateChangedEvent
							{
								PersonId = p,
								BelongsToDate = d,
								Timestamp = d.Date.AddHours(8).AddMinutes(i).Utc(),
								StateName = "Logoff",
								StateGroupId = null,
								ActivityName = "Phone",
								ActivityColor = Color.Green.ToArgb(),
								RuleName = "Out",
								RuleColor = Color.Red.ToArgb(),
								Adherence = EventAdherence.Out
							}, 
							new PersonRuleChangedEvent
							{
								PersonId = p,
								BelongsToDate = d,
								Timestamp = d.Date.AddHours(8).AddMinutes(i).Utc(),
								StateName = "Logoff",
								StateGroupId = null,
								ActivityName = "Phone",
								ActivityColor = Color.Green.ToArgb(),
								RuleName = "Out",
								RuleColor = Color.Red.ToArgb(),
								Adherence = EventAdherence.Out
							}
						});
					}
					return listOfEvents.ToArray();
				}),
				new Func<DateOnly, Guid, IEnumerable<IEvent>>((d, p) =>
				{
					return new[]
					{
						new PersonRuleChangedEvent
						{
							PersonId = p,
							BelongsToDate = d,
							Timestamp = d.Date.AddHours(8).AddHours(8).Utc(),
							StateName = "Logoff",
							StateGroupId = null,
							ActivityName = null,
							ActivityColor = null,
							RuleName = "In",
							RuleColor = Color.Green.ToArgb(),
							Adherence = EventAdherence.In
						}
					};
				})
			};

			log.AppendLine($"Removing all events");

			_events.Remove(DateTime.Now.AddYears(1), int.MaxValue);
			
			var events = new List<IEvent>();

			dates.ForEach(d =>
			{
				moments.ForEach(what =>
				{
					persons
						.SelectMany(x => what.Invoke(d, x.Id.Value))
						.ForEach(x =>
						{
							var q = (x as IRtaStoredEvent)?.QueryData();
							//log.AppendLine($"Adding event {q.PersonId} {q.StartTime} {x.GetType().Name}");

							events.Add(x);
							//_events.Add(x);
						});
				});
			});

			var orderedEvents = events.OrderBy(x => (x as IRtaStoredEvent).QueryData().StartTime).ThenBy(x => (x as IRtaStoredEvent).QueryData().PersonId).ToArray();

			orderedEvents.ForEach(e =>
			{
				var q = (e as IRtaStoredEvent)?.QueryData();
				log.AppendLine($"Adding event {q.PersonId} {q.StartTime} {e.GetType().Name}");			
				_events.Add(e);
			});
			
			
			return log;
		}


		[HttpGet, Route("api/RtaTestData/SynchStuff")]
		public virtual HttpResponseMessage SynchStuff()
		{
			_synchronizer.Synchronize();

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("Synchrooooooooooooooonized ", System.Text.Encoding.UTF8, "text/plain")
			};
		}
	}
}