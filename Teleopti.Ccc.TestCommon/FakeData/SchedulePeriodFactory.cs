using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-06-15
    /// </remarks>
  public static class SchedulePeriodFactory
    {

        /// <summary>
        /// Creates a week lenth schedule period from the <paramref name="selectedDate"/> parameter.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-15
        /// </remarks>
      public static ISchedulePeriod CreateSchedulePeriod(DateOnly selectedDate)
      {
         return new SchedulePeriod(selectedDate, SchedulePeriodType.Week, 1);
      }
      /// <summary>
      /// Creates the schedule period.
      /// </summary>
      /// <param name="selectedDate">The selected date.</param>
      /// <param name="periodType">Type of the period.</param>
      /// <param name="number">The number.</param>
      /// <returns></returns>
      public static ISchedulePeriod CreateSchedulePeriod(DateOnly selectedDate, SchedulePeriodType periodType, int number)
      {
          return new SchedulePeriod(selectedDate, periodType, number);
      }
    }
}
