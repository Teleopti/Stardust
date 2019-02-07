using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersonAssignment : AggregateRoot_Events_ChangeInfo_Versioned,
									IPersonAssignment,
									IExportToAnotherScenario
	{
		private static readonly VisualLayerFactory visualLayerFactory = new VisualLayerFactory();
		private IList<ShiftLayer> _shiftLayers;
		private IPerson _person;
		private IScenario _scenario;
		private IShiftCategory _shiftCategory;
		private IDayOffTemplate _dayOffTemplate;
		private string _source;

		public PersonAssignment(IPerson agent, IScenario scenario, DateOnly date)
		{
			Date = date;
			_person = agent;
			_scenario = scenario;
			_shiftLayers = new List<ShiftLayer>();
		}

		protected PersonAssignment()
		{
		}

		public virtual void Clear(bool muteEvent = false)
		{
			ClearMainActivities();
			ClearOvertimeActivities();
			ClearPersonalActivities();
			SetDayOff(null, true);
			if (!muteEvent)
			{
				AddEvent(() => new DayUnscheduledEvent
				{
					Date = Date.Date,
					PersonId = Person.Id.Value,
					ScenarioId = Scenario.Id.Value,
					LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
				});
			}

		}

		public virtual DateOnly Date { get; protected set; }

		public virtual string Source
		{
			get { return _source; }
			set { _source = value; }
		}

		public virtual DateTimePeriod Period => mergedMainShiftAndPersonalPeriods();

		public virtual DateTimePeriod PeriodExcludingPersonalActivity()
		{
			// Duplicate mergedMainShiftAndPersonalPeriods but exlude PersonalShiftLayer explicitly.
			DateTimePeriod? mergedPeriod = null;
			foreach (var shiftLayer in ShiftLayers)
			{
				if (shiftLayer.GetType() == typeof(PersonalShiftLayer)) continue;
				mergedPeriod = DateTimePeriod.MaximumPeriod(shiftLayer.Period, mergedPeriod);
			}
			if (mergedPeriod.HasValue)
				return mergedPeriod.Value;

			var dayOff = DayOff();
			return dayOff == null ?
				new DateOnlyPeriod(Date, Date).ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone()) :    //don't like to jump to person aggregate here... "stämpla" assignment with a timezone instead.
				new DateTimePeriod(dayOff.Anchor, dayOff.Anchor.AddTicks(1));
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
				Date.ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone()) : //don't like to jump to person aggregate here... "stämpla" assignment with a timezone instead.
				new DateTimePeriod(dayOff.Anchor, dayOff.Anchor.AddTicks(1));
		}

		public virtual bool BelongsToScenario(IScenario scenario)
		{
			return Scenario.Equals(scenario);
		}

		public virtual string FunctionPath { get; } = DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;

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
			protected set => _shiftCategory = value;
		}

		public virtual IEnumerable<MainShiftLayer> MainActivities()
		{
			return _shiftLayers.OfType<MainShiftLayer>();
		}

		public virtual IEnumerable<PersonalShiftLayer> PersonalActivities()
		{
			return _shiftLayers.OfType<PersonalShiftLayer>();
		}

		public virtual IEnumerable<OvertimeShiftLayer> OvertimeActivities()
		{
			return _shiftLayers.OfType<OvertimeShiftLayer>();
		}

		public virtual IEnumerable<ShiftLayer> ShiftLayers => _shiftLayers;

		public virtual IPerson Person => _person;

		public virtual IScenario Scenario => _scenario;

		public virtual bool RemoveActivity(ShiftLayer layer, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			var removed = _shiftLayers.Remove(layer);

			if (!muteEvent)
			{
				AddEvent(() =>
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
				  if (trackedCommandInfo != null)
				  {
					  activityRemovedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
					  activityRemovedEvent.CommandId = trackedCommandInfo.TrackId;
				  }
				  return activityRemovedEvent;
			  });
			}

			return removed;
		}

		public virtual void ClearPersonalActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			PersonalActivities().ToArray().ForEach(l => RemoveActivity(l, muteEvent, trackedCommandInfo));
		}

		public virtual void ClearMainActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			MainActivities().ToArray().ForEach(l => RemoveActivity(l, muteEvent, trackedCommandInfo));
		}

		public virtual void ClearOvertimeActivities(bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			OvertimeActivities().ToArray().ForEach(l => RemoveActivity(l, muteEvent, trackedCommandInfo));
		}

		public virtual void CheckRestrictions()
		{
			RestrictionSet.CheckEntity(this);
		}

		public virtual IRestrictionSet<IPersonAssignment> RestrictionSet => PersonAssignmentRestrictionSet.CurrentPersonAssignmentRestrictionSet;

		public virtual IProjectionService ProjectionService()
		{
			var proj = new VisualLayerProjectionService();
			if (hasProjection())
			{
				MainActivities().ForEach(l => proj.Add(l, visualLayerFactory));
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
			CloneEvents(retobj);
			retobj._shiftLayers = _shiftLayers.Select(layer =>
			{
				var newLayer = layer.EntityClone();
				newLayer.SetParent(retobj);
				newLayer.SetId(null);
				return newLayer;
			}).ToList();


			return retobj;
		}

		public virtual IPersonAssignment EntityClone()
		{
			var retobj = (PersonAssignment)MemberwiseClone();
			CloneEvents(retobj);
			retobj._shiftLayers = _shiftLayers.Select(layer =>
			{
				var newLayer = layer.EntityClone();
				newLayer.SetParent(retobj);
				return newLayer;
			}).ToList();

			return retobj;
		}

		public virtual IAggregateRoot MainRoot => Person;

		public virtual void AddPersonalActivity(IActivity activity, DateTimePeriod period, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			var layer = new PersonalShiftLayer(activity, period);
			layer.SetParent(this);
			_shiftLayers.Add(layer);

			if (!muteEvent)
			{
				AddEvent(() =>
				{
					var personalActivityAddedEvent = new PersonalActivityAddedEvent
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
						personalActivityAddedEvent.InitiatorId = trackedCommandInfo.OperatedPersonId;
						personalActivityAddedEvent.CommandId = trackedCommandInfo.TrackId;
					}

					return personalActivityAddedEvent;
				});
			}
		}

		public virtual void AddOvertimeActivity(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet multiplicatorDefinitionSet, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			var layer = new OvertimeShiftLayer(activity, period, multiplicatorDefinitionSet);
			layer.SetParent(this);
			_shiftLayers.Add(layer);

			if (!muteEvent)
			{
				AddEvent(() =>
				{
					var activityAddedEvent = new ActivityAddedEvent
					{
						Date = Date.Date,
						PersonId = Person.Id.GetValueOrDefault(),
						ActivityId = activity.Id.GetValueOrDefault(),
						StartDateTime = period.StartDateTime,
						EndDateTime = period.EndDateTime,
						ScenarioId = Scenario.Id.GetValueOrDefault(),
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
		}
		
		public virtual void AddActivity(IActivity activity, DateTime start, DateTime end)
		{
			AddActivity(activity, new DateTimePeriod(start, end), null);
		}

		public virtual void AddActivity(IActivity activity, DateTimePeriod period, bool muteEvent = false)
		{
			AddActivity(activity, period, null, muteEvent);
		}

		public virtual void AddActivity(IActivity activity, TimePeriod period)
		{
			var periodAsDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(Date.Date.Add(period.StartTime),
				Date.Date.Add(period.EndTime),
				Person.PermissionInformation.DefaultTimeZone());
			AddActivity(activity, periodAsDateTimePeriod);
		}

		public virtual void AddActivity(IActivity activity, DateTimePeriod period, TrackedCommandInfo trackedCommandInfo, bool muteEvent = false)
		{
			addActivityInternal(activity, period);
			if (!muteEvent)
			{
				AddEvent(() =>
				{
					var activityAddedEvent = new ActivityAddedEvent
					{
						Date = Date.Date,
						PersonId = Person.Id.GetValueOrDefault(),
						ActivityId = activity.Id.GetValueOrDefault(),
						StartDateTime = period.StartDateTime,
						EndDateTime = period.EndDateTime,
						ScenarioId = Scenario.Id.GetValueOrDefault(),
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
		}

		private void addActivityInternal(IActivity activity, DateTimePeriod period)
		{
			var layer = new MainShiftLayer(activity, period);
			layer.SetParent(this);
			_shiftLayers.Add(layer);
			SetDayOff(null, true);
		}

		public virtual void SetShiftCategory(IShiftCategory shiftCategory, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			_shiftCategory = shiftCategory;
			if (!muteEvent)
			{
				AddEvent(() =>
				{
					var @event = new MainShiftCategoryReplaceEvent
					{
						Date = Date.Date,
						PersonId = Person.Id.Value,
						ScenarioId = Scenario.Id.Value,
						LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
					};
					if (trackedCommandInfo != null)
					{
						@event.InitiatorId = trackedCommandInfo.OperatedPersonId;
						@event.CommandId = trackedCommandInfo.TrackId;
					}
					return @event;
				});
			}
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
			Clear();
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
			SetDayOff(null, true);

		}

		public virtual void InsertOvertimeLayer(IActivity activity, DateTimePeriod period, int index,
										IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
		{

			var layer = new OvertimeShiftLayer(activity, period, multiplicatorDefinitionSet);
			layer.SetParent(this);
			_shiftLayers.Insert(index, layer);
		}

		public virtual void InsertPersonalLayer(IActivity activity, DateTimePeriod period, int index)
		{
			var layer = new PersonalShiftLayer(activity, period);
			layer.SetParent(this);
			_shiftLayers.Insert(index, layer);
		}

		public virtual void MoveLayerDown(ShiftLayer shiftLayer)
		{
			var t = shiftLayer.GetType();
			var currentIndex = _shiftLayers.IndexOf(shiftLayer);
			var toBeReplaced = _shiftLayers.Skip(currentIndex + 1).First(l => l.GetType() == t);

			moveLayer(_shiftLayers.IndexOf(toBeReplaced), currentIndex);
		}

		public virtual void MoveLayerUp(ShiftLayer shiftLayer)
		{
			var t = shiftLayer.GetType();
			var currentIndex = _shiftLayers.IndexOf(shiftLayer);
			var toBeReplaced = _shiftLayers.Take(currentIndex).Last(l => l.GetType() == t);
			var indexToInsert = _shiftLayers.IndexOf(toBeReplaced);

			moveLayer(currentIndex, indexToInsert);
		}

		public virtual bool HasDayOffOrMainShiftLayer()
		{
			return DayOffTemplate != null || MainActivities().Any();
		}

		private void moveLayer(int currentIndex, int newIndex)
		{
			var layer = _shiftLayers[currentIndex];
			_shiftLayers.RemoveAt(currentIndex);
			if (newIndex > currentIndex)
				newIndex--;
			_shiftLayers.Insert(newIndex, layer);
		}

		public virtual void MoveActivityAndSetHighestPriority(IActivity activity, DateTime currentStartTime, DateTime newStartTime, TimeSpan length, TrackedCommandInfo trackedCommandInfo)
		{
			var anyLayerFound = false;

			foreach (var layer in ShiftLayers.ToArray())
			{
				var currentLayerPeriod = new DateTimePeriod(currentStartTime, currentStartTime.Add(length));
				if (!layer.Payload.Equals(activity) || !layer.Period.Intersect(currentLayerPeriod))
					continue;
				var originalOrderIndex = ShiftLayers.ToList().IndexOf(layer);
				RemoveActivity(layer);

				var layerContainsCurrentLayerPeriod = layer.Period;
				var splittedLayerPeriods = layerContainsCurrentLayerPeriod.Subtract(currentLayerPeriod);
				splittedLayerPeriods.ForEach(period => InsertActivity(layer.Payload, period, originalOrderIndex));
				anyLayerFound = true;
			}
			if (!anyLayerFound)
				throw new ArgumentException("No layer(s) found!", nameof(activity));

			//will be fixed later (=Erik)
			var newPeriod = new DateTimePeriod(newStartTime, newStartTime.Add(length));
			///////////////////////////////

			addActivityInternal(activity, newPeriod);

			var affectedPeriod = new DateTimePeriod(currentStartTime, currentStartTime.Add(length)).MaximumPeriod(newPeriod);

			AddEvent(() =>
			{
				var activityMovedEvent = new ActivityMovedEvent
				{
					PersonId = Person.Id.Value,
					StartDateTime = affectedPeriod.StartDateTime,
					EndDateTime = affectedPeriod.EndDateTime,
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

		public virtual void MoveAllActivitiesAndKeepOriginalPriority(DateTime newStartTimeInUtc,
			TrackedCommandInfo trackedCommandInfo, bool muteEvent = false)
		{
			var originalStartTimeInUtc = PeriodExcludingPersonalActivity().StartDateTime;
			var distanceToMove = newStartTimeInUtc.Subtract(originalStartTimeInUtc);

			var shiftLayers = ShiftLayers.ToList();

			var affectedPeriods = new List<DateTimePeriod>();

			foreach (var shiftLayer in shiftLayers)
			{
				if (shiftLayer is PersonalShiftLayer)
					continue;

				var newPeriod = shiftLayer.Period.MovePeriod(distanceToMove);
				var index = shiftLayers.IndexOf(shiftLayer);
				RemoveActivity(shiftLayer);

				if (shiftLayer is OvertimeShiftLayer overtimeLayer)
				{
					InsertOvertimeLayer(overtimeLayer.Payload, newPeriod, index, overtimeLayer.DefinitionSet);
				}
				else
				{
					InsertActivity(shiftLayer.Payload, newPeriod, index);
				}

				affectedPeriods.Add(shiftLayer.Period.MaximumPeriod(newPeriod));
			}

			if (!muteEvent)
			{
				AddEvent(() =>
				{
					var activityMovedEvent = new ActivityMovedEvent
					{
						PersonId = Person.Id.Value,
						StartDateTime = affectedPeriods.Min(p => p.StartDateTime),
						EndDateTime = affectedPeriods.Max(p => p.EndDateTime),
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
		}

		public virtual void MoveActivityAndKeepOriginalPriority(ShiftLayer shiftLayer, DateTime newStartTimeInUtc, TrackedCommandInfo trackedCommandInfo, bool muteEvent = false)
		{
			var originalOrderIndex = ShiftLayers.ToList().IndexOf(shiftLayer);
			if (originalOrderIndex < 0)
				throw new ArgumentException("No layer(s) found!", nameof(shiftLayer));
			RemoveActivity(shiftLayer);

			var newLayerPeriod = new DateTimePeriod(newStartTimeInUtc, newStartTimeInUtc.Add(shiftLayer.Period.EndDateTime.Subtract(shiftLayer.Period.StartDateTime)));
			if (shiftLayer is MainShiftLayer)
			{
				InsertActivity(shiftLayer.Payload, newLayerPeriod, originalOrderIndex);
			}
			else if (shiftLayer is PersonalShiftLayer)
			{
				InsertPersonalLayer(shiftLayer.Payload, newLayerPeriod, originalOrderIndex);
			}
			else if (shiftLayer is OvertimeShiftLayer overtimeLayer)
			{
				InsertOvertimeLayer(overtimeLayer.Payload, newLayerPeriod, originalOrderIndex,  overtimeLayer.DefinitionSet);
			}

			var affectedPeriod = shiftLayer.Period.MaximumPeriod(newLayerPeriod);
			if (!muteEvent)
			{
				AddEvent(() =>
				{
					var activityMovedEvent = new ActivityMovedEvent
					{
						PersonId = Person.Id.Value,
						StartDateTime = affectedPeriod.StartDateTime,
						EndDateTime = affectedPeriod.EndDateTime,
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

		}

		public virtual DayOff DayOff()
		{
			if (_dayOffTemplate == null)
				return null;
			//don't like to jump to person aggregate here... "stämpla" assignment with a timezone instead.
			var anchorDateTime = TimeZoneHelper.ConvertToUtc(Date.Date.Add(_dayOffTemplate.Anchor), Person.PermissionInformation.DefaultTimeZone());
			return new DayOff(anchorDateTime, _dayOffTemplate.TargetLength, _dayOffTemplate.Flexibility, _dayOffTemplate.Description, _dayOffTemplate.DisplayColor, _dayOffTemplate.PayrollCode, _dayOffTemplate.Id.GetValueOrDefault());
		}

		public virtual IDayOffTemplate DayOffTemplate => _dayOffTemplate;

		public virtual void SetDayOff(IDayOffTemplate template, bool muteEvent = false, TrackedCommandInfo trackedCommandInfo = null)
		{
			_dayOffTemplate = template;
			if (_dayOffTemplate == null)
			{
				if (!muteEvent)
				{
					AddEvent(() =>
					{
						var @event = new DayOffDeletedEvent
						{
							Date = Date.Date,
							PersonId = Person.Id.Value,
							ScenarioId = Scenario.Id.Value,
							LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
						};
						if (trackedCommandInfo != null)
						{
							@event.InitiatorId = trackedCommandInfo.OperatedPersonId;
							@event.CommandId = trackedCommandInfo.TrackId;
						}
						return @event;
					});
				}
				return;
			}

			ClearMainActivities();

			if (!muteEvent)
			{
				AddEvent(() =>
				{
					var @event = new DayOffAddedEvent
					{
						Date = Date.Date,
						PersonId = Person.Id.Value,
						ScenarioId = Scenario.Id.Value,
						LogOnBusinessUnitId = Scenario.BusinessUnit.Id.GetValueOrDefault()
					};
					if (trackedCommandInfo != null)
					{
						@event.InitiatorId = trackedCommandInfo.OperatedPersonId;
						@event.CommandId = trackedCommandInfo.TrackId;
					}
					return @event;
				});
			}
		}

		public virtual void SetThisAssignmentsDayOffOn(IPersonAssignment dayOffDestination, bool muteEvent = false, TrackedCommandInfo trackedCommandInfo = null)
		{
			dayOffDestination.SetDayOff(_dayOffTemplate, muteEvent, trackedCommandInfo);
		}

		public virtual bool AssignedWithDayOff(IDayOffTemplate template)
		{
			if (_dayOffTemplate == null)
				return template == null;
			return _dayOffTemplate.Equals(template);
		}

		public virtual void FillWithDataFrom(IPersonAssignment personAssignmentSource, bool muteEvent = false)
		{
			Clear(true);
			personAssignmentSource.SetThisAssignmentsDayOffOn(this, muteEvent);
			SetShiftCategory(personAssignmentSource.ShiftCategory);
			foreach (var mainLayer in personAssignmentSource.MainActivities())
			{
				AddActivity(mainLayer.Payload, mainLayer.Period, muteEvent);
			}
			foreach (var personalLayer in personAssignmentSource.PersonalActivities())
			{
				AddPersonalActivity(personalLayer.Payload, personalLayer.Period, muteEvent);
			}
			foreach (var overtimeLayer in personAssignmentSource.OvertimeActivities())
			{
				AddOvertimeActivity(overtimeLayer.Payload, overtimeLayer.Period, overtimeLayer.DefinitionSet, muteEvent);
			}
		}
		

		#region Equals

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((IEntity)obj);
		}

		public override bool Equals(IEntity other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			var otherAsAss = other as IPersonAssignment;
			return otherAsAss != null && Equals(_person, otherAsAss.Person) && Equals(_scenario, otherAsAss.Scenario) && Date.Equals(otherAsAss.Date);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Date.GetHashCode();
				hashCode = (hashCode * 397) ^ (_person?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (_scenario?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		#endregion
	}
}
