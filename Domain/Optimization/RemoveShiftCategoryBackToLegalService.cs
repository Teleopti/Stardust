using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RemoveShiftCategoryBackToLegalService : IRemoveShiftCategoryBackToLegalService
    {
        private readonly IRemoveShiftCategoryOnBestDateService _removeShiftCategoryOnBestDateService;
        private readonly IScheduleMatrixPro _scheduleMatrix;

        public RemoveShiftCategoryBackToLegalService(IRemoveShiftCategoryOnBestDateService removeShiftCategoryOnBestDateService, 
                                                     IScheduleMatrixPro scheduleMatrix)
        {
            _removeShiftCategoryOnBestDateService = removeShiftCategoryOnBestDateService;
            _scheduleMatrix = scheduleMatrix;
        }

        public  IScheduleMatrixPro ScheduleMatrixPro
        {
            get { return _scheduleMatrix; }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IScheduleDayPro> Execute(IShiftCategoryLimitation shiftCategoryLimitation, SchedulingOptions schedulingOptions)
        {
            if(shiftCategoryLimitation.Weekly)
            {
                return ExecuteWeeks(shiftCategoryLimitation, schedulingOptions);
            }

            return ExecutePeriod(shiftCategoryLimitation, schedulingOptions);

        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IScheduleDayPro> ExecutePeriod(IShiftCategoryLimitation shiftCategoryLimitation, SchedulingOptions schedulingOptions)
        {
            IList<IScheduleDayPro> result = new List<IScheduleDayPro>();
            while (IsShiftCategoryOverPeriodLimit(shiftCategoryLimitation))
            {
                IScheduleDayPro thisResult =
                    _removeShiftCategoryOnBestDateService.ExecuteOne(shiftCategoryLimitation.ShiftCategory, schedulingOptions);
                if (thisResult != null)
                    result.Add(thisResult);
                else
                {
                    break;
                }
            }

            return result;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IScheduleDayPro> ExecuteWeeks(IShiftCategoryLimitation shiftCategoryLimitation, SchedulingOptions schedulingOptions)
        {

            IList<IScheduleDayPro> days = _scheduleMatrix.FullWeeksPeriodDays;
            IList<IScheduleDayPro> result = new List<IScheduleDayPro>();
            for (int o = 0; o < days.Count; o += 7)
            {
                int categoryCounter = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (_removeShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(days[o + i], shiftCategoryLimitation.ShiftCategory))
                        categoryCounter++;
                }
                while (categoryCounter > shiftCategoryLimitation.MaxNumberOf)
                {
                    DateOnlyPeriod periodForWeek = new DateOnlyPeriod(days[o].Day, days[o].Day.AddDays(6));
                    IScheduleDayPro thisResult =
                        _removeShiftCategoryOnBestDateService.ExecuteOne(shiftCategoryLimitation.ShiftCategory,
                                                                         periodForWeek, schedulingOptions);
                    if (thisResult != null)
                        result.Add(thisResult);
                    else
                    {
                        break;
                    }

                    categoryCounter = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        if (_removeShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(days[o + i], shiftCategoryLimitation.ShiftCategory))
                            categoryCounter++;
                    }
                }    
            }

            return result;
        }

        public bool IsShiftCategoryOverPeriodLimit(IShiftCategoryLimitation shiftCategoryLimitation)
        {
            if (shiftCategoryLimitation.Weekly)
                throw new ArgumentException("shiftCategoryLimitation.Weekly must be true");

            int categoryCounter = 0;
            
            foreach (var scheduleDay in _scheduleMatrix.EffectivePeriodDays)
            {
                if(_removeShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(scheduleDay, shiftCategoryLimitation.ShiftCategory))
                    categoryCounter++;
            }

            return (categoryCounter > shiftCategoryLimitation.MaxNumberOf);
        }
    }
}
