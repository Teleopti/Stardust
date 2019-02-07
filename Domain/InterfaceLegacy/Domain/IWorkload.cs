using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// A forecast
    /// </summary>
    public interface IWorkload : ICloneableEntity<IWorkload>, IAggregateRoot, IChangeInfo,
								IForecastTemplateOwner,
                                IFilterOnBusinessUnit
    {
        /// <summary>
        /// Gets and sets the name of the Workload
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Points to a Skill
        /// </summary>
        ISkill Skill { get; set; }

        /// <summary>
        /// Gets the forecast.
        /// Read only wrapper around the forecast list.
        /// </summary>
        /// <value>The forecast.</value>
        ReadOnlyCollection<IQueueSource> QueueSourceCollection { get; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; set; }

        /// <summary>
        /// Gets all templates.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-12
        /// </remarks>
        IDictionary<int, IWorkloadDayTemplate> TemplateWeekCollection { get; }

        ///<summary>
        /// Calculation rules for statistics from queues
        ///</summary>
        QueueAdjustment QueueAdjustments { get; set; }

        /// <summary>
        /// Adds a QueueSource to the Forecast collection
        /// </summary>
        /// <param name="queueSource"></param>
        void AddQueueSource(IQueueSource queueSource);

        /// <summary>
        /// Removes all QueueSources
        /// </summary>
        void RemoveAllQueueSources();

        /// <summary>
        /// Removes the queue source.
        /// </summary>
        /// <param name="queueSource">The queueSource.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-11-19
        /// </remarks>
        void RemoveQueueSource(IQueueSource queueSource);
    }
}
