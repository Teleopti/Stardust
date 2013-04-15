using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private IMainShift _mainShift;
		private IPerson _person;
		private IScenario _scenario;
		private DateTime _zorder;


		public PersonAssignment(IPerson agent, IScenario scenario, DateOnly date)
		{
			Date = date;
			_person = agent;
			_scenario = scenario;
			_zorder = DateTime.MinValue;
			_personalShiftCollection = new List<IPersonalShift>();
			_overtimeShiftCollection = new List<IOvertimeShift>();
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
			if (MainShift != null)
				mergedPeriod = DateTimePeriod.MaximumPeriod(MainShift.LayerCollection.Period(), mergedPeriod);
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


		public virtual IMainShift MainShift
		{
			get { return _mainShift; }
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


		#region Manipulate personalshifts

		public virtual void AddPersonalShift(IPersonalShift personalShift)
		{
			InsertPersonalShiftInternal(personalShift, _personalShiftCollection.Count);
		}

		public virtual void InsertPersonalShift(IPersonalShift personalShift, int index)
		{
			InsertPersonalShiftInternal(personalShift, index);
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

		public virtual void ClearMainShift(IPersonAssignmentRepository personAssignmentRepository)
		{
			InParameter.NotNull("personAssignmentRepository", personAssignmentRepository);
			personAssignmentRepository.RemoveMainShift(this);
			_mainShift = null;
		}

		public virtual void SetMainShift(IMainShift mainShift)
		{
			InParameter.NotNull("mainShift", mainShift); //use ClearMainShift method instead!
			_mainShift = mainShift;
			((AggregateEntity)_mainShift).SetParent(this);
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
				var validPeriods = new List<DateTimePeriod>();
				if (MainShift != null)
				{
					proj.Add(MainShift);
					var mainShiftPeriod = MainShift.LayerCollection.Period();
					if (mainShiftPeriod.HasValue)
						validPeriods.Add(mainShiftPeriod.Value);
				}
				foreach (var overtimeShift in _overtimeShiftCollection)
				{
					var overTimePeriod = overtimeShift.LayerCollection.Period();
					proj.Add(overtimeShift);
					if (overTimePeriod.HasValue)
						validPeriods.Add(overTimePeriod.Value);
				}
				foreach (var personalShift in _personalShiftCollection)
				{
					var persShiftPeriod = personalShift.LayerCollection.Period();
					if (persShiftPeriod.HasValue)
					{
						if (validPeriods.Any(validPeriod => validPeriod.Intersect(persShiftPeriod.Value) || validPeriod.Adjacent(persShiftPeriod.Value)))
						{
							proj.Add(personalShift);
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
				return (MainShift != null && MainShift.HasProjection) || (_overtimeShiftCollection.Count > 0);
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
			return dateAndPeriod.Period().Contains(Period.StartDateTime);
		}

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            DateTimePeriod dateTimePeriod =
               dateOnlyPeriod.ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone());

            return dateTimePeriod.Contains(Period.StartDateTime);
        }

		public virtual IPersonAssignment NoneEntityClone()
		{
			PersonAssignment retobj = (PersonAssignment)MemberwiseClone();
			retobj.SetId(null);
			retobj._personalShiftCollection = new List<IPersonalShift>();
			retobj._overtimeShiftCollection = new List<IOvertimeShift>();
			if (MainShift != null)
				retobj.SetMainShift((MainShift)MainShift.NoneEntityClone());
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
			if (MainShift != null)
				retobj.SetMainShift((MainShift)MainShift.EntityClone());
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

		/// <summary>
		/// The period stored in the database. Must not be set explicitly.
		/// Note - this value will not ever change in ram, not even when
		/// object is persisted. It will only be set when fetched from database
		/// </summary>
		public virtual DateTimePeriod DatabasePeriod { get; protected set; }

		public override bool Equals(IEntity other)
		{
			//to prevent equal with Mainshift (has same Id)
			if (!(other is IPersonAssignment))
				return false;
			return base.Equals(other);
		}
	}
}
