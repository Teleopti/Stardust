﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupRemoveShiftCategoryBackToLegalService
    {
        IList<IScheduleDayPro> Execute(IShiftCategoryLimitation shiftCategoryLimitation, ISchedulingOptions schedulingOptions);
    }

    public class GroupRemoveShiftCategoryBackToLegalService : IGroupRemoveShiftCategoryBackToLegalService
    {
        private readonly IRemoveShiftCategoryOnBestDateService _removeShiftCategoryOnBestDateService;
        private readonly IScheduleMatrixPro _scheduleMatrix;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        public GroupRemoveShiftCategoryBackToLegalService(IRemoveShiftCategoryOnBestDateService removeShiftCategoryOnBestDateService,
                                                     IScheduleMatrixPro scheduleMatrix, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            _removeShiftCategoryOnBestDateService = removeShiftCategoryOnBestDateService;
            _scheduleMatrix = scheduleMatrix;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IList<IScheduleDayPro> Execute(IShiftCategoryLimitation shiftCategoryLimitation, ISchedulingOptions schedulingOptions)
        {
            if (shiftCategoryLimitation.Weekly)
            {
                return ExecuteWeeks(shiftCategoryLimitation, schedulingOptions);
            }

            return ExecutePeriod(shiftCategoryLimitation, schedulingOptions);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IList<IScheduleDayPro> ExecutePeriod(IShiftCategoryLimitation shiftCategoryLimitation, ISchedulingOptions schedulingOptions)
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
        public IList<IScheduleDayPro> ExecuteWeeks(IShiftCategoryLimitation shiftCategoryLimitation, ISchedulingOptions schedulingOptions)
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
                if (_removeShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(scheduleDay, shiftCategoryLimitation.ShiftCategory))
                    categoryCounter++;
            }

            return (categoryCounter > shiftCategoryLimitation.MaxNumberOf);
        }

        public bool IsShiftCategoryOverWeekLimit(IShiftCategoryLimitation shiftCategoryLimitation)
        {
            if (!shiftCategoryLimitation.Weekly)
                throw new ArgumentException("shiftCategoryLimitation.Weekly must not be true");

            IList<IScheduleDayPro> days = _scheduleMatrix.FullWeeksPeriodDays;

            for (int o = 0; o < days.Count; o += 7)
            {
                int categoryCounter = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (_removeShiftCategoryOnBestDateService.IsThisDayCorrectShiftCategory(days[o + i], shiftCategoryLimitation.ShiftCategory))
                        categoryCounter++;
                }
                if (categoryCounter > shiftCategoryLimitation.MaxNumberOf)
                    return true;
            }

            return false;
        }
    }
}
