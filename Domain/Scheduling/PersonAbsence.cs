using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	/// <summary>
	/// Class describing an PersonAbsence
	/// </summary>
	public class PersonAbsence : VersionedAggregateRoot, IPersonAbsence
	{
		private IPerson _person;
		private IScenario _scenario;
		private IAbsenceLayer _layer;
		private DateTime? _lastChange;
		private IAbsenceRequest _absenceRequest;

		/// <summary>
		/// Creates a new instance of PersonAbsence
		/// </summary>
		/// <param name="agent">The agent.</param>
		/// <param name="scenario">The scenario.</param>
		/// <param name="layer">The layer.</param>
		public PersonAbsence(IPerson agent,
							 IScenario scenario,
							 IAbsenceLayer layer)
		{
			_person = agent;
			_scenario = scenario;
			_layer = layer;
		}

		public PersonAbsence(IPerson agent, IScenario scenario, IAbsenceLayer layer, IAbsenceRequest absenceRequest)
		{
			_person = agent;
			_scenario = scenario;
			_layer = layer;

			_absenceRequest = absenceRequest;

			if (_absenceRequest != null)
			{
				_absenceRequest.PersonAbsences.Add (this);
			}
		}

		/// <summary>
		/// Constructor for CommandHandlers
		/// </summary>
		public PersonAbsence(IScenario scenario)
		{
			_scenario = scenario;
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
				LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
			};
			if (trackedCommandInfo != null)
			{
				fullDayAbsenceAddedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				fullDayAbsenceAddedEvent.CommandId = trackedCommandInfo.TrackId;
			}
			AddEvent(fullDayAbsenceAddedEvent);
		}

		public virtual void IntradayAbsence(IPerson person, TrackedCommandInfo trackedCommandInfo)
		{
			_person = person;

			var personAbsenceAddedEvent = new PersonAbsenceAddedEvent
			{
				AbsenceId = _layer.Payload.Id.GetValueOrDefault(),
				PersonId = person.Id.GetValueOrDefault(),
				StartDateTime = _layer.Period.StartDateTime,
				EndDateTime = _layer.Period.EndDateTime,
				ScenarioId = _scenario.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
			};
			if (trackedCommandInfo != null)
			{
				personAbsenceAddedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				personAbsenceAddedEvent.CommandId = trackedCommandInfo.TrackId;
			}
			AddEvent(personAbsenceAddedEvent);
		}

		public virtual void RemovePersonAbsence(TrackedCommandInfo trackedCommandInfo)
		{
			var personAbsenceRemovedEvent = new PersonAbsenceRemovedEvent
			{
				PersonId = Person.Id.GetValueOrDefault(),
				ScenarioId = Scenario.Id.GetValueOrDefault(),
				StartDateTime = Period.StartDateTime,
				EndDateTime = Period.EndDateTime,
				LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
				
			};
			
			if (trackedCommandInfo != null)
			{
				personAbsenceRemovedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				personAbsenceRemovedEvent.CommandId = trackedCommandInfo.TrackId;
			}
			AddEvent(personAbsenceRemovedEvent);
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
				LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
			};
			if (trackedCommandInfo != null)
			{
				personAbsenceModifiedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
				personAbsenceModifiedEvent.CommandId = trackedCommandInfo.TrackId;
			}

			AddEvent(personAbsenceModifiedEvent);
		}

		public override IEnumerable<IEvent> PopAllEvents(INow now, DomainUpdateType? operation = null)
		{
			var events = base.PopAllEvents(now, operation).ToList();
			if (!operation.HasValue) return events;
			if (operation == DomainUpdateType.Delete)
			{
				var requestPersonAbsenceRemovedEvent = new RequestPersonAbsenceRemovedEvent()
				{
					PersonId = Person.Id.GetValueOrDefault(),
					ScenarioId = Scenario.Id.GetValueOrDefault(),
					StartDateTime = Period.StartDateTime,
					EndDateTime = Period.EndDateTime,
					LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()					
				};

				if (AbsenceRequest != null)
				{
					requestPersonAbsenceRemovedEvent.AbsenceRequestId = AbsenceRequest.Id.GetValueOrDefault();
				}
				events.Add(requestPersonAbsenceRemovedEvent);
			}
			return events;
		}


		/// <summary>
		/// Constructor for NHibernate
		/// </summary>
		protected PersonAbsence()
		{
		}



		/// <summary>
		/// Gets the layer.
		/// </summary>
		/// <value>The layer.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-15
		/// </remarks>
		public virtual IAbsenceLayer Layer
		{
			get { return _layer; }
		}



		/// <summary>
		/// Gets or sets the Person
		/// </summary>
		public virtual IPerson Person
		{
			get { return _person; }
		}

		/// <summary>
		/// Gets or sets the Scenario
		/// </summary>
		public virtual IScenario Scenario
		{
			get { return _scenario; }
		}

		public virtual bool BelongsToScenario(IScenario scenario)
		{
			return Scenario.Equals(scenario);
		}

		public virtual string FunctionPath
		{
			get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence; }
		}

		public virtual IPersistableScheduleData CreateTransient()
		{
			return NoneEntityClone();
		}

		public virtual IPersistableScheduleData CloneAndChangeParameters(IScheduleParameters parameters)
		{
			var retObj = (PersonAbsence)NoneEntityClone();
			retObj._scenario = parameters.Scenario;
			retObj._person = parameters.Person;
			retObj._absenceRequest = null;
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
		public virtual DateTimePeriod Period
		{
			get { return Layer.Period; }
		}

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
					var newPeriods = Period.ExcludeDateTimePeriod(period);
					foreach (var dateTimePeriod in newPeriods)
					{
						if (dateTimePeriod.ElapsedTime() > TimeSpan.Zero)
						{
							var newLayer = new AbsenceLayer(Layer.Payload, dateTimePeriod);
							IPersonAbsence personAbsence = new PersonAbsence(Person, Scenario, newLayer, _absenceRequest);
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
			get { return _lastChange; }
			set { _lastChange = value; }
		}

		public virtual IAbsenceRequest AbsenceRequest
		{
			get { return _absenceRequest; }
			set { _absenceRequest = value; }
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
			retObj._layer = (AbsenceLayer)Layer.EntityClone();

			return retObj;
		}

		#endregion


		public virtual IAggregateRoot MainRoot
		{
			get { return Person; }
		}
	}
}
