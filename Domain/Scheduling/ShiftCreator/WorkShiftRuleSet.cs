using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
		private ISet<DateOnly> _accessibilityDates;

		private Description _description;
		private bool _onlyForRestrictions;

		public WorkShiftRuleSet(IWorkShiftTemplateGenerator generator)
        {
            InParameter.NotNull(nameof(generator), generator);
            _extenderCollection = new List<IWorkShiftExtender>();
            _limiterCollection = new List<IWorkShiftLimiter>();
            _ruleSetBagCollection = new List<IRuleSetBag>();
            _templateGenerator = generator;
			_accessibilityDaysOfWeek = new HashSet<DayOfWeek>();
			_accessibilityDates = new HashSet<DateOnly>();

        }

		protected WorkShiftRuleSet() { }

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

		public virtual ReadOnlyCollection<IWorkShiftExtender> ExtenderCollection => new ReadOnlyCollection<IWorkShiftExtender>(_extenderCollection);

	    public virtual ReadOnlyCollection<IWorkShiftLimiter> LimiterCollection => new ReadOnlyCollection<IWorkShiftLimiter>(_limiterCollection);

	    public virtual ReadOnlyCollection<IRuleSetBag> RuleSetBagCollection => new ReadOnlyCollection<IRuleSetBag>(_ruleSetBagCollection);

	    public virtual void RemoveRuleSetBag(IRuleSetBag ruleSetBag)
        {
            InParameter.NotNull(nameof(ruleSetBag), ruleSetBag);
            var concrete = ruleSetBag as RuleSetBag;
	        concrete?.RuleSetCollectionWritable.Remove(this);
	        _ruleSetBagCollection.Remove(ruleSetBag);
        }
		
        public virtual ICollection<IRuleSetBag> RuleSetBagCollectionWritable => _ruleSetBagCollection;

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

        public virtual IEnumerable<DayOfWeek> AccessibilityDaysOfWeek => _accessibilityDaysOfWeek;

	    public virtual IEnumerable<DateOnly> AccessibilityDates => _accessibilityDates;

	    public virtual void AddExtender(IWorkShiftExtender extender)
        {
            InParameter.NotNull(nameof(extender), extender);
            if(extender.Parent != null)
                throw new ArgumentException("extender.Parent nust be null");
            extender.SetParent(this);
            _extenderCollection.Add(extender);
        }

        public virtual void AddLimiter(IWorkShiftLimiter limiter)
        {
            InParameter.NotNull(nameof(limiter), limiter);
            if (limiter.Parent != null)
                throw new ArgumentException("limiter.Parent nust be null");
            limiter.SetParent(this);
            _limiterCollection.Add(limiter);
        }

		public virtual void InsertExtender(int index, IWorkShiftExtender extender)
		{
			InParameter.NotNull(nameof(extender), extender);
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

        public virtual void AddAccessibilityDate(DateOnly date)
        {
	        _accessibilityDates.Add(date);
        }

		public virtual void RemoveAccessibilityDate(DateOnly date)
        {
            _accessibilityDates.Remove(date);
        }

        public virtual bool IsValidDate(DateOnly dateToCheck)
        {
            var dateInList = _accessibilityDates.Contains(dateToCheck);
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
	        _accessibilityDates = new HashSet<DateOnly>();
        }

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

			retobj._limiterCollection = _limiterCollection.Select(limiter =>
			{
				IWorkShiftLimiter limiterCloned = limiter.NoneEntityClone();
				limiterCloned.SetParent(retobj);
				return limiterCloned;
			}).ToList();

			retobj._extenderCollection = _extenderCollection.Select(extender =>
			{
				IWorkShiftExtender extenderCloned = extender.NoneEntityClone();
				extenderCloned.SetParent(retobj);
				return extenderCloned;
			}).ToList();

			foreach (DayOfWeek dayOfWeek in _accessibilityDaysOfWeek)
				retobj.AddAccessibilityDayOfWeek(dayOfWeek);

			foreach (var date in _accessibilityDates)
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

			retobj._limiterCollection = _limiterCollection.Select(limiter =>
			{
				IWorkShiftLimiter limiterCloned = limiter.EntityClone();
				limiterCloned.SetParent(retobj);
				return limiterCloned;
			}).ToList();

			retobj._extenderCollection = _extenderCollection.Select(extender =>
			{
				IWorkShiftExtender extenderCloned = extender.EntityClone();
				extenderCloned.SetParent(retobj);
				return extenderCloned;
			}).ToList();

			foreach (DayOfWeek dayOfWeek in _accessibilityDaysOfWeek)
				retobj.AddAccessibilityDayOfWeek(dayOfWeek);

			foreach (var date in _accessibilityDates)
				retobj.AddAccessibilityDate(date);

			// No need to have the bags.
			retobj._ruleSetBagCollection = new List<IRuleSetBag>();

            return retobj;
        }

	    public virtual object Clone()
        {
            return EntityClone();
        }
	}
}
