using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class MultisiteSkill : Skill, IMultisiteSkill
    {
        private ISet<IChildSkill> _childSkills = new HashSet<IChildSkill>();
        private IDictionary<int, IMultisiteDayTemplate> _templateMultisiteWeekCollection;

        /// <summary>
        /// For NHibernate
        /// </summary>
        protected MultisiteSkill()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="displayColor">The display color.</param>
        /// <param name="defaultSolution">The default solution.</param>
        /// <param name="skillType">Type of the skill.</param>
        public MultisiteSkill(string name, string description, Color displayColor, int defaultSolution, ISkillType skillType) :
            base(name, description, displayColor, defaultSolution, skillType)
		{
			_templateMultisiteWeekCollection = Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>().Select(dayOfWeek =>
			{
				string templateName =
					string.Format(CultureInfo.CurrentUICulture, "<{0}>",
						CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek).ToUpper(
							CultureInfo.CurrentUICulture));
				IMultisiteDayTemplate multisiteDayTemplate =
					new MultisiteDayTemplate(templateName, new List<ITemplateMultisitePeriod>());
				multisiteDayTemplate.SetParent(this);
				return ((int) dayOfWeek, multisiteDayTemplate);
			}).ToDictionary(k => k.Item1, v => v.Item2);
		}

		/// <summary>
		/// Gets the child skills.
		/// </summary>
		/// <value>The child skills.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-18
		/// </remarks>
		public virtual ReadOnlyCollection<IChildSkill> ChildSkills => new ReadOnlyCollection<IChildSkill>(_childSkills.Where(c => !((IDeleteTag)c).IsDeleted).ToArray());

		/// <summary>
		/// Sets the template at.
		/// First 7 slots are the standard WeekDays
		/// </summary>
		/// <param name="templateIndex">Index of the template.</param>
		/// <param name="dayTemplate">The day template.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-22
		/// </remarks>
		public override void SetTemplateAt(int templateIndex, IForecastDayTemplate dayTemplate)
        {
            base.SetTemplateAt(templateIndex,dayTemplate);
			if (dayTemplate is IMultisiteDayTemplate newTemplate)
            {
                newTemplate.SetParent(this);
                _templateMultisiteWeekCollection.Remove(templateIndex);
                _templateMultisiteWeekCollection.Add(templateIndex, newTemplate);
            }
        }

        /// <summary>
        /// Gets the template at given position.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="templateIndex">Index of the template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        public override IForecastDayTemplate GetTemplateAt(TemplateTarget target, int templateIndex)
        {
            if (target != TemplateTarget.Multisite) return base.GetTemplateAt(target, templateIndex);
            return _templateMultisiteWeekCollection[templateIndex];
        }

        /// <summary>
        /// Gets all multisite templates.
        /// </summary>
        /// <value>All multisite templates.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public virtual IDictionary<int, IMultisiteDayTemplate> TemplateMultisiteWeekCollection => new ReadOnlyDictionary<int, IMultisiteDayTemplate>(_templateMultisiteWeekCollection);

	    /// <summary>
        /// Tries to the find the template by name.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public override IForecastDayTemplate TryFindTemplateByName(TemplateTarget target, string name)
        {
            if (target != TemplateTarget.Multisite) return base.TryFindTemplateByName(target, name);

            //Returns null if it doesnt exists
            name = name.ToLower(CultureInfo.CurrentCulture);
            KeyValuePair<int, IMultisiteDayTemplate> pair = _templateMultisiteWeekCollection.FirstOrDefault(n => n.Value.Name.ToLower(CultureInfo.CurrentCulture) == name);

            return pair.Value;
        }

        /// <summary>
        /// Gets the restriction set.
        /// </summary>
        /// <value>The restriction set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public override IRestrictionSet<ISkill> RestrictionSet => MultisiteSkillRestrictionSet.CurrentRestrictionSet;

	    /// <summary>
        /// Adds the template.
        /// </summary>
        /// <param name="dayTemplate">The day template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        public override int AddTemplate(IForecastDayTemplate dayTemplate)
        {
            IMultisiteDayTemplate newTemplate = dayTemplate as IMultisiteDayTemplate;
            if (newTemplate == null) return base.AddTemplate(dayTemplate);

            newTemplate.SetParent(this);
            int nextFreeKey = _templateMultisiteWeekCollection.Max(k=>k.Key)+1;
            _templateMultisiteWeekCollection.Add(nextFreeKey, newTemplate);
            return nextFreeKey;
        }

        /// <summary>
        /// Removes the child skill.
        /// </summary>
        /// <param name="childSkill">The child skill.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        public virtual void RemoveChildSkill(IChildSkill childSkill)
        {
            if (!_childSkills.Contains(childSkill)) return;

            bool templateExists = (from t in _templateMultisiteWeekCollection.Values
                                   from p in t.TemplateMultisitePeriodCollection
                                   select p).Any(p =>
                                   {
	                                   Percent value;
	                                   return p.Distribution.TryGetValue(childSkill, out value) &&
	                                          value.Value > 0;
                                   });

            if (templateExists)
            {
                throw new ArgumentException("There are still templates using this sub skill. Make sure all templates have zero values first.");
            }

            //TODO! Maybe do this a nicer way? :)
            ((Skill)childSkill).SetDeleted();
        }

        /// <summary>
        /// Adds the child skill.
        /// </summary>
        /// <param name="childSkill">The child skill.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        public virtual void AddChildSkill(IChildSkill childSkill)
        {
            InParameter.NotNull(nameof(childSkill), childSkill);
            if (_childSkills.Contains(childSkill)) return;

            _childSkills.Add(childSkill);
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
        public override void RemoveTemplate(TemplateTarget target, string templateName)
        {
            if (target != TemplateTarget.Multisite)
            {
                base.RemoveTemplate(TemplateTarget.Skill, templateName);
            }
            else
            {
                IForecastDayTemplate template = TryFindTemplateByName(target, templateName);
                if (template != null)
                {
	                int key = _templateMultisiteWeekCollection.First(i => i.Value.Equals(template)).Key;
                    _templateMultisiteWeekCollection.Remove(key);
                }
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
        public override void SetDefaultTemplates(IEnumerable theDays)
        {
            base.SetDefaultTemplates(theDays);
            foreach (var multisiteDay in theDays.OfType<MultisiteDay>())
            {
                multisiteDay.ApplyTemplate((MultisiteDayTemplate) GetTemplate(TemplateTarget.Multisite, multisiteDay.MultisiteDayDate.DayOfWeek));
            }
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
        public override IDictionary<int, IForecastDayTemplate> GetTemplates(TemplateTarget target)
        {
            if (target != TemplateTarget.Multisite) 
                return base.GetTemplates(target);

            return
                new ReadOnlyDictionary<int, IForecastDayTemplate>(_templateMultisiteWeekCollection.ToDictionary(k => k.Key,
                                                                                                       i => (IForecastDayTemplate)i.Value));
        }

        public override void RefreshTemplates(IForecastTemplateOwner forecastTemplateOwner)
        {
            base.RefreshTemplates(forecastTemplateOwner);

            _templateMultisiteWeekCollection = new Dictionary<int, IMultisiteDayTemplate>(((MultisiteSkill)forecastTemplateOwner)._templateMultisiteWeekCollection);
        }

        public override ISkill NoneEntityClone()
        {
            MultisiteSkill retobj = (MultisiteSkill) base.NoneEntityClone();
            retobj._templateMultisiteWeekCollection = new Dictionary<int, IMultisiteDayTemplate>();
            foreach (KeyValuePair<int, IMultisiteDayTemplate> keyValuePair in _templateMultisiteWeekCollection)
            {
                IMultisiteDayTemplate template = keyValuePair.Value.NoneEntityClone();
                template.SetParent(retobj);
                retobj._templateMultisiteWeekCollection.Add(keyValuePair.Key, template);
            }
            retobj._childSkills = new HashSet<IChildSkill>();
            foreach (IChildSkill childSkill in _childSkills)
            {
                retobj.AddChildSkill(childSkill);
            }

            return retobj;
        }

        public override ISkill EntityClone()
        {
            MultisiteSkill retobj = (MultisiteSkill)base.EntityClone();
            retobj._templateMultisiteWeekCollection = new Dictionary<int, IMultisiteDayTemplate>();
            foreach (KeyValuePair<int, IMultisiteDayTemplate> keyValuePair in _templateMultisiteWeekCollection)
            {
                IMultisiteDayTemplate template = keyValuePair.Value.EntityClone();
                template.SetParent(retobj);
                retobj._templateMultisiteWeekCollection.Add(keyValuePair.Key, template);
            }
            retobj._childSkills = new HashSet<IChildSkill>();
            foreach (IChildSkill childSkill in _childSkills)
            {
                retobj.AddChildSkill(childSkill);
            }

            return retobj;
        }

		public virtual void ClearChildSkill(ChildSkill childSkill)
		{
			if (!_childSkills.Contains(childSkill)) return;
			_childSkills.Remove(childSkill);
		}
	}
}