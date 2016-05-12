using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersonAssignment : VersionedAggregateRoot, 
									IPersonAssignment,
									IExportToAnotherScenario
	{
		private IList<IShiftLayer> _shiftLayers;
		private IPerson _person;
		private IScenario _scenario;
		private IShiftCategory _shiftCategory;
		private IDayOffTemplate _dayOffTemplate;


		public PersonAssignment(IPerson agent, IScenario scenario, DateOnly date)
		{
			Date = date;
			_person = agent;
			_scenario = scenario;
			_shiftLayers = new List<IShiftLayer>();
		}

		protected PersonAssignment()
		{
		}

		public virtual void Clear()
		{
			ClearMainActivities();
			ClearOvertimeActivities();
			ClearPersonalActivities();
			SetDayOff(null);
			AddEvent(() => new DayUnscheduledEvent
			{
				Date = Date.Date,
				PersonId = Person.Id.Value,
				ScenarioId = Scenario.Id.Value,
				LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
			});
		}

		public virtual DateOnly Date { get; protected set; }

		public virtual DateTimePeriod Period
		{
			get { return mergedMainShiftAndPersonalPeriods(); }
		}

		public virtual DateTimePeriod PeriodExcludingPersonalActivity()
		{
			// Duplicate mergedMainShiftAndPersonalPeriods but exlude PersonalShiftLayer explicitly.
			DateTimePeriod? mergedPeriod = null;
			foreach(var shiftLayer in ShiftLayers)
			{
				if (shiftLayer.GetType() == typeof (PersonalShiftLayer)) continue;
				mergedPeriod = DateTimePeriod.MaximumPeriod(shiftLayer.Period,mergedPeriod);
			}
			if(mergedPeriod.HasValue)
				return mergedPeriod.Value;

			var dayOff = DayOff();
			return dayOff == null ?
				new DateOnlyPeriod(Date,Date).ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone()) :    //don't like to jump to person aggregate here... "st�mpla" assignment with a timezone instead.
				new DateTimePeriod(dayOff.Anchor,dayOff.Anchor.AddTicks(1));
		}

		private DateTimePeriod mergedMainShiftAndPersonalPeriods()
		{
			//this is quite strange... probably wrong impl if eg only overtime or personal layer exists together with dayoff
			//just changed this impl for perf reasons, but kept old "strange" (?) behaviour.
			DateTimePeriod? mergedPeriod = null;
			foreach (var shiftLayer in ShiftLayers)
			{
				mergedPeriod = DateTimePeriod.MaximumPeriod(shiftLayer.Period, mergedPeriod);
			}
			if (mergedPeriod.HasValue)
				return mergedPeriod.Value;

			var dayOff = DayOff();
			return dayOff == null ? 
				new DateOnlyPeriod(Date, Date).ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone()) :	//don't like to jump to person aggregate here... "st�mpla" assignment with a timezone instead.
				new DateTimePeriod(dayOff.Anchor, dayOff.Anchor.AddTicks(1));
		}

		public virtual bool BelongsToScenario(IScenario scenario)
		{
			return Scenario.Equals(scenario);
		}

		public virtual string FunctionPath
		{
			get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment; }
		}

		public virtual IPersistableScheduleData CreateTransient()
		{
			return NoneEntityClone();
		}

		public virtual IPersistableScheduleData CloneAndChangeParameters(IScheduleParameters parameters)
		{
			var retObj = (PersonAssignment)NoneEntityClone();
			retObj._scenario = parameters.Scenario;
			retObj._person = parameters.Person;
			return retObj;
		}

		public virtual IShiftCategory ShiftCategory
		{
			get
			{
				if (!MainActivities().Any())
					return null;

				return _shiftCategory;
			}
			protected set { _shiftCategory = value; }
		}

		public virtual IEnumerable<IMainShiftLayer> MainActivities()
		{
			return _shiftLayers.OfType<IMainShiftLayer>();
		}

		public virtual IEnumerable<IPersonalShiftLayer> PersonalActivities()
		{
			return _shiftLayers.OfType<IPersonalShiftLayer>();
		}

		public virtual IEnumerable<IOvertimeShiftLayer> OvertimeActivities()
		{
			return _shiftLayers.OfType<IOvertimeShiftLayer>();
		}

		public virtual IEnumerable<IShiftLayer> ShiftLayers
		{
			get { return _shiftLayers; }
		}

		public virtual IPerson Person
		{
			get { return _person; }
		}

		public virtual IScenario Scenario
		{
			get { return _scenario; }
		}

		public virtual bool RemoveActivity(IShiftLayer layer, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			var removed = _shiftLayers.Remove(layer);

			if ( !muteEvent )
			{
				AddEvent ( () =>
				{
					var activityRemovedEvent = new PersonAssignmentLayerRemovedEvent
					{
						Date = Date.Date,
						PersonId = Person.Id.Value,						
						StartDateTime = layer.Period.StartDateTime,
						EndDateTime = layer.Period.EndDateTime,
						ScenarioId = Scenario.Id.Value,
						LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
					};
					if ( trackedCommandInfo != null )
					{
						activityRemovedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
						activityRemovedEvent.CommandId = trackedCommandInfo.TrackId;
					}
					return activityRemovedEvent;
				} );
			}

			return removed;
		}
		
		public virtual void ClearPersonalActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			PersonalActivities().ToArray().ForEach(l => RemoveActivity(l, muteEvent, trackedCommandInfo));
		}

		public virtual void ClearMainActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			MainActivities().ToArray().ForEach(l => RemoveActivity(l, muteEvent, trackedCommandInfo ) );
		}

		public virtual void ClearOvertimeActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			OvertimeActivities().ToArray().ForEach(l => RemoveActivity(l, muteEvent, trackedCommandInfo ) );
		}

		public virtual void CheckRestrictions()
		{
			RestrictionSet.CheckEntity(this);
		}

		public virtual IRestrictionSet<IPersonAssignment> RestrictionSet
		{
			get { return PersonAssignmentRestrictionSet.CurrentPersonAssignmentRestrictionSet; }
		}

		public virtual IProjectionService ProjectionService()
		{
			var proj = new VisualLayerProjectionService(Person);
			if (hasProjection())
			{
				proj.Add(MainActivities(), new VisualLayerFactory());
				var validPeriods = new HashSet<DateTimePeriod>(MainActivities().PeriodBlocks());
				foreach (var overtimeLayer in OvertimeActivities())
				{
					var overTimePeriod = overtimeLayer.Period;
					proj.Add(overtimeLayer, new VisualLayerOvertimeFactory());
					validPeriods.Add(overTimePeriod);
				}
				foreach (var personalLayer in PersonalActivities())
				{
					if (validPeriods.Any(validPeriod => validPeriod.Intersect(personalLayer.Period) || validPeriod.AdjacentTo(personalLayer.Period)))
					{
						proj.Add(personalLayer, new VisualLayerFactory());
					}
				}
			}

			return proj;
		}

		private bool hasProjection()
		{
			return MainActivities().Any() || OvertimeActivities().Any();
		}

		public virtual object Clone()
		{
			return EntityClone();
		}

		public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
		{
			return dateAndPeriod.DateOnly == Date;
		}

		public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
		{
			return dateOnlyPeriod.Contains(Date);
		}

		public virtual IPersonAssignment NoneEntityClone()
		{
			var retobj = (PersonAssignment)MemberwiseClone();
			retobj.SetId(null);
			retobj._shiftLayers = new List<IShiftLayer>();
			foreach (var newLayer in _shiftLayers.Select(layer => layer.EntityClone()))
			{
				newLayer.SetParent(retobj);
				newLayer.SetId(null);
				retobj._shiftLayers.Add(newLayer);
			}

			return retobj;
		}

		public virtual IPersonAssignment EntityClone()
		{
			var retobj = (PersonAssignment)MemberwiseClone();
			retobj._shiftLayers = new List<IShiftLayer>();
			foreach (var newLayer in _shiftLayers.Select(layer => layer.EntityClone()))
			{
				newLayer.SetParent(retobj);
				retobj._shiftLayers.Add(newLayer);
			}

			return retobj;
		}

		public virtual IAggregateRoot MainRoot
		{
			get { return Person; }
		}

		public virtual void AddPersonalActivity(IActivity activity, DateTimePeriod period)
		{
			var layer = new PersonalShiftLayer(activity, period);
			layer.SetParent(this);
			_shiftLayers.Add(layer);
		}

		public virtual void AddOvertimeActivity(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
		{
			if (period.StartDateTime.Equals(period.EndDateTime)) return;
			var layer = new OvertimeShiftLayer(activity, period, multiplicatorDefinitionSet);
			layer.SetParent(this);
			_shiftLayers.Add(layer);
		}

		public virtual void AddActivity(IActivity activity, DateTimePeriod period)
		{
			AddActivity(activity, period, null);
		}

		public virtual void AddActivity(IActivity activity, TimePeriod period)
		{
			var periodAsDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(Date.Date.Add(period.StartTime), 
				Date.Date.Add(period.EndTime),
				Person.PermissionInformation.DefaultTimeZone());
			AddActivity(activity, periodAsDateTimePeriod);
		}

		public virtual void AddActivity(IActivity activity, DateTimePeriod period, TrackedCommandInfo trackedCommandInfo)
		{
			addActivityInternal(activity, period);
			AddEvent(() =>
			{
				var activityAddedEvent = new ActivityAddedEvent
				{
					Date = Date.Date,
					PersonId = Person.Id.Value,
					ActivityId = activity.Id.Value,
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					ScenarioId = Scenario.Id.Value,
					LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
				};
				if (trackedCommandInfo != null)
				{
					activityAddedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
					activityAddedEvent.CommandId = trackedCommandInfo.TrackId;
				}
				return activityAddedEvent;
			});
		}

		private void addActivityInternal(IActivity activity, DateTimePeriod period)
		{
			var layer = new MainShiftLayer(activity, period);
			layer.SetParent(this);
			_shiftLayers.Add(layer);
			SetDayOff(null);
		}

		public virtual void SetShiftCategory(IShiftCategory shiftCategory)
		{
			_shiftCategory = shiftCategory;
		}

		public virtual void SetActivitiesAndShiftCategoryFrom(IPersonAssignment assignment)
		{
			ClearMainActivities();
			SetShiftCategory(assignment.ShiftCategory);
			foreach (var mainLayer in assignment.MainActivities())
			{
				AddActivity(mainLayer.Payload, mainLayer.Period);
			}
		}

		public virtual void SetActivitiesAndShiftCategoryFromWithOffset(IPersonAssignment assignment, TimeSpan periodOffset)
		{
			ClearMainActivities();
			SetShiftCategory(assignment.ShiftCategory);
			foreach (var mainLayer in assignment.MainActivities())
			{
				addActivityInternal(mainLayer.Payload, mainLayer.Period.MovePeriod(periodOffset));
			}
			var startDate = assignment.MainActivities().Min(x => x.Period.StartDateTime);
			var endDate = assignment.MainActivities().Max(x => x.Period.EndDateTime);
			AddEvent(() =>
			{
				var someEventForNotification = new MainShiftReplaceNotificationEvent
				{
					PersonId = Person.Id.Value,
					StartDateTime = startDate,
					EndDateTime = endDate,
					ScenarioId = Scenario.Id.Value,
					LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
				};
				return someEventForNotification;
			});
		}

		public virtual void InsertActivity(IActivity activity, DateTimePeriod period, int index)
		{
			var layer = new MainShiftLayer(activity, period);
			layer.SetParent(this);
			_shiftLayers.Insert(index, layer);
			SetDayOff(null);

		}

		public virtual void InsertOvertimeLayer(IActivity activity, DateTimePeriod period, int index,
		                                IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
		{

			var layer = new OvertimeShiftLayer(activity, period, multiplicatorDefinitionSet);
			layer.SetParent(this);
			_shiftLayers.Insert(index,layer);
		}

		public virtual void InsertPersonalLayer(IActivity activity, DateTimePeriod period, int index)
		{
			var layer = new PersonalShiftLayer(activity, period);
			layer.SetParent(this);
			_shiftLayers.Insert(index,layer);
		}

		public virtual void MoveLayerVertical(IMoveLayerVertical target, IShiftLayer layer)
		{
			target.Move(_shiftLayers, layer);
		}

		public virtual void MoveActivityAndSetHighestPriority(IActivity activity, DateTime currentStartTime, DateTime newStartTime, TimeSpan length, TrackedCommandInfo trackedCommandInfo)
		{
			var anyLayerFound = false;

			foreach (var layer in ShiftLayers.ToArray())
			{
				var currentLayerPeriod = new DateTimePeriod(currentStartTime, currentStartTime.Add(length));
				if(!layer.Payload.Equals(activity) || !layer.Period.Intersect(currentLayerPeriod))
					continue;
				var originalOrderIndex = ShiftLayers.ToList().IndexOf(layer);
				RemoveActivity(layer);

				var layerContainsCurrentLayerPeriod = layer.Period;
				var splittedLayerPeriods = layerContainsCurrentLayerPeriod.ExcludeDateTimePeriod(currentLayerPeriod);
				splittedLayerPeriods.ForEach(period => InsertActivity(layer.Payload, period, originalOrderIndex));
				anyLayerFound = true;
			}
			if(!anyLayerFound)
				throw new ArgumentException("No layer(s) found!", "activity");

			//will be fixed later (=Erik)
			var newPeriod = new DateTimePeriod(newStartTime, newStartTime.Add(length));
			///////////////////////////////
			
			addActivityInternal(activity, newPeriod);

			var affectedPeriod = new DateTimePeriod(currentStartTime, currentStartTime.Add(length)).MaximumPeriod(newPeriod);
			
			AddEvent(()=>
			{
				var activityMovedEvent = new ActivityMovedEvent
				{
					PersonId = Person.Id.Value,
					StartDateTime = affectedPeriod.StartDateTime,
					EndDateTime =affectedPeriod.EndDateTime,
					ScenarioId = Scenario.Id.Value,
					LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
				};
				if (trackedCommandInfo != null)
				{
					activityMovedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
					activityMovedEvent.CommandId = trackedCommandInfo.TrackId;
				}
				return activityMovedEvent;
			});
		}
		public virtual void MoveActivityAndKeepOriginalPriority(IShiftLayer shiftLayer, DateTime newStartTimeInUtc, TrackedCommandInfo trackedCommandInfo)
		{
			var originalOrderIndex = ShiftLayers.ToList().IndexOf(shiftLayer);
			if (originalOrderIndex < 0)
				throw new ArgumentException("No layer(s) found!", "activity");
			RemoveActivity(shiftLayer);

			var newLayerPeriod = new DateTimePeriod(newStartTimeInUtc, newStartTimeInUtc.Add(shiftLayer.Period.EndDateTime.Subtract(shiftLayer.Period.StartDateTime)));
			InsertActivity(shiftLayer.Payload, newLayerPeriod, originalOrderIndex);

			var affectedPeriod = shiftLayer.Period.MaximumPeriod(newLayerPeriod);
			
			AddEvent(()=>
			{
				var activityMovedEvent = new ActivityMovedEvent
				{
					PersonId = Person.Id.Value,
					StartDateTime = affectedPeriod.StartDateTime,
					EndDateTime =affectedPeriod.EndDateTime,
					ScenarioId = Scenario.Id.Value,
					LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
				};
				if (trackedCommandInfo != null)
				{
					activityMovedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
					activityMovedEvent.CommandId = trackedCommandInfo.TrackId;
				}
				return activityMovedEvent;
			});
		}

		public virtual IDayOff DayOff()
		{
			if (_dayOffTemplate == null)
				return null;
			//don't like to jump to person aggregate here... "st�mpla" assignment with a timezone instead.
			var anchorDateTime = TimeZoneHelper.ConvertToUtc(Date.Date.Add(_dayOffTemplate.Anchor), Person.PermissionInformation.DefaultTimeZone());
			return new DayOff(anchorDateTime, _dayOffTemplate.TargetLength, _dayOffTemplate.Flexibility, _dayOffTemplate.Description, _dayOffTemplate.DisplayColor, _dayOffTemplate.PayrollCode);
		}

		public virtual void SetDayOff(IDayOffTemplate template)
		{
			_dayOffTemplate = template;
			if (_dayOffTemplate == null) return;

			ClearMainActivities();
			AddEvent(() => new DayOffAddedEvent
			{
				Date = Date.Date,
				PersonId = Person.Id.Value,
				ScenarioId = Scenario.Id.Value,
				LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
			});
		}

		public virtual void SetThisAssignmentsDayOffOn(IPersonAssignment dayOffDestination)
		{
			dayOffDestination.SetDayOff(_dayOffTemplate);
		}

		public virtual bool AssignedWithDayOff(IDayOffTemplate template)
		{
			if (_dayOffTemplate == null)
				return template == null;
			return _dayOffTemplate.Equals(template);
		}

		public virtual void FillWithDataFrom(IPersonAssignment personAssignmentSource)
		{
			Clear();
			personAssignmentSource.SetThisAssignmentsDayOffOn(this);
			SetShiftCategory(personAssignmentSource.ShiftCategory);
			foreach (var mainLayer in personAssignmentSource.MainActivities())
			{
				AddActivity(mainLayer.Payload, mainLayer.Period);
			}
			foreach (var personalLayer in personAssignmentSource.PersonalActivities())
			{
				AddPersonalActivity(personalLayer.Payload, personalLayer.Period);
			}
			foreach (var overtimeLayer in personAssignmentSource.OvertimeActivities())
			{
				AddOvertimeActivity(overtimeLayer.Payload, overtimeLayer.Period, overtimeLayer.DefinitionSet);
			}
		}

		#region Equals

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((IEntity)obj);
		}

		public override bool Equals(IEntity other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			var otherAsAss = other as IPersonAssignment;
			return otherAsAss != null && (Equals(_person, otherAsAss.Person) &&
						      Equals(_scenario, otherAsAss.Scenario) &&
						      Date.Equals(otherAsAss.Date));
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Date.GetHashCode();
				hashCode = (hashCode * 397) ^ (_person != null ? _person.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_scenario != null ? _scenario.GetHashCode() : 0);
				return hashCode;
			}
		}

		#endregion
	}
}
