using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Logic for removing Layer from schedule
    /// </summary>
    /// <remarks>
    /// Finds the Shift that contains the layer
    /// </remarks>
    public interface IRemoveLayerFromSchedule
    {

        /// <summary>
        /// Removes the specified layer if it exists.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="layer">The layer.</param>
        void Remove(IScheduleDay part, ShiftLayer layer);

        /// <summary>
        /// Removes the specified layer if it exists.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="layer">The layer.</param>
        /// <returns>The schedulepart</returns>
        void Remove(IScheduleDay part, ILayer<IAbsence> layer);

    }
}
