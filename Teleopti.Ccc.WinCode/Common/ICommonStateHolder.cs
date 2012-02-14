using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface ICommonStateHolder
    {
        /// <summary>
        /// Loads the common state holder.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-29
        /// </remarks>
        void LoadCommonStateHolder(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork);

        /// <summary>
        /// Gets the absences.
        /// </summary>
        /// <value>The absences.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-08
        /// </remarks>
        IList<IAbsence> Absences { get; }

        /// <summary>
        /// Gets the dayOffs
        /// </summary>
        IList<IDayOffTemplate> DayOffs { get; }

        /// <summary>
        /// Gets the activities.
        /// </summary>
        /// <value>The activities.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        IList<IActivity> Activities { get; }

        /// <summary>
        /// Gets the active activities (ie nondeleted).
        /// </summary>
        IList<IActivity> ActiveActivities { get; }

        /// <summary>
        /// Gets the shift categories.
        /// </summary>
        /// <value>The shift categories.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-16
        /// </remarks>
        IList<IShiftCategory> ShiftCategories { get; }
    }
}