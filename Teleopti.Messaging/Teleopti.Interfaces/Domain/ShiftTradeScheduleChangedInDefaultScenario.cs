namespace Teleopti.Interfaces.Domain
{
    ///<summary>
	/// A message that schedule has changed in the default scenario and the shift trade request is denied.
	///</summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IShiftTradeScheduleChangedInDefaultScenario
    {
    }

    ///<summary>
    /// A message that schedule has changed in the default scenario.
    ///</summary>
    public class ShiftTradeScheduleChangedInDefaultScenario : IShiftTradeScheduleChangedInDefaultScenario
    {
    }
}
