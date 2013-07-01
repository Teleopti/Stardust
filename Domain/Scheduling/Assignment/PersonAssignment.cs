using System;
using System.Collections.Generic;
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
		private IList<IShiftLayer> _shiftLayers;
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
			_shiftLayers = new List<IShiftLayer>();
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
			foreach (var mainShiftActivityLayer in MainLayers)
			{
				mergedPeriod = DateTimePeriod.MaximumPeriod(mainShiftActivityLayer.Period, mergedPeriod);
			}
			foreach (var personalLayer in PersonalLayers)
			{
				mergedPeriod = DateTimePeriod.MaximumPeriod(personalLayer.Period, mergedPeriod);
			}
			foreach (var overtimeLayer in OvertimeLayers)
			{
				mergedPeriod = DateTimePeriod.MaximumPeriod(overtimeLayer.Period, mergedPeriod);
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
				if (!MainLayers.Any())
					return null;

				return _shiftCategory;
			}
			protected set { _shiftCategory = value; }
		}

		public virtual IEnumerable<IMainShiftLayer> MainLayers
		{
			get { return _shiftLayers.OfType<IMainShiftLayer>(); }
		}

		public virtual IEnumerable<IPersonalShiftLayer> PersonalLayers
		{
			get { return _shiftLayers.OfType<IPersonalShiftLayer>(); }
		}

		public virtual IEnumerable<IOvertimeShiftLayer> OvertimeLayers
		{
			get { return _shiftLayers.OfType<IOvertimeShiftLayer>(); }
		}

		public virtual void SetMainShiftLayers(IEnumerable<IMainShiftLayer> activityLayers, IShiftCategory shiftCategory)
		{
			//todo: make sure not reusing layer from another assignment...
			//* either do a check here or 
			//* don't expose and accept IMainShiftACtivityLayerNew but another type

			InParameter.ListCannotBeEmpty("activityLayers", activityLayers);
			//clear or new list?
			ClearMainLayers();
			activityLayers.ForEach(layer =>
			{
				layer.SetParent(this);
				_shiftLayers.Add(layer);
			});
			ShiftCategory = shiftCategory;
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
			return _shiftLayers.Remove(layer);
		}

		public virtual bool RemoveLayer(IPersonalShiftLayer layer)
		{
			return _shiftLayers.Remove(layer);
		}

		public virtual bool RemoveLayer(IOvertimeShiftLayer layer)
		{
			return _shiftLayers.Remove(layer);
		}

		public virtual void ClearPersonalLayers()
		{
			//todo: do smarter
			foreach (var layer in _shiftLayers.OfType<IPersonalShiftLayer>())
			{
				_shiftLayers.Remove(layer);
			}
		}

		public virtual void ClearMainLayers()
		{
			//todo: do smarter
			foreach (var layer in _shiftLayers.OfType<IMainShiftLayer>())
			{
				_shiftLayers.Remove(layer);
			}
		}

		public virtual void ClearOvertimeLayers()
		{
			//todo: do smarter
			foreach (var layer in _shiftLayers.OfType<IOvertimeShiftLayer>())
			{
				_shiftLayers.Remove(layer);
			}
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
			if (HasProjection)
			{
				proj.Add(MainLayers, new VisualLayerFactory());
				var validPeriods = new HashSet<DateTimePeriod>(MainLayers.PeriodBlocks());
				foreach (var overtimeLayer in OvertimeLayers)
				{
					var overTimePeriod = overtimeLayer.Period;
					proj.Add(overtimeLayer, new VisualLayerOvertimeFactory());
					validPeriods.Add(overTimePeriod);
				}
				foreach (var personalLayer in PersonalLayers)
				{
					if (validPeriods.Any(validPeriod => validPeriod.Intersect(personalLayer.Period) || validPeriod.AdjacentTo(personalLayer.Period)))
					{
						proj.Add(personalLayer, new VisualLayerFactory());
					}
				}
			}

			return proj;
		}

		public virtual bool HasProjection
		{
			get
			{
				return MainLayers.Any() || OvertimeLayers.Any();
			}
		}

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
			var retobj = (PersonAssignment)MemberwiseClone();
			retobj.SetId(null);
			retobj._shiftLayers = new List<IShiftLayer>();
			//todo: no need to cast here when interfaces are correct
			foreach (IShiftLayer newLayer in _shiftLayers.Select(layer => layer.NoneEntityClone()))
			{
				newLayer.SetParent(retobj);
				retobj._shiftLayers.Add(newLayer);
			}

			return retobj;
		}

		public virtual IPersonAssignment EntityClone()
		{
			var retobj = (PersonAssignment)MemberwiseClone();
			retobj._shiftLayers = new List<IShiftLayer>();
			//todo: no need to cast here when interfaces are correct
			foreach (IShiftLayer newLayer in _shiftLayers.Select(layer => layer.EntityClone()))
			{
				newLayer.SetParent(retobj);
				retobj._shiftLayers.Add(newLayer);
			}

			return retobj;
		}

		#endregion

		public virtual IAggregateRoot MainRoot
		{
			get { return Person; }
		}

		public virtual void AddPersonalLayer(IActivity activity, DateTimePeriod period)
		{
			var layer = new PersonalShiftLayer(activity, period);
			layer.SetParent(this);
			_shiftLayers.Add(layer);
		}

		public virtual void AddOvertimeLayer(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
		{
			var layer = new OvertimeShiftLayer(activity, period, multiplicatorDefinitionSet);
			layer.SetParent(this);
			_shiftLayers.Add(layer);
		}
	}
}
