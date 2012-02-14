using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SchedulingObjectContainer : ISchedulingObjectContainer
    {
        private readonly IList<IShiftCategory> _shiftCategories;
        public SchedulingObjectContainer(IList<IShiftCategory> shiftCategories)
        {
            _shiftCategories = shiftCategories;
        }
        public IList<IShiftCategory> ShiftCategories { get { return _shiftCategories; } }
        public IWorkShiftFinderService WorkShiftFinderService { get; set; }
        public IPersonSkillPeriodsDataHolderManager PersonSkillPeriodsDataHolderManager { get; set; }
        public IShiftProjectionCacheManager ShiftProjectionCacheManager { get; set; }
        public ISchedulingOptions Options { get; set; }
        public IShiftProjectionCacheFilter ShiftProjectionCacheFilter { get; set; }
        public IWorkShiftCalculator WorkShiftCalculator { get; set; }
        public IFairnessValueCalculator FairnessValueCalculator { get; set; }
        public IPreSchedulingStatusChecker PreSchedulingStatusChecker { get; set; }

    }
}
