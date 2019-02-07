using System;
using System.Collections.Generic;
using System.Drawing;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a skill.
    /// </summary>
    public interface ISkill : IAggregateRoot, IChangeInfo,
								IRestrictionChecker<ISkill>, 
                                IForecastTemplateOwner,
                                IFilterOnBusinessUnit,
                                ICloneableEntity<ISkill>, IAggregateSkill
        
    {
        /// <summary>
        /// SkillType
        /// </summary>
        ISkillType SkillType { get; set; }

        /// <summary>
        /// Gets/Sets the color of a Skill
        /// </summary>
        Color DisplayColor { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the forecast.
        /// Read only wrapper around the workload list.
        /// </summary>
        /// <value>The workload.</value>
        IEnumerable<IWorkload> WorkloadCollection { get; }

        /// <summary>
        /// Gets or sets the activity.
        /// </summary>
        /// <value>The activity.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-27
        /// </remarks>
        IActivity Activity { get; set; }

        /// <summary>
        /// Gets or sets the default solution.
        /// </summary>
        /// <value>The default solution.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-01-28
        /// </remarks>
        int DefaultResolution { get; set; }

        /// <summary>
        /// Gets all templates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        IDictionary<int, ISkillDayTemplate> TemplateWeekCollection { get; }

        /// <summary>
        /// Gets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-19
        /// </remarks>
        TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the staffing thresholds.
        /// </summary>
        /// <value>The staffing thresholds.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-16
        /// </remarks>
        StaffingThresholds StaffingThresholds { get; set; }

        /// <summary>
        /// Gets or sets the midnight break offset.
        /// </summary>
        /// <value>The midnight break offset.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/15/2008
        /// </remarks>
        TimeSpan MidnightBreakOffset { get; set; }

        /// <summary>
        /// Adds a Workload to the Forecast collection
        /// </summary>
        /// <param name="workload"></param>
        void AddWorkload(IWorkload workload);

        /// <summary>
        /// Removes the workload.
        /// </summary>
        /// <param name="workload">The workload.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-11-19
        /// </remarks>
        void RemoveWorkload(IWorkload workload);

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
        void SetTemplateAt(int templateIndex, ISkillDayTemplate newTemplate);

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
        ISkillDayTemplate GetTemplateAt(int templateIndex);

        /// <summary>
        /// Adds a new template to the list.
        /// First 7 slots are the standard WeekDays
        /// </summary>
        /// <param name="newTemplate">The new template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        int AddTemplate(ISkillDayTemplate newTemplate);

        /// <summary>
        /// Removes the template.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-21
        /// </remarks>
        void RemoveTemplate(string templateName);

        /// <summary>
        /// Tries the name of the find template by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-03
        /// </remarks>
        ISkillDayTemplate TryFindTemplateByName(string name);

		/// <summary>
		/// How many tasks can be hadled in parallel. Used for chat skill. Defaults to 3 on chat skill.
		/// Always 1 on all other. Must be between 1 and 100.
		/// </summary>
		/// <remarks>
		/// Created by: Ola
		/// Created date: 2013-06-13
		/// </remarks>
		int MaxParallelTasks { get; set; }

		/// <summary>
		/// What is the abandon rate for the skill. Used to calculate average patience in Erlang A. 
		/// Only used for Phone and Chat. Must be between 0 and 1
		/// </summary>
		/// <remarks>
		/// Created by: Oskar
		/// Created date: 2018-04-30
		/// </remarks>
		Percent AbandonRate { get; set; }

		/// <summary>
		/// Changes the name of the skill, also publishes a SkillnameChangedEvent
		/// </summary>
		/// <remarks>
		/// Created by: erikn
		/// Created date: 2016-04-28
		/// </remarks>
		void ChangeName(string name);

	    void SetCascadingIndex(int index);
	    void ClearCascadingIndex();
		int? CascadingIndex { get; }
		bool IsChildSkill { get; }
		bool IsCascading();
    }
}
