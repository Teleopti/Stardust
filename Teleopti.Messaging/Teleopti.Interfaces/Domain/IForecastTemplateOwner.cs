using System;
using System.Collections;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for owners of template collections
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-07-02
    /// </remarks>
    public interface IForecastTemplateOwner
    {
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
        void SetTemplateAt(int templateIndex, IForecastDayTemplate dayTemplate);

        /// <summary>
        /// Sets the template.
        /// </summary>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <param name="dayTemplate">The day template.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-02-26
        /// </remarks>
        void SetTemplate(DayOfWeek dayOfWeek, IForecastDayTemplate dayTemplate);

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
        IForecastDayTemplate GetTemplateAt(TemplateTarget target, int templateIndex);

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
        IForecastDayTemplate GetTemplate(TemplateTarget target, DayOfWeek dayOfWeek);

        /// <summary>
        /// Adds a new template to the list.
        /// First 7 slots are the standard WeekDays
        /// </summary>
        /// <param name="dayTemplate">The day template.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        int AddTemplate(IForecastDayTemplate dayTemplate);

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
        IForecastDayTemplate TryFindTemplateByName(TemplateTarget target, string name);

        /// <summary>
        /// Removes the template.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="templateName">Name of the template.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-21
        /// </remarks>
        void RemoveTemplate(TemplateTarget target, string templateName);

        /// <summary>
        /// Sets the default templates.
        /// </summary>
        /// <param name="theDays">The days.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-02
        /// </remarks>
        void SetDefaultTemplates(IEnumerable theDays);
        ///<summary>
        /// Sets the longterm template
        ///</summary>
        ///<param name="theDays">The days</param>
        void SetLongtermTemplate(IEnumerable theDays);
        /// <summary>
        /// Gets the templates.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-02
        /// </remarks>
        IDictionary<int, IForecastDayTemplate> GetTemplates(TemplateTarget target);

        /// <summary>
        /// Refreshes the templates.
        /// </summary>
        /// <param name="forecastTemplateOwner">The forecast template owner.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-01-06
        /// </remarks>
        void RefreshTemplates(IForecastTemplateOwner forecastTemplateOwner);

        /// <summary>
        /// Sets the templates by name.
        /// </summary>
        /// <param name="templateTarget">The template target.</param>
        /// <param name="name">The name.</param>
        /// <param name="days">The days.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-29
        /// </remarks>
        void SetTemplatesByName(TemplateTarget templateTarget, string name, IList<ITemplateDay> days);
    }
}