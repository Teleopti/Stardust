using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class SchedulePeriodShiftCategoryBackToLegalStateService : ISchedulePeriodShiftCategoryBackToLegalStateService
    {
        private readonly IRemoveShiftCategoryBackToLegalService _removeShiftCategoryBackToLegalService;
        private readonly IScheduleDayService _scheduleDayService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulePeriodShiftCategoryBackToLegalStateService"/> class.
        /// </summary>
        /// <param name="removeShiftCategoryBackToLegalService">The remove shift category back to legal service.</param>
        /// <param name="scheduleDayService">The schedule day service.</param>
        public SchedulePeriodShiftCategoryBackToLegalStateService(
            IRemoveShiftCategoryBackToLegalService removeShiftCategoryBackToLegalService,
            IScheduleDayService scheduleDayService)
        {
            _removeShiftCategoryBackToLegalService = removeShiftCategoryBackToLegalService;
            _scheduleDayService = scheduleDayService;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Execute(IVirtualSchedulePeriod schedulePeriod, SchedulingOptions schedulingOptions)
        {
            bool result = true;
            var resultList = new List<IScheduleDayPro>();
            foreach (IShiftCategoryLimitation limitation in schedulePeriod.ShiftCategoryLimitationCollection())
            {
                resultList.AddRange(_removeShiftCategoryBackToLegalService.Execute(limitation, schedulingOptions));
            }

            foreach (var scheduleDayPro in resultList)
            {
                result = result & _scheduleDayService.ScheduleDay(scheduleDayPro.DaySchedulePart(), schedulingOptions);
            }
            return result;
        }
    }
}
