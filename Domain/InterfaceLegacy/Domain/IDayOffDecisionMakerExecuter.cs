using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Removes, get the period back to legal state and then reschedules the days that the decision maker found.
	/// </summary>
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
	public interface IDayOffDecisionMakerExecuter
    {
		/// <summary>
		/// Executes the service.
		/// </summary>
		/// <param name="workingBitArray">The working bit array.</param>
		/// <param name="originalBitArray">The original bit array.</param>
		/// <param name="currentScheduleMatrix">The currentScheduleMatrix.</param>
		/// <param name="originalStateContainer">The original state container.</param>
		/// <returns></returns>
        bool Execute(
            ILockableBitArray workingBitArray, 
            ILockableBitArray originalBitArray, 
            IScheduleMatrixPro currentScheduleMatrix,
            IScheduleMatrixOriginalStateContainer originalStateContainer);
    }
}
