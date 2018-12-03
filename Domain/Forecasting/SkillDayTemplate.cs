using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{


    /// <summary>
    /// Used to represent skill data
    /// </summary>
    /// <remarks>
    /// Created by: micke
    /// Created date: 18.12.2007
    /// </remarks>
    public class SkillDayTemplate : AggregateEntity, ISkillDayTemplate
    {
	    private ISet<ITemplateSkillDataPeriod> _templateSkillDataPeriodCollection = new HashSet<ITemplateSkillDataPeriod>();
        private string _name;
        private int _versionNumber;
    	private DateTime _updatedDate;
    	private bool _templateVersionNumberIncreased;

    	/// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-12
        /// </remarks>
        public virtual string Name
        {
            get
            {
                return TemplateReference.DisplayName(DayOfWeek, _name, false);
            }
            set {
                InParameter.NotNull(nameof(Name), value);
                _name = value; }
        }
        /// <summary>
        /// Gets or sets the template reference.
        /// </summary>
        /// <value>The template reference.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-22
        /// </remarks>
        public virtual ITemplateReference TemplateReference
        {
            get
            {return new TemplateReference(Guid.Empty, 0, string.Empty, null);}
            protected set
            { }
        }

	    /// <summary>
	    /// Gets the base date.
	    /// </summary>
	    /// <value>The base date.</value>
	    /// <remarks>
	    /// Created by: robink
	    /// Created date: 2008-02-18
	    /// </remarks>
	    public static DateOnly BaseDate { get; } = new DateOnly(1800, 1, 1);

	    /// <summary>
        /// Initializes a new instance of the <see cref="SkillDayTemplate"/> class for NHibernate.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        protected SkillDayTemplate(){ }

	    /// <summary>
        /// Initializes a new instance of the <see cref="SkillDayTemplate"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="skillDataPeriodCollection">The skill data period collection.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public SkillDayTemplate(string name, IEnumerable<ITemplateSkillDataPeriod> skillDataPeriodCollection)
            :this()
        {
            InParameter.NotStringEmptyOrNull(nameof(name), name);
            _name = name;
        	_updatedDate = DateTime.UtcNow;
            VerifyAndAttachSkillDataPeriods(skillDataPeriodCollection);
        }

        /// <summary>
        /// Gets the skill data period template collection.
        /// </summary>
        /// <value>The skill data period template collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        public virtual ReadOnlyCollection<ITemplateSkillDataPeriod> TemplateSkillDataPeriodCollection
        {
            get { return new ReadOnlyCollection<ITemplateSkillDataPeriod>(_templateSkillDataPeriodCollection.ToArray()); }
        }

        private void VerifyAndAttachSkillDataPeriods(IEnumerable<ITemplateSkillDataPeriod> templateSkillDataPeriodCollection)
        {
            if (templateSkillDataPeriodCollection == null) return;
            foreach (ITemplateSkillDataPeriod skillDataPeriod in templateSkillDataPeriodCollection)
            {
                if (skillDataPeriod.Period.StartDateTime > BaseDate.AddDays(3).Date)
                    throw new ArgumentOutOfRangeException(nameof(templateSkillDataPeriodCollection),"The date is out of order.");
                skillDataPeriod.SetParent(this);
                _templateSkillDataPeriodCollection.Add(skillDataPeriod);
            }
            VerifyPeriodConsistency();
        }

        private void VerifyPeriodConsistency()
        {
            ITemplateSkillDataPeriod previousSkillDataPeriod = null;
            foreach (ITemplateSkillDataPeriod skillDataPeriod in _templateSkillDataPeriodCollection.OrderBy(p => p.Period.StartDateTime))
            {
                if (previousSkillDataPeriod != null)
                {
                    if (previousSkillDataPeriod.Period.EndDateTime > skillDataPeriod.Period.StartDateTime)
                    {
                        throw new InvalidOperationException("The periods cannot be overlapping.");
                    }
                }
                previousSkillDataPeriod = skillDataPeriod;
            }
        }

        /// <summary>
        /// Sets the skill data period collection.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void SetSkillDataPeriodCollection(IList<ITemplateSkillDataPeriod> periods)
        {
            if(periods!=null)
            {
                _templateSkillDataPeriodCollection.Clear();
                VerifyAndAttachSkillDataPeriods(periods);
                IncreaseVersionNumber();
            }
        }

        /// <summary>
        /// Gets the skill resolution.
        /// </summary>
        /// <value>The skill resolution.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-10
        /// </remarks>
        public virtual int SkillResolution
        {
            get
            {
				if (Parent is ISkill skill)
                    return skill.DefaultResolution;
                return 15;
            }
        }

        /// <summary>
        /// Gets the day of week.
        /// </summary>
        /// <value>The day of week.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-12
        /// </remarks>
        public virtual DayOfWeek? DayOfWeek
        {
            get
            {
                int wDayIndex = WeekdayIndex;
                if (!Enum.IsDefined(typeof(DayOfWeek), wDayIndex))
                    return null;
                return (DayOfWeek) wDayIndex;
            }
        }

        public virtual int VersionNumber
        {
            get { return _versionNumber; }
            // setter is for nhibernate
            protected set { _versionNumber = value;  }
        }

		///<summary>
		/// Gets the updated date.
		///</summary>
		/// <value>The updated date.</value>
		public virtual DateTime UpdatedDate
		{
			get { return _updatedDate = _updatedDate == new DateTime() ? BaseDate.Date : _updatedDate; }
			protected set { _updatedDate = value; }
		}

    	/// <summary>
        /// Gets or sets the index of the weekday.
        /// Just here for bi-directional mapping purposes (will be saved in column)
        /// </summary>
        /// <value>The index of the weekday.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-30
        /// </remarks>
        protected virtual int WeekdayIndex
        {
            get
            {
                ISkill skill = Parent as ISkill;
                if (skill == null || skill.TemplateWeekCollection.Count == 0) return -1;

                var item = skill.TemplateWeekCollection.SingleOrDefault(t => t.Value.Equals(this));
                if (item.Value == null) return -1;

                return item.Key;
            }
            set
            {
                //do nada
            }
        }

        /// <summary>
        /// Splits the template skill data periods.
        /// Only template skill data periods owned by this skill day template will be splitted!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void SplitTemplateSkillDataPeriods(IList<ITemplateSkillDataPeriod> list)
        {
            if (!list.All(i => Equals(i.Parent)))
            {
                throw new ArgumentException("All items in supplied list must have this template as parent.", nameof(list));
            }
            TimeSpan resolutionAsTimeSpan = TimeSpan.FromMinutes(SkillResolution);
            foreach (ITemplateSkillDataPeriod skillDataPeriod in list)
            {
                if (_templateSkillDataPeriodCollection.Contains(skillDataPeriod))
                {
                    _templateSkillDataPeriodCollection.Remove(skillDataPeriod);

                    for (DateTime t = skillDataPeriod.Period.StartDateTime; t < skillDataPeriod.Period.EndDateTime; t = t.Add(resolutionAsTimeSpan))
                    {
                        ITemplateSkillDataPeriod newSkillDataPeriod = new TemplateSkillDataPeriod(
                                                                             new ServiceAgreement(
                                                                                 new ServiceLevel(
                                                                                     skillDataPeriod.ServiceLevelPercent,
                                                                                     skillDataPeriod.ServiceLevelSeconds),
                                                                                     skillDataPeriod.MinOccupancy,
                                                                                     skillDataPeriod.MaxOccupancy),
                                                                             new SkillPersonData(
                                                                                 skillDataPeriod.MinimumPersons,
                                                                                 skillDataPeriod.MaximumPersons),
                                                                             new DateTimePeriod(t, t.Add(resolutionAsTimeSpan)));

                        newSkillDataPeriod.Shrinkage = skillDataPeriod.Shrinkage;
                        newSkillDataPeriod.ManualAgents = skillDataPeriod.ManualAgents;
                        newSkillDataPeriod.Efficiency = skillDataPeriod.Efficiency;
                        newSkillDataPeriod.SetParent(this);
                        _templateSkillDataPeriodCollection.Add(newSkillDataPeriod);
                    }
                }
            }
            VerifyPeriodConsistency();
            IncreaseVersionNumber();
        }

        /// <summary>
        /// Merges the template skill data periods.
        /// Only skill data periods owned by this skill day template will be merged!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void MergeTemplateSkillDataPeriods(IList<ITemplateSkillDataPeriod> list)
        {
            if (!list.All(i => Equals(i.Parent)))
            {
                throw new ArgumentException("All items in supplied list must have this template as parent.", nameof(list));
            }
            ITemplateSkillDataPeriod newSkillDataPeriod = TemplateSkillDataPeriod.Merge(list, this);
            list.ForEach(i => _templateSkillDataPeriodCollection.Remove(i));
            newSkillDataPeriod.SetParent(this);
            _templateSkillDataPeriodCollection.Add(newSkillDataPeriod);
            VerifyPeriodConsistency();
            IncreaseVersionNumber();
        }

		public virtual void IncreaseVersionNumber()
		{
			_versionNumber++;
			_templateVersionNumberIncreased = true;
		}

		public virtual void RefreshUpdatedDate()
		{
			if (_templateVersionNumberIncreased)
				_updatedDate = DateTime.UtcNow;
		}

	    public virtual object Clone()
        {
            return NoneEntityClone();
        }

	    public virtual ISkillDayTemplate NoneEntityClone()
        {
            SkillDayTemplate retobj = (SkillDayTemplate)MemberwiseClone();
            retobj.SetId(null);
            retobj._templateSkillDataPeriodCollection = new HashSet<ITemplateSkillDataPeriod>();
            foreach (ITemplateSkillDataPeriod templateSkillDataPeriod in _templateSkillDataPeriodCollection)
            {
                ITemplateSkillDataPeriod clonedPeriod = templateSkillDataPeriod.NoneEntityClone();
                clonedPeriod.SetParent(retobj);
                retobj._templateSkillDataPeriodCollection.Add(clonedPeriod);
            }
            return retobj;
        }

        public virtual ISkillDayTemplate EntityClone()
        {
            SkillDayTemplate retobj = (SkillDayTemplate)MemberwiseClone();
            retobj._templateSkillDataPeriodCollection = new HashSet<ITemplateSkillDataPeriod>();
            foreach (ITemplateSkillDataPeriod templateSkillDataPeriod in _templateSkillDataPeriodCollection)
            {
                ITemplateSkillDataPeriod clonedPeriod = templateSkillDataPeriod.EntityClone();
                clonedPeriod.SetParent(retobj);
                retobj._templateSkillDataPeriodCollection.Add(clonedPeriod);
            }
            return retobj;
        }
    }
}
