namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Removes, get the period back to legal state and then reschedules the days that the decision maker found.
    /// </summary>
    public interface IDayOffDecisionMakerExecuter
    {
        /// <summary>
        /// Executes the service.
        /// </summary>
        /// <param name="workingBitArray">The working bit array.</param>
        /// <param name="originalBitArray">The original bit array.</param>
        /// <param name="currentScheduleMatrix">The currentScheduleMatrix.</param>
        /// <param name="originalStateContainer">The original state container.</param>
        /// <param name="doReschedule">if set to <c>true</c> [re schedule].</param>
        /// <param name="handleDayOffConflict">if set to <c>true</c> [handle day off conflict].</param>
        /// <returns></returns>
        bool Execute(
            ILockableBitArray workingBitArray, 
            ILockableBitArray originalBitArray, 
            IScheduleMatrixPro currentScheduleMatrix,
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            bool doReschedule, 
            bool handleDayOffConflict);
    }
}
