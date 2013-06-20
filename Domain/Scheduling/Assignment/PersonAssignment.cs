using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersonAssignment : AggregateRootWithBusinessUnit, 
									IPersonAssignment,
									IExportToAnotherScenario
	{
		public static readonly DateTimePeriod UndefinedPeriod = new DateTimePeriod(1800,1,1,1800,1,2);

		private IList<IPersonalShift> _personalShiftCollection;
		private IList<IOvertimeShift> _overtimeShiftCollection;
		private IList<IMainShiftLayer> _mainLayers;
		private IList<IPersonalShiftLayer> _personalLayers;
		private IPerson _person;
		private IScenario _scenario;
		private DateTime _zorder;
		private IShiftCategory _shiftCategory;


		public PersonAssignment(IPerson agent, IScenario scenario, DateOnly date)
		{
			Date = date;
			_person = agent;
			_scenario = scenario;
			_zorder = DateTime.MinValue;
			_personalShiftCollection = new List<IPersonalShift>();
			_overtimeShiftCollection = new List<IOvertimeShift>();
			_mainLayers = new List<IMainShiftLayer>();
			_personalLayers = new List<IPersonalShiftLayer>();
		}

		protected PersonAssignment()
		{
		}

		public virtual DateOnly Date { get; protected set; }

		public virtual DateTimePeriod Period
		{
			get { return mergedMainShiftAndPersonalPeriods(); }
		}

		private DateTimePeriod mergedMainShiftAndPersonalPeriods()
		{
			DateTimePeriod? mergedPeriod = null;
			//fix later 
			foreach (var mainShiftActivityLayer in MainLayers)
			{
				mergedPeriod = DateTimePeriod.MaximumPeriod(mainShiftActivityLayer.Period, mergedPeriod);
			}
			foreach (IPersonalShift personalShift in _personalShiftCollection)
			{
				mergedPeriod = DateTimePeriod.MaximumPeriod(personalShift.LayerCollection.Period(), mergedPeriod);
			}
			foreach (IOvertimeShift overtimeShift in _overtimeShiftCollection)
			{
				mergedPeriod = DateTimePeriod.MaximumPeriod(overtimeShift.LayerCollection.Period(), mergedPeriod);
			}
			if (!mergedPeriod.HasValue)
			{
				mergedPeriod = UndefinedPeriod;
			}
			return mergedPeriod.Value;
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
				if (!_mainLayers.Any())
					return null;

				return _shiftCategory;
			}
			protected set { _shiftCategory = value; }
		}

		public virtual IEnumerable<IMainShiftLayer> MainLayers
		{
			get { return _mainLayers; }
		}

		public IEnumerable<IPersonalShiftLayer> PersonalLayers
		{
			get { return _personalLayers; }
		}

		public virtual void SetMainShiftLayers(IEnumerable<IMainShiftLayer> activityLayers, IShiftCategory shiftCategory)
		{
			//todo: make sure not reusing layer from another assignment...
			//* either do a check here or 
			//* don't expose and accept IMainShiftACtivityLayerNew but another type

			InParameter.ListCannotBeEmpty("activityLayers", activityLayers);
			//clear or new list?
			ClearMainShiftLayers();
			activityLayers.ForEach(layer =>
			{
				layer.SetParent(this);
				_mainLayers.Add(layer);
			});
			ShiftCategory = shiftCategory;
		}
	
		public virtual ReadOnlyCollection<IPersonalShift> PersonalShiftCollection
		{
			get { return new ReadOnlyCollection<IPersonalShift>(_personalShiftCollection); }
		}

		public virtual IPerson Person
		{
			get { return _person; }
		}

		public virtual IScenario Scenario
		{
			get { return _scenario; }
		}

		public virtual DateTime ZOrder
		{
			get { return _zorder; }
			set { _zorder = value; }
		}

		public virtual ReadOnlyCollection<IOvertimeShift> OvertimeShiftCollection
		{
			get { return new ReadOnlyCollection<IOvertimeShift>(_overtimeShiftCollection); }
		}

		public virtual void AddOvertimeShift(IOvertimeShift overtimeShift)
		{
			overtimeShift.SetParent(this);
			_overtimeShiftCollection.Add(overtimeShift);
		}

		public virtual void RemoveOvertimeShift(IOvertimeShift overtimeShift)
		{
			_overtimeShiftCollection.Remove(overtimeShift);
		}

		public virtual void ScheduleChanged(string dataSource)
		{
			AddEvent(new ScheduleChangedEvent
				{
					Datasource = dataSource,
					BusinessUnitId = _scenario.BusinessUnit.Id.Value,
					ScenarioId = Scenario.Id.Value,
					StartDateTime = Period.StartDateTime,
					EndDateTime = Period.EndDateTime,
					PersonId = Person.Id.Value,
				});
		}

		public virtual bool RemoveLayer(IMainShiftLayer layer)
		{
			return _mainLayers.Remove(layer);
		}

		#region Manipulate personalshifts

		public virtual void AddPersonalShift(IPersonalShift personalShift)
		{
			InsertPersonalShiftInternal(personalShift, _personalShiftCollection.Count);
		}

		private void InsertPersonalShiftInternal(IPersonalShift personalShift, int index)
		{
			InParameter.NotNull("personalShift", personalShift);
			if (_personalShiftCollection.Contains(personalShift))
			{
				if (personalShift.OrderIndex < index)
					index = index - 1;
				_personalShiftCollection.Remove(personalShift);
			}
			_personalShiftCollection.Insert(index, personalShift);
			personalShift.SetParent(this);
		}

		public virtual void ClearPersonalShift()
		{
			_personalShiftCollection.Clear();
		}

		public virtual void RemovePersonalShift(IPersonalShift personalShift)
		{
			_personalShiftCollection.Remove(personalShift);
		}

		#endregion

		#region Manipulate MainShift

		public virtual void ClearMainShiftLayers()
		{
			_mainLayers.Clear();
		}

		#endregion

		#region IRestrictionChecker Members

		public virtual void CheckRestrictions()
		{
			RestrictionSet.CheckEntity(this);
		}

		public virtual IRestrictionSet<IPersonAssignment> RestrictionSet
		{
			get { return PersonAssignmentRestrictionSet.CurrentPersonAssignmentRestrictionSet; }
		}

		#endregion

		#region IProjection<Activity> Members

		public virtual IProjectionService ProjectionService()
		{
			var proj = new VisualLayerProjectionService(Person);
			if (HasProjection)
			{
				proj.Add(MainLayers, new VisualLayerFactory());
				var validPeriods = new List<DateTimePeriod>(MainLayers.PeriodBlocks());
				foreach (var overtimeShift in _overtimeShiftCollection)
				{
					var overTimePeriod = overtimeShift.LayerCollection.Period();
					proj.Add(overtimeShift.LayerCollection, new VisualLayerOvertimeFactory());
					if (overTimePeriod.HasValue)
						validPeriods.Add(overTimePeriod.Value);
				}
				foreach (var personalShift in _personalShiftCollection)
				{
					var persShiftPeriod = personalShift.LayerCollection.Period();
					if (persShiftPeriod.HasValue)
					{
						if (validPeriods.Any(validPeriod => validPeriod.Intersect(persShiftPeriod.Value) || validPeriod.AdjacentTo(persShiftPeriod.Value)))
						{
							proj.Add(personalShift.LayerCollection, new VisualLayerFactory());
						}
					}
				}
			}

			return proj;
		}

		public virtual bool HasProjection
		{
			get
			{
				return MainLayers.Any() || _overtimeShiftCollection.Count > 0;
			}
		}
		#endregion

		#region ICloneableEntity<PersonAssignment> Members

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
			PersonAssignment retobj = (PersonAssignment)MemberwiseClone();
			retobj.SetId(null);
			retobj._personalShiftCollection = new List<IPersonalShift>();
			retobj._overtimeShiftCollection = new List<IOvertimeShift>();
			//fix this!
			retobj._mainLayers = new List<IMainShiftLayer>();
			foreach (IMainShiftLayer newLayer in _mainLayers.Select(layer => layer.NoneEntityClone()))
			{
				newLayer.SetParent(retobj);
				retobj._mainLayers.Add(newLayer);
			}
			//
			foreach (IPersonalShift shift in _personalShiftCollection)
			{
				retobj.AddPersonalShift((IPersonalShift)shift.NoneEntityClone());
			}
			foreach (IOvertimeShift overtimeShift in _overtimeShiftCollection)
			{
				retobj.AddOvertimeShift((IOvertimeShift)overtimeShift.NoneEntityClone());
			}

			return retobj;
		}

		public virtual IPersonAssignment EntityClone()
		{
			PersonAssignment retobj = (PersonAssignment)MemberwiseClone();
			retobj._personalShiftCollection = new List<IPersonalShift>();
			retobj._overtimeShiftCollection = new List<IOvertimeShift>();
			//fix this
			retobj._mainLayers = new List<IMainShiftLayer>();
			foreach (IMainShiftLayer newLayer in _mainLayers.Select(layer => layer.EntityClone()))
			{
				newLayer.SetParent(retobj);
				retobj._mainLayers.Add(newLayer);
			}
			//
			foreach (IPersonalShift shift in _personalShiftCollection)
			{
				retobj.AddPersonalShift((PersonalShift)shift.EntityClone());
			}
			foreach (IOvertimeShift overtimeShift in _overtimeShiftCollection)
			{
				retobj.AddOvertimeShift((IOvertimeShift)overtimeShift.EntityClone());
			}


			return retobj;
		}

		#endregion

		public virtual IAggregateRoot MainRoot
		{
			get { return Person; }
		}

		public void AddPersonalLayer(IActivity activity, DateTimePeriod period)
		{
			var layer = new PersonalShiftLayer(activity, period);
			layer.SetParent(this);
			_personalLayers.Add(layer);
		}
	}
}
