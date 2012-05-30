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
    public class WorkShiftRuleSet : AggregateRootWithBusinessUnit, IWorkShiftRuleSet
    {
        private Description _description;
        private IList<IWorkShiftExtender> _extenderCollection;
        private IList<IWorkShiftLimiter> _limiterCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private IWorkShiftTemplateGenerator _templateGenerator;
        private DefaultAccessibility _defaultAccessibility = DefaultAccessibility.Included;
        private IList<DayOfWeek> _accessibilityDaysOfWeek = new List<DayOfWeek>(7);
        private IList<DateTime> _accessibilityDates = new List<DateTime>();
        private IDictionary<IEffectiveRestriction, IWorkTimeMinMax> _restrictionCashe;
    	private bool _onlyForRestrictions;

    	public WorkShiftRuleSet(IWorkShiftTemplateGenerator generator)
        {
            InParameter.NotNull("generator", generator);
            _extenderCollection = new List<IWorkShiftExtender>();
            _limiterCollection = new List<IWorkShiftLimiter>();
            _ruleSetBagCollection = new List<IRuleSetBag>();
            _templateGenerator = generator;
            
        }

        protected WorkShiftRuleSet()
        {
        }



		#region Properties (6) 

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
            RuleSetBag concrete = ruleSetBag as RuleSetBag;
            if (concrete != null)
                concrete.RuleSetCollectionWritable.Remove(this);
            _ruleSetBagCollection.Remove(ruleSetBag);
        }

        /// <summary>
        /// Gets the internal modifiable rule set bag collection.
        /// </summary>
        /// <value>The rule set bag collection internal.</value>
        public virtual ICollection<IRuleSetBag> RuleSetBagCollectionWritable
        {
            get
            {
                return _ruleSetBagCollection;   
            }
        }

        /// <summary>
        /// Gets the template generator.
        /// </summary>
        /// <value>The template generator.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        public virtual IWorkShiftTemplateGenerator TemplateGenerator
        {
            get { return _templateGenerator; }
            set { _templateGenerator = value; }
        }

        public virtual IWorkTimeMinMax MinMaxWorkTime(IRuleSetProjectionService ruleSetProjectionService, 
                                                        IEffectiveRestriction effectiveRestriction)
        {
            lock (this)
            {
                if (_restrictionCashe == null)
                    _restrictionCashe = new Dictionary<IEffectiveRestriction, IWorkTimeMinMax>();
                IWorkTimeMinMax cashedValue;

                if (_restrictionCashe.TryGetValue(effectiveRestriction, out cashedValue))
                {
                    return cashedValue;
                }

                cashedValue = calculateWorkTimeMinMax(ruleSetProjectionService, effectiveRestriction);
                _restrictionCashe.Add(effectiveRestriction, cashedValue);
                return cashedValue;
            }
        }

        private IWorkTimeMinMax calculateWorkTimeMinMax(IRuleSetProjectionService ruleSetProjectionService, 
                                                        IEffectiveRestriction effectiveRestriction)
        {
            IWorkTimeMinMax resultWorkTimeMinMax = null;
			if (effectiveRestriction.NotAvailable)
				return null;

            IEnumerable<IWorkShiftVisualLayerInfo> infoList = ruleSetProjectionService.ProjectionCollection(this);
            foreach (var visualLayerInfo in infoList)
            {
                if (effectiveRestriction.ValidateWorkShiftInfo(visualLayerInfo))
                {
                    IWorkTimeMinMax thisWorkTimeMinMax = new WorkTimeMinMax();
                    TimePeriod? period = visualLayerInfo.WorkShift.ToTimePeriod();
                    if (!period.HasValue)
                        continue;
                    thisWorkTimeMinMax.StartTimeLimitation = new StartTimeLimitation(period.Value.StartTime, period.Value.StartTime);
                    thisWorkTimeMinMax.EndTimeLimitation = new EndTimeLimitation(period.Value.EndTime, period.Value.EndTime);
                    thisWorkTimeMinMax.WorkTimeLimitation = new WorkTimeLimitation(visualLayerInfo.VisualLayerCollection.ContractTime(), visualLayerInfo.VisualLayerCollection.ContractTime());
                    if (resultWorkTimeMinMax == null)
                        resultWorkTimeMinMax = new WorkTimeMinMax();

                    resultWorkTimeMinMax = resultWorkTimeMinMax.Combine(thisWorkTimeMinMax);
                }
            }

            return resultWorkTimeMinMax;
        }
        
        /// <summary>
        /// Gets the default accessibility.
        /// </summary>
        /// <value>The default accessibility.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        public virtual DefaultAccessibility DefaultAccessibility
        {
            get { return _defaultAccessibility; }
            set { _defaultAccessibility = value; }
        }

        /// <summary>
        /// Gets the accessibility days of week.
        /// </summary>
        /// <value>The accessibility days of week.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        public virtual ReadOnlyCollection<DayOfWeek> AccessibilityDaysOfWeek
        {
            get { return new ReadOnlyCollection<DayOfWeek>(_accessibilityDaysOfWeek); }
        }

        /// <summary>
        /// Gets the accessibility dates.
        /// </summary>
        /// <value>The accessibility dates.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-07-01
        /// </remarks>
        public virtual ReadOnlyCollection<DateTime> AccessibilityDates
        {
            get { return new ReadOnlyCollection<DateTime>(_accessibilityDates); }
        }

		#endregion Properties 

		#region Methods (4) 


		// Public Methods (4) 

        /// <summary>
        /// Adds an extender.
        /// </summary>
        /// <param name="extender">The extender.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        public virtual void AddExtender(IWorkShiftExtender extender)
        {
            InParameter.NotNull("extender", extender);
            if(extender.Parent != null)
                throw new ArgumentException("extender.Parent nust be null");
            extender.SetParent(this);
            _extenderCollection.Add(extender);
        }

        /// <summary>
        /// Adds a limiter.
        /// </summary>
        /// <param name="limiter">The limiter.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-20
        /// </remarks>
        public virtual void AddLimiter(IWorkShiftLimiter limiter)
        {
            InParameter.NotNull("limiter", limiter);
            if (limiter.Parent != null)
                throw new ArgumentException("limiter.Parent nust be null");
            limiter.SetParent(this);
            _limiterCollection.Add(limiter);
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

        /// <summary>
        /// Adds the accessibility day of week.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        public virtual void AddAccessibilityDayOfWeek(DayOfWeek dayOfWeek)
        {
            if (!_accessibilityDaysOfWeek.Contains(dayOfWeek))
                _accessibilityDaysOfWeek.Add(dayOfWeek);
        }

        /// <summary>
        /// Removes the accessibility day of week.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        public virtual void RemoveAccessibilityDayOfWeek(DayOfWeek dayOfWeek)
        {
            _accessibilityDaysOfWeek.Remove(dayOfWeek);
        }

        /// <summary>
        /// Adds the accessibility date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-07-01
        /// </remarks>
        public virtual void AddAccessibilityDate(DateTime dateTime)
        {
            InParameter.VerifyDateIsUtc("dateTime",dateTime);
            dateTime = dateTime.Date;
            if (!_accessibilityDates.Contains(dateTime))
                _accessibilityDates.Add(dateTime);
        }

        /// <summary>
        /// Removes the accessibility date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-07-01
        /// </remarks>
        public virtual void RemoveAccessibilityDate(DateTime dateTime)
        {
            _accessibilityDates.Remove(DateTime.SpecifyKind(dateTime.Subtract(dateTime.TimeOfDay), DateTimeKind.Utc));
        }

        /// <summary>
        /// Determines whether [is valid date] [the specified date].
        /// </summary>
        /// <param name="dateToCheck">The date to check.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid date] [the specified date]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-02
        /// </remarks>
        public virtual bool IsValidDate(DateOnly dateToCheck)
        {
            bool dateInList = _accessibilityDates.Contains(dateToCheck.Date);
            bool dayOfWeekInList = _accessibilityDaysOfWeek.Contains(dateToCheck.DayOfWeek);
            bool availableByDefault = (_defaultAccessibility == DefaultAccessibility.Included);

            if (dateInList || dayOfWeekInList)
                availableByDefault = !availableByDefault;

            return availableByDefault;
        }

        /// <summary>
        /// Swap two extenders
        /// </summary>
        /// <param name="left">Left extender</param>
        /// <param name="right">Right extender</param>
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

        /// <summary>
        /// Reinitializes the fields.
        /// </summary>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-19
        /// </remarks>
        private void reinitializeFields()
        {
            _accessibilityDaysOfWeek = new List<DayOfWeek>();
            _accessibilityDates = new List<DateTime>();
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
            WorkShiftRuleSet retobj = (WorkShiftRuleSet)MemberwiseClone();
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

            //Do not copy rulesetbag on entity clone NH Bug8217
            //foreach (IRuleSetBag ruleSetBag in RuleSetBagCollection)
            //{
            //    ruleSetBag.AddRuleSet(retobj);
            //}

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
            WorkShiftRuleSet retobj = (WorkShiftRuleSet)MemberwiseClone();

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

        #region ICloneable Members (1) 

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-16
        /// </remarks>
        public virtual object Clone()
        {
            return EntityClone();
        }

        #endregion

	}

}
