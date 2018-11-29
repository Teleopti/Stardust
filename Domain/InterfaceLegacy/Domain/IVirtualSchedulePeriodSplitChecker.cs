namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Check if a schedule period should be splitted due to breaking changes in PersonPeriods
    /// </summary>
    public interface IVirtualSchedulePeriodSplitChecker
    {
        /// <summary>
        /// Check if necessary to split
        /// </summary>
        /// <param name="schedulePeriod"></param>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        DateOnlyPeriod? Check(DateOnlyPeriod schedulePeriod, DateOnly dateOnly);
    }
}
