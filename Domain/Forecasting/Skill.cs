using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Forecasting
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_HideSkillPrioSliders_41312)]
	public interface ISkillPriority
	{
		int Priority { get; set; }
		double PriorityValue { get; }
		Percent OverstaffingFactor { get; set; }
	}

	public class Skill : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, ISkill, ISkillPriority, IDeleteTag
	{
        private string _name = string.Empty;
        private string _description = string.Empty;
        private Color _displayColor;
        private ISkillType _skillType;
	    private ISet<IWorkload> _workloadCollection;
        private IDictionary<int, ISkillDayTemplate> _templateWeekCollection;
        private IActivity _activity;
        private string _timeZone;
        private TimeZoneInfo _cachedTimeZone;
        private int _defaultResolution;
        private StaffingThresholds _staffingThresholds;
        private TimeSpan _midnightBreakOffset;
        
        private IList<ISkill> _aggregateSkills = new List<ISkill>();
        private bool _isVirtual;
        private bool _isDeleted;
        private int _priority = 4;
        private Percent _overstaffingFactor = new Percent(.5);
	    private int _maxParallelTasks;
		private Percent _abandonRate;

		public Skill() : this("_")
        {
        }

		public Skill(string name) : this(name, "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="displayColor">The display color.</param>
        /// <param name="defaultResolution">The default resolution in minutes.</param>
        /// <param name="skillType">Type of the skill.</param>
        public Skill(string name, string description, Color displayColor, int defaultResolution, ISkillType skillType)
        {
            InParameter.StringTooLong(nameof(name), name, 50);
            InParameter.StringTooLong(nameof(description), description, 1024);
            ChangeName(name);
            _description = description;
            _displayColor = displayColor;
            _defaultResolution = defaultResolution;
            _skillType = skillType;
	        _workloadCollection = new HashSet<IWorkload>();
            _staffingThresholds = StaffingThresholds.DefaultValues();

            _templateWeekCollection = new Dictionary<int, ISkillDayTemplate>();
	        _maxParallelTasks = 1;
	        //defaults to 3 on chat
			if (skillType.ForecastSource.Equals(ForecastSource.Chat))
		        _maxParallelTasks = 3;
			if (skillType.ForecastSource.Equals(ForecastSource.Chat)|| skillType.ForecastSource.Equals(ForecastSource.InboundTelephony))
			{
				_abandonRate = new Percent(0.05);
			}

            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                string templateName =
                    string.Format(CultureInfo.CurrentUICulture, "<{0}>",
                                  CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek).ToUpper(
                                      CultureInfo.CurrentUICulture));
                ISkillDayTemplate skillDayTemplate = new SkillDayTemplate(templateName, new List<ITemplateSkillDataPeriod>());
                skillDayTemplate.SetParent(this);
                _templateWeekCollection.Add((int)dayOfWeek, skillDayTemplate);
            }
		}

	    public override void NotifyTransactionComplete(DomainUpdateType operation)
	    {
		    base.NotifyTransactionComplete(operation);
			if (operation == DomainUpdateType.Update)
				AddEvent(new SkillChangedEvent { SkillId = Id.GetValueOrDefault() });
			else if (operation == DomainUpdateType.Insert)
				AddEvent(new SkillCreatedEvent { SkillId = Id.GetValueOrDefault() });
		}
		
	    public virtual ISkillType SkillType
        {
            get { return _skillType; }
            set { _skillType = value; }
        }

        public virtual Color DisplayColor
        {
            get { return _displayColor; }
            set
            {
                if (value.IsEmpty)
                {
                    _displayColor = Color.Red;
                }
                else
                {
                    _displayColor = value;
                }
            }
        }

        public virtual ReadOnlyCollection<ISkill> AggregateSkills
        {
            get { return new ReadOnlyCollection<ISkill>(_aggregateSkills.OrderBy(s => s.Name).ToList()); }
        }

        public virtual void AddAggregateSkill(ISkill skill)
        {
			if(skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
				throw new InvalidOperationException("Max seat skill can not be added to aggregate");
            _aggregateSkills.Add(skill);
        }

        public virtual void RemoveAggregateSkill(ISkill skill)
        {
            _aggregateSkills.Remove(skill);
        }

        public virtual void ClearAggregateSkill()
        {
            _aggregateSkills.Clear();
        }

        public virtual bool IsVirtual
        {
            get { return _isVirtual; }
            set { _isVirtual = value; }
        }

        public virtual string Description
        {
            get { return _description; }
            set
            {
                InParameter.StringTooLong(nameof(Description), value, 1024);
                _description = value;
            }
        }

	    public virtual void ChangeName(string name)
	    {
			InParameter.StringTooLong(nameof(Name), name, 50);
			InParameter.NotStringEmptyOrNull(nameof(Name),name);
			_name = name;
		}

	    public virtual string Name => _name;

		public virtual StaffingThresholds StaffingThresholds
        {
            get { return _staffingThresholds; }
            set { _staffingThresholds = value; }
        }

        public virtual TimeSpan MidnightBreakOffset
        {
            get { return _midnightBreakOffset; }
            set { _midnightBreakOffset = value; }
        }

		protected internal virtual string TimeZoneId { get => _timeZone;
			set => _timeZone = value;
		}

        public virtual TimeZoneInfo TimeZone
        {
            get
            {
                if (String.IsNullOrEmpty(TimeZoneId))
                {
                    _cachedTimeZone = TimeZoneInfo.Utc;
	                TimeZoneId = _cachedTimeZone.Id;
                }
                if (_cachedTimeZone == null)
                {
                    _cachedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
                }

                return _cachedTimeZone;
            }
            set
            {
                InParameter.NotNull(nameof(TimeZone), value);
                _cachedTimeZone = value;
                _timeZone = _cachedTimeZone.Id;
            }
        }

		[RemoveMeWithToggle(Toggles.ResourcePlanner_HideSkillPrioSliders_41312)]
        public virtual int Priority
        {
            get { return _priority; }
            set
            {
                if (value < 1 || value > 7)
                    throw new ArgumentOutOfRangeException(nameof(Priority), value, "Priority must be between 1 and 7.");
                _priority = value;
            }
        }

		[RemoveMeWithToggle(Toggles.ResourcePlanner_HideSkillPrioSliders_41312)]
		public virtual double PriorityValue
        {
            get
            {
                switch (_priority)
                {
                    case 1:
                        return 0.16;
                    case 2:
                        return 0.32;
                    case 3:
                        return 0.64;

                    case 5:
                        return 4;
                    case 6:
                        return 16;
                    case 7:
                        return 256;
                }
                return 1;
            }
        }

		[RemoveMeWithToggle(Toggles.ResourcePlanner_HideSkillPrioSliders_41312)]
		public virtual Percent OverstaffingFactor
        {
            get { return _overstaffingFactor; }
            set
            {
                if (value.Value > 1)
                    throw new ArgumentOutOfRangeException(nameof(OverstaffingFactor), value, "OverstaffingFactor can not be higher than 100%.");
                if (value.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(OverstaffingFactor), value, "OverstaffingFactor can not be less than 0%.");
                _overstaffingFactor = value;
            }
        }

        public virtual void AddWorkload(IWorkload workload)
        {
            InParameter.NotNull(nameof(workload), workload);
            _workloadCollection.Add(workload);
            workload.Skill = this;
        }

        public virtual void RemoveWorkload(IWorkload workload)
        {
            InParameter.NotNull(nameof(workload), workload);
            if (_workloadCollection.Contains(workload))
            {
                _workloadCollection.Remove(workload);
            }
            else
            {
                throw new MissingMemberException("Workload " + workload.Name + " does not exist in this skill.");
            }
        }

        /// <summary>
        /// Gets the forecast.
        /// Read only wrapper around the workload list.
        /// </summary>
        /// <value>The workload.</value>
        public virtual IEnumerable<IWorkload> WorkloadCollection => _workloadCollection;

		public virtual IActivity Activity
        {
            get { return _activity; }
            set { _activity = value; }
        }

        public virtual int DefaultResolution
        {
            get { return _defaultResolution; }
            set { _defaultResolution = value; }
        }

        /// <summary>
        /// Sets the template at.
        /// First 7 slots are the standard WeekDays
        /// </summary>
        /// <param name="templateIndex">Index of the template.</param>
        /// <param name="newTemplate">The new template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        public virtual void SetTemplateAt(int templateIndex, ISkillDayTemplate newTemplate)
        {
            ((AggregateEntity)newTemplate).SetParent(this);
            _templateWeekCollection.Remove(templateIndex);
            _templateWeekCollection.Add(templateIndex, newTemplate);
        }

        /// <summary>
        /// Gets the template at.
        /// First 7 slots are the standard WeekDays
        /// </summary>
        /// <param name="templateIndex">Index of the template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        public virtual ISkillDayTemplate GetTemplateAt(int templateIndex)
        {
            return _templateWeekCollection[templateIndex];
        }

        /// <summary>
        /// Adds a new template to the list.
        /// First 7 slots are the standard WeekDays
        /// </summary>
        /// <param name="newTemplate">The new template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        public virtual int AddTemplate(ISkillDayTemplate newTemplate)
        {
            newTemplate.SetParent(this);
            int nextFreeKey = _templateWeekCollection.Max(k => k.Key) + 1;
            _templateWeekCollection.Add(nextFreeKey, newTemplate);
            return nextFreeKey;
        }

        /// <summary>
        /// Gets all templates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        public virtual IDictionary<int, ISkillDayTemplate> TemplateWeekCollection => new ReadOnlyDictionary<int, ISkillDayTemplate>(_templateWeekCollection);

		/// <summary>
        /// Removes the template.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-21
        /// </remarks>
        public virtual void RemoveTemplate(string templateName)
        {
            ISkillDayTemplate template = TryFindTemplateByName(templateName);
            if (template != null)
            {
	            int key = _templateWeekCollection.First(i => i.Value == template).Key;
                _templateWeekCollection.Remove(key);
            }
        }

        /// <summary>
        /// Tries the name of the find template by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual ISkillDayTemplate TryFindTemplateByName(string name)
        {
            //Returns null if it doesnt exists
            name = name.ToLower(CultureInfo.CurrentCulture);
            KeyValuePair<int, ISkillDayTemplate> pair = _templateWeekCollection.FirstOrDefault(n => n.Value.Name.ToLower(CultureInfo.CurrentCulture) == name);
            return pair.Value;
        }

	    public virtual int MaxParallelTasks
	    {
		    get { return _maxParallelTasks; }
		    set
		    {
				if (value < 1 || value > 100)
					throw new ArgumentOutOfRangeException(nameof(MaxParallelTasks), value, "Priority must be between 1 and 100.");
				_maxParallelTasks = value;
		    }
	    }

		public virtual Percent AbandonRate
		{
			get { return _abandonRate; }
			set
			{
				if (value.Value < 0 || value.Value > 1)
					throw new ArgumentOutOfRangeException(nameof(AbandonRate), value, "Abandon rate must be between 0 and 100%.");
				_abandonRate = value;
			}
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-04-15
		/// </remarks>
		public override string ToString()
        {
            return String.Concat(Name, ", ", base.ToString());
        }

        #region IRestrictionChecker<Skill> Members

        /// <summary>
        /// Checks the restrictions.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public virtual void CheckRestrictions()
        {
            RestrictionSet.CheckEntity(this);
        }

        /// <summary>
        /// Gets the restriction set.
        /// </summary>
        /// <value>The restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        public virtual IRestrictionSet<ISkill> RestrictionSet
        {
            get { return SkillRestrictionSet.CurrentRestrictionSet; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

		public virtual int? CascadingIndex { get; protected set; }
		public virtual bool IsChildSkill { get; } = false;

		public virtual bool IsCascading()
		{
			return CascadingIndex.HasValue && CanBeCascading();
		}

		public virtual bool CanBeCascading()
		{
			return SkillType == null || SkillType.ForecastSource != ForecastSource.MaxSeatSkill;
		}

		public virtual void SetCascadingIndex(int index)
		{
			CascadingIndex = index;
		}

		public virtual void ClearCascadingIndex()
		{
			CascadingIndex = null;
		}

		#endregion

		#region IForecastTemplateOwner Members

		/// <summary>
		/// Sets a template on a specific key
		/// First 7 slots are the standard WeekDays
		/// </summary>
		/// <param name="templateIndex">Index of the template.</param>
		/// <param name="dayTemplate">The day template.</param>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2008-01-24
		/// </remarks>
		public virtual void SetTemplateAt(int templateIndex, IForecastDayTemplate dayTemplate)
        {
            SkillDayTemplate newTemplate = dayTemplate as SkillDayTemplate;
            if (newTemplate == null) return;

            newTemplate.SetParent(this);
            _templateWeekCollection.Remove(templateIndex);
            _templateWeekCollection.Add(templateIndex, newTemplate);
        }

        /// <summary>
        /// Sets the template.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <param name="dayTemplate">The day template.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-26
        /// </remarks>
        public virtual void SetTemplate(DayOfWeek dayOfWeek, IForecastDayTemplate dayTemplate)
        {
            SetTemplateAt((int)dayOfWeek, dayTemplate);
        }

        public virtual void SetTemplatesByName(TemplateTarget templateTarget, string name, IList<ITemplateDay> days)
        {
            IForecastDayTemplate template = TryFindTemplateByName(templateTarget, name);

            switch (templateTarget)
            {
                case TemplateTarget.Skill:
                    foreach (var skillDay in days.OfType<ISkillDay>())
                    {
                        skillDay.ApplyTemplate((ISkillDayTemplate)template);
                    }
                    break;
                case TemplateTarget.Multisite:
                    foreach (var skillDay in days.OfType<IMultisiteDay>())
                    {
                        skillDay.ApplyTemplate((IMultisiteDayTemplate)template);
                    }
                    break;
            }
        }

        /// <summary>
        /// First 7 slots returns are dedicated to weekdays
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="templateIndex">Index of the template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-24
        /// </remarks>
        public virtual IForecastDayTemplate GetTemplateAt(TemplateTarget target, int templateIndex)
        {
            return _templateWeekCollection[templateIndex];
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-26
        /// </remarks>
        public virtual IForecastDayTemplate GetTemplate(TemplateTarget target, DayOfWeek dayOfWeek)
        {
            return GetTemplateAt(target, (int)dayOfWeek);
        }

        /// <summary>
        /// Adds a new template to the list.
        /// First 7 slots are the standard WeekDays
        /// </summary>
        /// <param name="dayTemplate">The day template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        public virtual int AddTemplate(IForecastDayTemplate dayTemplate)
        {
			if (dayTemplate is ISkillDayTemplate newTemplate)
                return AddTemplate(newTemplate);

            return -1;
        }

        /// <summary>
        /// Gets the name of the find template by.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <value>The name of the find template by.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-03
        /// </remarks>
        public virtual IForecastDayTemplate TryFindTemplateByName(TemplateTarget target, string name)
        {
            //Returns null if it doesnt exists
            name = name.ToLower(CultureInfo.CurrentCulture);
            KeyValuePair<int, ISkillDayTemplate> pair = _templateWeekCollection.FirstOrDefault(n => n.Value.Name.ToLower(CultureInfo.CurrentCulture) == name);

            return pair.Value;
        }

        /// <summary>
        /// Removes the template.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="templateName">Name of the template.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-21
        /// </remarks>
        public virtual void RemoveTemplate(TemplateTarget target, string templateName)
        {
            SkillDayTemplate template = (SkillDayTemplate)TryFindTemplateByName(target, templateName);
            if (template != null)
            {
	            int key = _templateWeekCollection.First(i => template.Equals(i.Value)).Key;
                _templateWeekCollection.Remove(key);
            }
        }

        /// <summary>
        /// Sets the default templates.
        /// </summary>
        /// <param name="theDays">The days.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-02
        /// </remarks>
        public virtual void SetDefaultTemplates(IEnumerable theDays)
        {
            foreach (var skillDay in theDays.OfType<SkillDay>())
            {
                skillDay.ApplyTemplate((SkillDayTemplate)GetTemplateAt(TemplateTarget.Skill, (int)skillDay.CurrentDate.DayOfWeek));
            }
        }

        public virtual void SetLongtermTemplate(IEnumerable theDays)
        {
        }

        /// <summary>
        /// Gets the templates.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-02
        /// </remarks>
        public virtual IDictionary<int, IForecastDayTemplate> GetTemplates(TemplateTarget target)
        {
            return
                new ReadOnlyDictionary<int, IForecastDayTemplate>(_templateWeekCollection.ToDictionary(k => k.Key,
                                                                                                       i => (IForecastDayTemplate)i.Value));
        }

        public virtual void RefreshTemplates(IForecastTemplateOwner forecastTemplateOwner)
        {
            if (!Equals(forecastTemplateOwner)) throw new ArgumentException("The supplied template owner must be of the same skill", "forecastTemplateOwner");

            _templateWeekCollection = new Dictionary<int, ISkillDayTemplate>(((Skill)forecastTemplateOwner)._templateWeekCollection);
        }

        #endregion

        #region Implementation of ICloneable

        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        #endregion

        #region Implementation of ICloneableEntity<ISkill>

        public virtual ISkill NoneEntityClone()
        {
            Skill retobj = (Skill)MemberwiseClone();
	        CloneEvents(retobj);
			retobj.SetId(null);
            retobj._templateWeekCollection = new Dictionary<int, ISkillDayTemplate>();
            foreach (KeyValuePair<int, ISkillDayTemplate> keyValuePair in _templateWeekCollection)
            {
                ISkillDayTemplate template = keyValuePair.Value.NoneEntityClone();
                template.SetParent(retobj);
                retobj._templateWeekCollection.Add(keyValuePair.Key, template);
            }
	        retobj._workloadCollection = new HashSet<IWorkload>();
            foreach (IWorkload workload in _workloadCollection)
            {
                retobj.AddWorkload(workload);
            }

            return retobj;
        }

        public virtual ISkill EntityClone()
        {
            Skill retobj = (Skill)MemberwiseClone();
	        CloneEvents(retobj);
			retobj._templateWeekCollection = new Dictionary<int, ISkillDayTemplate>();
            foreach (KeyValuePair<int, ISkillDayTemplate> keyValuePair in _templateWeekCollection)
            {
                ISkillDayTemplate template = keyValuePair.Value.EntityClone();
                template.SetParent(retobj);
                retobj._templateWeekCollection.Add(keyValuePair.Key, template);
            }
            retobj._workloadCollection = new HashSet<IWorkload>();
            foreach (IWorkload workload in _workloadCollection)
            {
                retobj.AddWorkload(workload);
            }

            return retobj;
        }

        #endregion

        public virtual void SetDeleted()
        {
            _isDeleted = true;
			AddEvent(() => new SkillDeletedEvent { SkillId = Id.GetValueOrDefault() });
		}
    }
}
