using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Forecasting
{

    /// <summary>
    /// A forecast
    /// </summary>
    public class Workload : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IWorkload, IDeleteTag
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private ISkill _skill;
        private IList<IQueueSource> _queueSourceCollection = new List<IQueueSource>();
        private IDictionary<int, IWorkloadDayTemplate> _templateWeekCollection;
        private QueueAdjustment _queueAdjustments;
        private bool _isDeleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="Workload"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-05-30
        /// </remarks>
        protected Workload()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Workload"/> class.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-06-02
        /// </remarks>
        public Workload(ISkill skill) : this()
        {
            AssociateWithSkill(skill);
            setDefaultQueueAdjustments();
            createWorkloadDays();
        }

        private void setDefaultQueueAdjustments()
        {
            _queueAdjustments = new QueueAdjustment
                                    {
                                        OfferedTasks = new Percent(1),
                                        OverflowIn = new Percent(1),
                                        OverflowOut = new Percent(-1),
                                        Abandoned = new Percent(-1),
                                        AbandonedShort = new Percent(0),
                                        AbandonedWithinServiceLevel = new Percent(1),
                                        AbandonedAfterServiceLevel = new Percent(1)
                                    };
        }

        /// <summary>
        /// Gets and sets the name of the Workload
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
            set
            {
                InParameter.NotStringEmptyOrNull(nameof(Name), value);
                InParameter.StringTooLong(nameof(Name),value,50);
                _name = value;
            }
        }

        /// <summary>
        /// Creates the workload days.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-06-02
        /// </remarks>
        private void createWorkloadDays()
        {
            _templateWeekCollection = new Dictionary<int, IWorkloadDayTemplate>();
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

                string templateName =
                    string.Format(CultureInfo.CurrentUICulture, "<{0}>",
                                  CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek).ToUpper(
                                      CultureInfo.CurrentUICulture));
                //CreateProjection closed template day
                workloadDayTemplate.Create(templateName, DateTime.UtcNow, this, new List<TimePeriod>());
                ((WorkloadDayTemplate)workloadDayTemplate).SetParent(this);
                _templateWeekCollection.Add((int)dayOfWeek, workloadDayTemplate);
            }
        }

        /// <summary>
        /// Assosiates to skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-05-30
        /// </remarks>
        private void AssociateWithSkill(ISkill skill)
        {
            InParameter.NotNull(nameof(skill), skill);
            _skill = skill;
            _skill.AddWorkload(this);
        }


        /// <summary>
        /// Points to a Skill
        /// </summary>
        public virtual ISkill Skill
        {
            get { return _skill; }
            set {
                InParameter.NotNull(nameof(value), value);
                _skill = value; }
        }

        /// <summary>
        /// Adds a QueueSource to the Forecast collection
        /// </summary>
        /// <param name="queueSource"></param>
        public virtual void AddQueueSource(IQueueSource queueSource)
        {
            InParameter.NotNull(nameof(queueSource), queueSource);
            _queueSourceCollection.Add(queueSource);
        }

        /// <summary>
        /// Removes all QueueSources
        /// </summary>
        public virtual void RemoveAllQueueSources()
        {
            _queueSourceCollection.Clear();
        }

        /// <summary>
        /// Gets the forecast.
        /// Read only wrapper around the forecast list.
        /// </summary>
        /// <value>The forecast.</value>
        public virtual ReadOnlyCollection<IQueueSource> QueueSourceCollection => new ReadOnlyCollection<IQueueSource>(_queueSourceCollection);

	    /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public virtual string Description
        {
            get { return _description; }
            set { _description = value;} 
        }

        /// <summary>
        /// Removes the queue source.
        /// </summary>
        /// <param name="queueSource">The queueSource.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-11-19
        /// </remarks>
        public virtual void RemoveQueueSource(IQueueSource queueSource)
        {
            _queueSourceCollection.Remove(queueSource);
        }

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
            IWorkloadDayTemplate workloadDayTemplate = dayTemplate as IWorkloadDayTemplate;
            //if (workloadDayTemplate==null) th TODO! Test for this!
                if (!Equals(workloadDayTemplate.Workload)) throw new ArgumentException("The workload template must be created using this workload.");
            dayTemplate.SetParent(this);
            _templateWeekCollection[templateIndex] = workloadDayTemplate;
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
            SetTemplateAt((int) dayOfWeek, dayTemplate);
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
            return GetTemplateAt(target,(int) dayOfWeek);
        }


        /// <summary>
        /// Adds a new template to the list.
        /// First 7 slots are the standard WeekDays
        /// </summary>
        /// <param name="dayTemplate">The day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        public virtual int AddTemplate(IForecastDayTemplate dayTemplate)
        {
            IWorkloadDayTemplate workloadDayTemplate = dayTemplate as IWorkloadDayTemplate;

            int nextFreeKey = _templateWeekCollection.Max(k => k.Key) + 1;
            _templateWeekCollection.Add(nextFreeKey, workloadDayTemplate);
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
        public virtual IDictionary<int, IWorkloadDayTemplate> TemplateWeekCollection
        {
            get { return new ReadOnlyDictionary<int, IWorkloadDayTemplate>(_templateWeekCollection); }
        }

        public virtual QueueAdjustment QueueAdjustments
        {
            get { return _queueAdjustments; }
            set { _queueAdjustments = value; }
        }

        public virtual bool IsDeleted => _isDeleted;

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
            KeyValuePair<int, IWorkloadDayTemplate> pair = _templateWeekCollection.FirstOrDefault(n => n.Value.Name.ToLower(CultureInfo.CurrentCulture) == name);

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
            IForecastDayTemplate template = TryFindTemplateByName(target,templateName);
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
            foreach (var workloadDay in theDays.OfType<WorkloadDay>())
            {
            	workloadDay.ApplyTemplate(
            		(WorkloadDayTemplate) GetTemplate(TemplateTarget.Workload, workloadDay.CurrentDate.DayOfWeek),
            		day => day.Lock(), day => {});
            }
        }

        public virtual void SetLongtermTemplate(IEnumerable theDays)
        {
            foreach (var workloadDay in theDays.OfType<WorkloadDay>())
            {
                var originalTemplate =
                    (WorkloadDayTemplate) GetTemplate(TemplateTarget.Workload, workloadDay.CurrentDate.DayOfWeek);

                IWorkloadDayTemplate newTemplate = new WorkloadDayTemplate();
                newTemplate.Create(TemplateReference.LongtermTemplateKey, DateTime.UtcNow, this, new List<TimePeriod>(originalTemplate.OpenHourList));
                newTemplate.MergeTemplateTaskPeriods(newTemplate.TaskPeriodList.ToList());
                workloadDay.ApplyTemplate(newTemplate, day => day.Lock(), day => day.Release());
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
        public virtual IDictionary<int, IForecastDayTemplate> GetTemplates(TemplateTarget target)
        {
            return
                new ReadOnlyDictionary<int, IForecastDayTemplate>(_templateWeekCollection.ToDictionary(k => k.Key, i => (IForecastDayTemplate)i.Value));
        }

        public virtual void RefreshTemplates(IForecastTemplateOwner forecastTemplateOwner)
        {
            if (!Equals(forecastTemplateOwner)) throw new ArgumentException("The supplied template owner must be of the same workload", nameof(forecastTemplateOwner));

            _templateWeekCollection = new Dictionary<int, IWorkloadDayTemplate>(((Workload)forecastTemplateOwner)._templateWeekCollection);

            foreach (var workloadDayTemplate in _templateWeekCollection)
            {
                workloadDayTemplate.Value.SetWorkloadInstance(this);
            }
        }

        #region Implementation of ICloneable

        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        #endregion

        #region Implementation of ICloneableEntity<IWorkload>

        public virtual IWorkload NoneEntityClone()
        {
            Workload retobj = (Workload)MemberwiseClone();
	        CloneEvents(retobj);
			retobj.SetId(null);
            retobj._templateWeekCollection = new Dictionary<int, IWorkloadDayTemplate>();
            foreach (KeyValuePair<int, IWorkloadDayTemplate> keyValuePair in _templateWeekCollection)
            {
                IWorkloadDayTemplate template = keyValuePair.Value.NoneEntityClone();
                template.SetParent(retobj);
                retobj._templateWeekCollection.Add(keyValuePair.Key,template);
            }
            retobj._queueSourceCollection = new List<IQueueSource>();
            foreach (IQueueSource queueSource in _queueSourceCollection)
            {
                retobj.AddQueueSource(queueSource);
            }

            return retobj;
        }

        public virtual IWorkload EntityClone()
        {
            Workload retobj = (Workload)MemberwiseClone();
	        CloneEvents(retobj);
			retobj._templateWeekCollection = new Dictionary<int, IWorkloadDayTemplate>();
            foreach (KeyValuePair<int, IWorkloadDayTemplate> keyValuePair in _templateWeekCollection)
            {
                IWorkloadDayTemplate template = keyValuePair.Value.EntityClone();
                template.SetParent(retobj);
                retobj._templateWeekCollection.Add(keyValuePair.Key, template);
            }
            retobj._queueSourceCollection = new List<IQueueSource>();
            foreach (IQueueSource queueSource in _queueSourceCollection)
            {
                retobj.AddQueueSource(queueSource);
            }

            return retobj;
        }

        #endregion

        public virtual void SetTemplatesByName(TemplateTarget templateTarget, string name, IList<ITemplateDay> days)
        {
            IForecastDayTemplate template = TryFindTemplateByName(templateTarget, name);

            TaskOwnerHelper period = new TaskOwnerHelper(days);
            period.BeginUpdate();
            foreach (var workloadDay in days.OfType<IWorkloadDay>())
            {
                workloadDay.ApplyTemplate((IWorkloadDayTemplate)template, day => {}, day => {});
            }
            period.EndUpdate();
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

	    public override void NotifyTransactionComplete(DomainUpdateType operation)
	    {
		    base.NotifyTransactionComplete(operation);
			switch (operation)
			{
				case DomainUpdateType.Insert:
				case DomainUpdateType.Update:
				case DomainUpdateType.Delete:
					AddEvent(new WorkloadChangedEvent
					{
						WorkloadId = Id.GetValueOrDefault()
					});
					break;
			}
		}
    }
}