using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using InParameter = Teleopti.Interfaces.Domain.InParameter;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{

	/// <summary>
    /// Holding rules, limiters and extenders, used
    /// when creating work shifts.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-20
    /// </remarks>
    public class WorkShiftRuleSet : VersionedAggregateRootWithBusinessUnit, IWorkShiftRuleSet
    {
		private IList<IWorkShiftExtender> _extenderCollection;
        private IList<IWorkShiftLimiter> _limiterCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private IWorkShiftTemplateGenerator _templateGenerator;
        private DefaultAccessibility _defaultAccessibility = DefaultAccessibility.Included;

		private ISet<DayOfWeek> _accessibilityDaysOfWeek;
		private ISet<DateTime> _accessibilityDates;

		private Description _description;
		private bool _onlyForRestrictions;

		public WorkShiftRuleSet(IWorkShiftTemplateGenerator generator)
        {
            InParameter.NotNull("generator", generator);
            _extenderCollection = new List<IWorkShiftExtender>();
            _limiterCollection = new List<IWorkShiftLimiter>();
            _ruleSetBagCollection = new List<IRuleSetBag>();
            _templateGenerator = generator;
			_accessibilityDaysOfWeek = new HashSet<DayOfWeek>();
			_accessibilityDates = new HashSet<DateTime>();

        }

		protected WorkShiftRuleSet() { }

		#region Properties (6) 
		// ReSharper disable ConvertToAutoProperty
		public virtual Description Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public virtual bool OnlyForRestrictions

		{
			get { return _onlyForRestrictions; }
			set { _onlyForRestrictions = value; }
		}
		// ReSharper restore ConvertToAutoProperty

		public virtual ReadOnlyCollection<IWorkShiftExtender> ExtenderCollection
        {
            get { return new ReadOnlyCollection<IWorkShiftExtender>(_extenderCollection); }
        }

        public virtual ReadOnlyCollection<IWorkShiftLimiter> LimiterCollection
        {
            get { return new ReadOnlyCollection<IWorkShiftLimiter>(_limiterCollection); }
        }

        public virtual ReadOnlyCollection<IRuleSetBag> RuleSetBagCollection
        {
            get { return new ReadOnlyCollection<IRuleSetBag>(_ruleSetBagCollection); }
        }

        public virtual void RemoveRuleSetBag(IRuleSetBag ruleSetBag)
        {
            InParameter.NotNull("ruleSetBag", ruleSetBag);
            var concrete = ruleSetBag as RuleSetBag;
            if (concrete != null)
                concrete.RuleSetCollectionWritable.Remove(this);
            _ruleSetBagCollection.Remove(ruleSetBag);
        }

        
        public virtual ICollection<IRuleSetBag> RuleSetBagCollectionWritable
        {
	        get { return _ruleSetBagCollection; }
        }

        public virtual IWorkShiftTemplateGenerator TemplateGenerator
        {
            get { return _templateGenerator; }
            set { _templateGenerator = value; }
        }
        
        public virtual DefaultAccessibility DefaultAccessibility
        {
            get { return _defaultAccessibility; }
            set { _defaultAccessibility = value; }
        }

        public virtual IEnumerable<DayOfWeek> AccessibilityDaysOfWeek
        {
            get { return _accessibilityDaysOfWeek; }
        }

				public virtual IEnumerable<DateTime> AccessibilityDates
        {
            get { return _accessibilityDates; }
        }
		#endregion Properties 

		#region Methods (4) 
		public virtual void AddExtender(IWorkShiftExtender extender)
        {
            InParameter.NotNull("extender", extender);
            if(extender.Parent != null)
                throw new ArgumentException("extender.Parent nust be null");
            extender.SetParent(this);
            _extenderCollection.Add(extender);
        }

        public virtual void AddLimiter(IWorkShiftLimiter limiter)
        {
            InParameter.NotNull("limiter", limiter);
            if (limiter.Parent != null)
                throw new ArgumentException("limiter.Parent nust be null");
            limiter.SetParent(this);
            _limiterCollection.Add(limiter);
        }

		public virtual void InsertExtender(int index, IWorkShiftExtender extender)
		{
			InParameter.NotNull("extender", extender);
			if (extender.Parent != null)
				throw new ArgumentException("extender.Parent must be null");
			extender.SetParent(this);
			_extenderCollection.Insert(index, extender);
		}

		/// <summary>
        /// Deletes the extender. The difference from RemoveExtender
        /// is that the DeleteExtender method does not set the Parent property to null.
        /// </summary>
        /// <param name="extender">The extender.</param>
        public virtual void DeleteExtender(IWorkShiftExtender extender)
        {
            _extenderCollection.Remove(extender);
        }

        /// <summary>
        /// Deletes the limiter. The difference from RemoveLimiter
        /// is that the DeleteLimiter method does not set the Parent property to null.
        /// </summary>
        /// <param name="limiter">The limiter.</param>
        public virtual void DeleteLimiter(IWorkShiftLimiter limiter)
        {
            _limiterCollection.Remove(limiter);
        }

        public virtual void AddAccessibilityDayOfWeek(DayOfWeek dayOfWeek)
        {
	        _accessibilityDaysOfWeek.Add(dayOfWeek);
        }

        public virtual void RemoveAccessibilityDayOfWeek(DayOfWeek dayOfWeek)
        {
            _accessibilityDaysOfWeek.Remove(dayOfWeek);
        }

        public virtual void AddAccessibilityDate(DateTime dateTime)
        {
            InParameter.VerifyDateIsUtc("dateTime",dateTime);
            dateTime = dateTime.Date;
						_accessibilityDates.Add(dateTime);
        }

        public virtual void RemoveAccessibilityDate(DateTime dateTime)
        {
            _accessibilityDates.Remove(DateTime.SpecifyKind(dateTime.Subtract(dateTime.TimeOfDay), DateTimeKind.Utc));
        }

        public virtual bool IsValidDate(DateOnly dateToCheck)
        {
            var dateInList = _accessibilityDates.Contains(dateToCheck.Date);
            var dayOfWeekInList = _accessibilityDaysOfWeek.Contains(dateToCheck.DayOfWeek);
            var availableByDefault = (_defaultAccessibility == DefaultAccessibility.Included);

            if (dateInList || dayOfWeekInList)
                availableByDefault = !availableByDefault;

            return availableByDefault;
        }

        public virtual void SwapExtenders(IWorkShiftExtender left, IWorkShiftExtender right)
        {
            int top = _extenderCollection.IndexOf(left);
            int bottom = _extenderCollection.IndexOf(right);

            if (top != -1 && bottom != -1)
            {
                _extenderCollection[top] = right;
                _extenderCollection[bottom] = left;
            }
        }

        private void reinitializeFields()
        {
	        _accessibilityDaysOfWeek = new HashSet<DayOfWeek>();
	        _accessibilityDates = new HashSet<DateTime>();
        }
        #endregion Methods 

        #region ICloneableEntity<WorkShiftRuleSet> Members (2) 

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-16
        /// </remarks>
        public virtual IWorkShiftRuleSet NoneEntityClone()
        {
            var retobj = (WorkShiftRuleSet)MemberwiseClone();
            retobj.SetId(null);
            retobj.reinitializeFields();
            
            if (TemplateGenerator != null)
                retobj.TemplateGenerator = TemplateGenerator.NoneEntityClone();

            retobj._limiterCollection = new List<IWorkShiftLimiter>();
            foreach (IWorkShiftLimiter limiter in _limiterCollection)
            {
                IWorkShiftLimiter limiterCloned = limiter.NoneEntityClone();
                limiterCloned.SetParent(retobj);
                retobj._limiterCollection.Add(limiterCloned);
            }

            retobj._extenderCollection = new List<IWorkShiftExtender>();
            foreach (IWorkShiftExtender extender in _extenderCollection)
            {
                IWorkShiftExtender extenderCloned = extender.NoneEntityClone();
                extenderCloned.SetParent(retobj);
                retobj._extenderCollection.Add(extenderCloned);
            }

            foreach(DayOfWeek dayOfWeek in _accessibilityDaysOfWeek)
                retobj.AddAccessibilityDayOfWeek(dayOfWeek);

            foreach (DateTime date in _accessibilityDates)
                retobj.AddAccessibilityDate(date);

            retobj._ruleSetBagCollection = new List<IRuleSetBag>();

            return retobj;
        }


        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-16
        /// </remarks>
        public virtual IWorkShiftRuleSet EntityClone()
        {
            var retobj = (WorkShiftRuleSet)MemberwiseClone();

            if (TemplateGenerator != null)
                retobj.TemplateGenerator = TemplateGenerator.EntityClone();

            retobj._limiterCollection = new List<IWorkShiftLimiter>();
            foreach (IWorkShiftLimiter limiter in _limiterCollection)
            {
                IWorkShiftLimiter limiterCloned = limiter.EntityClone();
                limiterCloned.SetParent(retobj);
                retobj._limiterCollection.Add(limiterCloned);
            }

            retobj._extenderCollection = new List<IWorkShiftExtender>();
            foreach (IWorkShiftExtender extender in _extenderCollection)
            {
                IWorkShiftExtender extenderCloned = extender.EntityClone();
                extenderCloned.SetParent(retobj);
                retobj._extenderCollection.Add(extenderCloned);
            }
            // No need to have the bags.
            retobj._ruleSetBagCollection = new List<IRuleSetBag>();

            return retobj;
        }

        #endregion

        public virtual object Clone()
        {
            return EntityClone();
        }

	}

}
