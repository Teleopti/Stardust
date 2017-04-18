using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    /// <summary>
    /// Logic for selecting a layer
    /// </summary>
    /// <remarks>
    /// Used for selecting a layer from a new collection if certain data matches
    /// Because sometimes a new LayerViewModelCollection is created when a user already has a selection
    /// Created by: henrika
    /// Created date: 2010-06-15
    /// </remarks>
    public interface ILayerViewModelSelector
    {
        /// <summary>
        /// Checks if the schedule affects same day and person as the selected LayerViewModel
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-06-15
        /// </remarks>
        bool ScheduleAffectsSameDayAndPerson(IScheduleDay schedule);

        /// <summary>
        /// Tries to select layer.
        /// </summary>
        /// <param name="layers">The layers.</param>
        /// <param name="observer">The observer.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-06-15
        /// </remarks>
        bool TryToSelectLayer(IList<ILayerViewModel> layers, ILayerViewModelObserver observer);

        /// <summary>
        /// Gets the selected layer.
        /// </summary>
        /// <value>The selected layer.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-06-15
        /// </remarks>
        ILayerViewModel SelectedLayer { get; }
    }
}