using System.ComponentModel;
using System.Windows.Input;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
 
    public interface IRestrictionViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the restriction.
        /// </summary>
        /// <value>The restriction.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-15
        /// </remarks>
        IRestrictionBase Restriction { get; set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-15
        /// </remarks>
        string Description { get; }

        /// <summary>
        /// Gets the update command model.
        /// </summary>
        /// <value>The update command model.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-15
        /// </remarks>
        CommandModel UpdateCommandModel { get; }
        
        ILimitationViewModel StartTimeLimits { get; }
        ILimitationViewModel EndTimeLimits { get; }
        ILimitationViewModel WorkTimeLimits { get; }
        IScheduleData PersistableScheduleData { get; set; }


        /// <summary>
        /// Gets the update on event command for connectiong routedevents
        /// </summary>
        /// <value>The update on event command.</value>
        /// <remarks>
        /// Use with EventBehaviorFactory
        /// </remarks>
        ICommand UpdateOnEventCommand { get; }

        /// <summary>
        /// Determines whether this all data for this instance is valid
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        bool IsValid();

        /// <summary>
        /// Commits the changes.
        /// </summary>
        void CommitChanges();

        IScheduleDay ScheduleDay { get; set; }
        
        bool BelongsToPart();
    }
}
