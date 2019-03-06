using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.Scheduling
{
	/// <summary>
	/// Class describing an PersonAbsence
	/// </summary>
	public class PersonAbsence : AggregateRoot_Events_ChangeInfo_Versioned, IPersonAbsence
	{
		private IPerson _person;
		private IScenario _scenario;
		private IAbsenceLayer _layer;
		private DateTime? _lastChange;

		public PersonAbsence(IPerson agent, IScenario scenario, IAbsenceLayer layer) : this(scenario)
		{
			_person = agent;
			_layer = layer;
		}

		/// <summary>
		/// Constructor for CommandHandlers
		/// </summary>
		public PersonAbsence(IScenario scenario) : this()
		{
			_scenario = scenario;
		}

		protected PersonAbsence()
		{
			_lastChange = DateTime.UtcNow;
		}

		/// <summary>
		/// Make this person absence a full day absence
		/// </summary>
		public virtual void FullDayAbsence(IPerson person, TrackedCommandInfo trackedCommandInfo)
		{
			_person = person;

			var fullDayAbsenceAddedEvent = new FullDayAbsenceAddedEvent
			{
				AbsenceId = _layer.Payload.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = _layer.Period.StartDateTime,
				EndDateTime = _layer.Period.EndDateTime,
				ScenarioId = _scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault()
			};
			if (trackedCommandInfo != null)
			{
				fullDayAbsenceAddedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				fullDayAbsenceAddedEvent.CommandId = trackedCommandInfo.TrackId;
			}

			var @events = PopAllEvents(null).ToList();

			if (!@events.Any(e =>
			 {
				 var _e = e as FullDayAbsenceAddedEvent;
				 if (_e == null) return false;
				 return _e.AbsenceId == fullDayAbsenceAddedEvent.AbsenceId
						&& _e.PersonId == fullDayAbsenceAddedEvent.PersonId
						&& _e.StartDateTime == fullDayAbsenceAddedEvent.StartDateTime
						&& _e.EndDateTime == fullDayAbsenceAddedEvent.EndDateTime
						&& _e.ScenarioId == fullDayAbsenceAddedEvent.ScenarioId
						&& _e.LogOnBusinessUnitId == fullDayAbsenceAddedEvent.LogOnBusinessUnitId;
			 }))
			{
				@events.Add(fullDayAbsenceAddedEvent);
			}

			@events.ForEach(AddEvent);
		}

		public virtual void IntradayAbsence(IPerson person, TrackedCommandInfo trackedCommandInfo, bool muteEvent = false)
		{
			_person = person;
			if (muteEvent) return;

			var personAbsenceAddedEvent = new PersonAbsenceAddedEvent
			{
				AbsenceId = _layer.Payload.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = _layer.Period.StartDateTime,
				EndDateTime = _layer.Period.EndDateTime,
				ScenarioId = _scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault()
			};
			if (trackedCommandInfo != null)
			{
				personAbsenceAddedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				personAbsenceAddedEvent.CommandId = trackedCommandInfo.TrackId;
			}

			var @events = PopAllEvents(null).ToList();

			if (!@events.Any(e => e is PersonAbsenceAddedEvent addedEvent && (addedEvent.AbsenceId == personAbsenceAddedEvent.AbsenceId
																			  && addedEvent.PersonId == personAbsenceAddedEvent.PersonId
																			  && addedEvent.StartDateTime == personAbsenceAddedEvent.StartDateTime
																			  && addedEvent.EndDateTime == personAbsenceAddedEvent.EndDateTime
																			  && addedEvent.ScenarioId == personAbsenceAddedEvent.ScenarioId
																			  && addedEvent.LogOnBusinessUnitId ==
																			  personAbsenceAddedEvent.LogOnBusinessUnitId)))
			{
				@events.Add(personAbsenceAddedEvent);
			}

			@events.ForEach(AddEvent);
		}

		public virtual void RemovePersonAbsence(TrackedCommandInfo trackedCommandInfo, DateTimePeriod? eventPeriod = null)
		{
			var period = eventPeriod ?? Period;

			var personAbsenceRemovedEvent = new PersonAbsenceRemovedEvent
			{
				PersonId = Person.Id.GetValueOrDefault(),
				ScenarioId = Scenario.Id.GetValueOrDefault(),
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime,
				LogOnBusinessUnitId = Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault()

			};

			if (trackedCommandInfo != null)
			{
				personAbsenceRemovedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				personAbsenceRemovedEvent.CommandId = trackedCommandInfo.TrackId;
			}
			AddEvent(personAbsenceRemovedEvent);
		}

		public override void NotifyDelete()
		{
			base.NotifyDelete();
			if (!_scenario.DefaultScenario) return;

			var requestPersonAbsenceRemovedEvent = new RequestPersonAbsenceRemovedEvent
			{
				PersonId = Person.Id.GetValueOrDefault(),
				ScenarioId = Scenario.Id.GetValueOrDefault(),
				StartDateTime = Period.StartDateTime,
				EndDateTime = Period.EndDateTime,
				LogOnBusinessUnitId = Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault(),
			};

			AddEvent(requestPersonAbsenceRemovedEvent);
		}

		public virtual void AddExplicitAbsence(IPerson person, IAbsence absence, DateTime startDateTime, DateTime endDateTime)
		{
			startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startDateTime, DateTimeKind.Unspecified), person.PermissionInformation.DefaultTimeZone());
			endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endDateTime, DateTimeKind.Unspecified), person.PermissionInformation.DefaultTimeZone());

			_person = person;
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(startDateTime, endDateTime));
			_layer = absenceLayer;

			AddEvent(new PersonAbsenceAddedEvent
			{
				AbsenceId = absence.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = startDateTime,
				EndDateTime = endDateTime,
				ScenarioId = _scenario.Id.GetValueOrDefault()
			});
		}

		public virtual void ModifyPersonAbsencePeriod(DateTimePeriod newAbsencePeriod, TrackedCommandInfo trackedCommandInfo)
		{
			var absence = _layer.Payload as Absence;
			var existingEndDateTime = _layer.Period.EndDateTime;
			_layer = new AbsenceLayer(absence, newAbsencePeriod);
			LastChange = DateTime.UtcNow;

			var personAbsenceModifiedEvent = new PersonAbsenceModifiedEvent
			{
				AbsenceId = Id.GetValueOrDefault(),
				PersonId = Person.Id.GetValueOrDefault(),
				ScenarioId = Scenario.Id.GetValueOrDefault(),
				StartDateTime = newAbsencePeriod.StartDateTime,
				EndDateTime = existingEndDateTime, // use existingEndDateTime so the entire existing period schedule is updated.
				LogOnBusinessUnitId = Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id.GetValueOrDefault()
			};
			if (trackedCommandInfo != null)
			{
				personAbsenceModifiedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				personAbsenceModifiedEvent.CommandId = trackedCommandInfo.TrackId;
			}

			AddEvent(personAbsenceModifiedEvent);
		}

		/// <summary>
		/// Gets the layer.
		/// </summary>
		/// <value>The layer.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-15
		/// </remarks>
		public virtual IAbsenceLayer Layer => _layer;

		/// <summary>
		/// Gets or sets the Person
		/// </summary>
		public virtual IPerson Person => _person;

		/// <summary>
		/// Gets or sets the Scenario
		/// </summary>
		public virtual IScenario Scenario => _scenario;

		public virtual bool BelongsToScenario(IScenario scenario)
		{
			return Scenario.Equals(scenario);
		}

		public virtual string FunctionPath => DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence;

		public virtual IPersistableScheduleData CreateTransient()
		{
			return NoneEntityClone();
		}

		public virtual IPersistableScheduleData CloneAndChangeParameters(IScheduleParameters parameters)
		{
			var retObj = (PersonAbsence)NoneEntityClone();
			CloneEvents(retObj);
			retObj._scenario = parameters.Scenario;
			retObj._person = parameters.Person;
			return retObj;
		}


		/// <summary>
		/// Gets the Period.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-06
		/// </remarks>
		public virtual DateTimePeriod Period => Layer.Period;

		/// <summary>
		/// Returns a list with the absence splitted on specified period
		/// </summary>
		/// <param name="period"></param>
		/// <returns></returns>
		public virtual IList<IPersonAbsence> Split(DateTimePeriod period)
		{
			IList<IPersonAbsence> splitList = new List<IPersonAbsence>();

			if (!period.Contains(Period))
			{
				if (_layer.Period.Intersect(period))
				{
					var newPeriods = Period.Subtract(period);
					foreach (var dateTimePeriod in newPeriods)
					{
						if (dateTimePeriod.ElapsedTime() > TimeSpan.Zero)
						{
							var newLayer = new AbsenceLayer(Layer.Payload, dateTimePeriod);
							IPersonAbsence personAbsence = new PersonAbsence(Person, Scenario, newLayer);
							splitList.Add(personAbsence);
						}
					}
				}
			}

			return splitList;
		}


		/// <summary>
		/// return a merged absence, or null if no merge
		/// </summary>
		/// <param name="personAbsence"></param>
		/// <returns></returns>
		public virtual IPersonAbsence Merge(IPersonAbsence personAbsence)
		{
			IPersonAbsence mergedPersonAbsence = null;

			if (personAbsence.Layer.Payload.Equals(Layer.Payload))
			{
				if (personAbsence.Period.Intersect(Period) || personAbsence.Period.AdjacentTo(Period))
				{
					var newLayer = new AbsenceLayer(Layer.Payload, Period.MaximumPeriod(personAbsence.Period));
					mergedPersonAbsence = new PersonAbsence(Person, Scenario, newLayer);
				}
			}

			return mergedPersonAbsence;
		}

		public virtual DateTime? LastChange
		{
			get => _lastChange;
			set => _lastChange = value;
		}

		public virtual void ReplaceLayer(IAbsence newAbsence, DateTimePeriod newPeriod)
		{
			_layer = new AbsenceLayer(newAbsence, newPeriod);
		}

		#region ICloneableEntity<PersonAbsence> Members

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2007-11-09
		/// </remarks>
		public virtual object Clone()
		{
			return EntityClone();
		}

		public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
		{
			var addExtraEndDayDueToNightShifts = dateAndPeriod.Period().ChangeEndTime(TimeSpan.FromDays(1));
			return addExtraEndDayDueToNightShifts.Intersect(Period);
		}

		public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
		{
			DateTimePeriod dateTimePeriod =
				dateOnlyPeriod.ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone());

			var addExtraEndDayDueToNightShifts = dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(1));
			var addExtraEndDayDueToConsecutiveAbsence = addExtraEndDayDueToNightShifts.ChangeStartTime(TimeSpan.FromDays(-1));
			return addExtraEndDayDueToConsecutiveAbsence.Intersect(Period);
		}

		/// <summary>
		/// Returns a clone of this T with IEntitiy.Id set to null.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-05-27
		/// </remarks>
		public virtual IPersonAbsence NoneEntityClone()
		{
			PersonAbsence retObj = (PersonAbsence)MemberwiseClone();
			retObj.SetId(null);
			CloneEvents(retObj);
			retObj._layer = (IAbsenceLayer)Layer.NoneEntityClone();

			return retObj;
		}

		/// <summary>
		/// Returns a clone of this T with IEntitiy.Id as this T.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-05-27
		/// </remarks>
		public virtual IPersonAbsence EntityClone()
		{
			PersonAbsence retObj = (PersonAbsence)MemberwiseClone();
			CloneEvents(retObj);
			retObj._layer = (AbsenceLayer)Layer.EntityClone();

			return retObj;
		}

		#endregion


		public virtual IAggregateRoot MainRoot => Person;
	}
}
