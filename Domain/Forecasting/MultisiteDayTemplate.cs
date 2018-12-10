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
    public class MultisiteDayTemplate : AggregateEntity, IMultisiteDayTemplate
    {
	    private ISet<ITemplateMultisitePeriod> _templateMultisitePeriodCollection = new HashSet<ITemplateMultisitePeriod>();
        private string _name;
        private int _versionNumber;
    	private DateTime _updatedDate;
    	private bool _templateVersionNumberIncreased;

	    /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public virtual string Name
        {
            get { return TemplateReference.DisplayName(DayOfWeek, _name, false); }
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
            { return new TemplateReference(Guid.Empty, 0, string.Empty, null); }
            protected set
            { }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteDayTemplate"/> class for NHibernate.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-08
        /// </remarks>
        protected MultisiteDayTemplate() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultisiteDayTemplate"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="multisitePeriodCollection">The multisite period collection.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        public MultisiteDayTemplate(string name, IList<ITemplateMultisitePeriod> multisitePeriodCollection)
            :this()
        {
            InParameter.NotStringEmptyOrNull(nameof(name), name);
            _name = name;
			_updatedDate = DateTime.UtcNow;
            VerifyAndAttachMultisitePeriods(multisitePeriodCollection);
        }

        /// <summary>
        /// Gets the skill data period template collection.
        /// </summary>
        /// <value>The skill data period template collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        public virtual ReadOnlyCollection<ITemplateMultisitePeriod> TemplateMultisitePeriodCollection
        {
            get { return new ReadOnlyCollection<ITemplateMultisitePeriod>(_templateMultisitePeriodCollection.ToArray()); }
        }

        private void VerifyAndAttachMultisitePeriods(IList<ITemplateMultisitePeriod> templateMultisitePeriodCollection)
        {
            if (templateMultisitePeriodCollection == null) return;
            foreach (ITemplateMultisitePeriod multisitePeriod in templateMultisitePeriodCollection)
            {
                if (multisitePeriod.Period.StartDateTime > SkillDayTemplate.BaseDate.AddDays(3).Date)
                    throw new ArgumentOutOfRangeException(nameof(templateMultisitePeriodCollection),"The date is out of order.");
                if (_templateMultisitePeriodCollection.Any(t => t.Period.StartDateTime == multisitePeriod.Period.StartDateTime))
                    throw new InvalidOperationException("The multisite periods must have unique start times");
                multisitePeriod.SetParent(this);
                _templateMultisitePeriodCollection.Add(multisitePeriod);
            }
        }

        /// <summary>
        /// Sets the multisite period collection.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void SetMultisitePeriodCollection(IList<ITemplateMultisitePeriod> periods)
        {
            if(periods!=null)
            {
                _templateMultisitePeriodCollection.Clear();
                VerifyAndAttachMultisitePeriods(periods);
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
                ISkill skill = Parent as ISkill;
                if (skill == null) return 15;
                    
                return skill.DefaultResolution;
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
                return (DayOfWeek)wDayIndex;
            }
        }

        public virtual int VersionNumber
        {
            get { return _versionNumber; }
            // setter is for nhibernate
            protected set { _versionNumber = value; }
        }

		///<summary>
		/// Gets the updated date.
		///</summary>
		/// <value>The updated date.</value>
		public virtual DateTime UpdatedDate
		{
			get { return _updatedDate = _updatedDate == new DateTime() ? SkillDayTemplate.BaseDate.Date : _updatedDate; }
			protected set { _updatedDate = value; }
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
                IMultisiteSkill skill = Parent as IMultisiteSkill;
                if (skill == null || skill.TemplateMultisiteWeekCollection.Count == 0) return -1;

                var item = skill.TemplateMultisiteWeekCollection.SingleOrDefault(t => t.Value.Equals(this));
                if (item.Value == null) return -1;

                return item.Key;
            }
            set
            {
                //do nada
            }
        }

        /// <summary>
        /// Splits the template multisite periods.
        /// Only template multisite periods owned by this multisite day template will be splitted!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void SplitTemplateMultisitePeriods(IList<ITemplateMultisitePeriod> list)
        {
            foreach (ITemplateMultisitePeriod multisitePeriod in list)
            {
                if (_templateMultisitePeriodCollection.Contains(multisitePeriod))
                {
                    _templateMultisitePeriodCollection.Remove(multisitePeriod);
                    TimeSpan resolutionAsTimeSpan = TimeSpan.FromMinutes(SkillResolution);
                    for (DateTime t = multisitePeriod.Period.StartDateTime; t < multisitePeriod.Period.EndDateTime; t = t.Add(resolutionAsTimeSpan))
                    {
                        ITemplateMultisitePeriod newMultisitePeriod = new TemplateMultisitePeriod(
                            new DateTimePeriod(t, t.Add(resolutionAsTimeSpan)),
                            new Dictionary<IChildSkill, Percent>(multisitePeriod.Distribution));
                        newMultisitePeriod.SetParent(this);
                        _templateMultisitePeriodCollection.Add(newMultisitePeriod);
                    }
                    IncreaseVersionNumber();
                }
            }
        }

        /// <summary>
        /// Merges the template multisite periods.
        /// Only multisite periods owned by this multisite day template will be merged!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        public virtual void MergeTemplateMultisitePeriods(IList<ITemplateMultisitePeriod> list)
        {
            list = list.OrderBy(s => s.Period.StartDateTime).ToList();
            TemplateMultisitePeriod newMultisitePeriod = new TemplateMultisitePeriod(
                            new DateTimePeriod(list[0].Period.StartDateTime, list.Last().Period.EndDateTime),
                            new Dictionary<IChildSkill, Percent>(list[0].Distribution));
            newMultisitePeriod.SetParent(this);
            list.ForEach(i => _templateMultisitePeriodCollection.Remove(i));
            _templateMultisitePeriodCollection.Add(newMultisitePeriod);
            IncreaseVersionNumber();
        }

	    public virtual object Clone()
        {
            return NoneEntityClone();
        }

	    public virtual IMultisiteDayTemplate NoneEntityClone()
        {
            MultisiteDayTemplate retobj = (MultisiteDayTemplate)MemberwiseClone();
	        retobj.SetId(null);
            retobj._templateMultisitePeriodCollection = new HashSet<ITemplateMultisitePeriod>();
            foreach (ITemplateMultisitePeriod templateSkillDataPeriod in _templateMultisitePeriodCollection)
            {
                ITemplateMultisitePeriod clonedPeriod = templateSkillDataPeriod.NoneEntityClone();
                clonedPeriod.SetParent(retobj);
                retobj._templateMultisitePeriodCollection.Add(clonedPeriod);
            }
            return retobj;
        }

        public virtual IMultisiteDayTemplate EntityClone()
        {
            MultisiteDayTemplate retobj = (MultisiteDayTemplate)MemberwiseClone();
            retobj._templateMultisitePeriodCollection = new HashSet<ITemplateMultisitePeriod>();
            foreach (ITemplateMultisitePeriod templateSkillDataPeriod in _templateMultisitePeriodCollection)
            {
                ITemplateMultisitePeriod clonedPeriod = templateSkillDataPeriod.EntityClone();
                clonedPeriod.SetParent(retobj);
                retobj._templateMultisitePeriodCollection.Add(clonedPeriod);
            }
            return retobj;
        }
    }
}
