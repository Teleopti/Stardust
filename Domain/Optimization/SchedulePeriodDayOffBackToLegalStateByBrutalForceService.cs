using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{

    public class SchedulePeriodDayOffBackToLegalStateByBrutalForceService : ISchedulePeriodDayOffBackToLegalStateByBrutalForceService
    {
        private readonly IScheduleMatrixBitArrayConverter _scheduleMatrixConverter;
        private readonly IDayOffPlannerRules _dayOffPlannerRules;
        private readonly CultureInfo _agentsCulture;
        private bool? _result;
        private BitArray _resultArray;

        public SchedulePeriodDayOffBackToLegalStateByBrutalForceService(
            IScheduleMatrixBitArrayConverter scheduleMatrixConverter,
            IDayOffPlannerRules dayOffPlannerRules,
            CultureInfo agentsCulture
            )
        {
            _scheduleMatrixConverter = scheduleMatrixConverter;
            _dayOffPlannerRules = dayOffPlannerRules;
            _agentsCulture = agentsCulture;
        }

        public void Execute(IScheduleMatrixPro matrix)
        {
            BitArray periodDays = _scheduleMatrixConverter.OuterWeekPeriodDayOffsBitArray(matrix);
            MinMax<int> indexRange = _scheduleMatrixConverter.PeriodIndexRange(matrix);

            IEnumerable<IBinaryValidator> validators = createValidators(_scheduleMatrixConverter, _dayOffPlannerRules, _agentsCulture, matrix);
            var binaryPermutation = new BinaryPermutation(periodDays, validators, indexRange.Minimum, indexRange.Maximum);
            _resultArray = binaryPermutation.FindFirstValid();
            _result = (_resultArray != null);
        }

        public bool? Result
        {
            get { return _result; }
        }

        public BitArray ResultArray
        {
            get { return _resultArray; }
        }

        private static IEnumerable<IBinaryValidator> createValidators(IScheduleMatrixBitArrayConverter scheduleMatrixConverter, IDayOffPlannerRules dayOffPlannerRules,
            CultureInfo agentsCulture, IScheduleMatrixPro matrix)
        {
            BitArray lockedDays = scheduleMatrixConverter.OuterWeekPeriodLockedDaysBitArray(matrix);
            BitArray periodDays = scheduleMatrixConverter.OuterWeekPeriodDayOffsBitArray(matrix);
            MinMax<int> periodRange = scheduleMatrixConverter.PeriodIndexRange(matrix);
            IOfficialWeekendDays officialWeekendDays = new OfficialWeekendDays(agentsCulture);

            IDayOffLegalStateValidatorListCreator validatorListCreator = new DayOffBackToLegalStateValidatorListCreator(dayOffPlannerRules, officialWeekendDays, periodRange);
            IList<IDayOffLegalStateValidator> validatorList = validatorListCreator.BuildActiveValidatorList();

            IBinaryValidator schedulePeriodDayOffLegalStateValidator = new SchedulePeriodDayOffLegalStateValidator(validatorList, periodRange);
            IBinaryValidator lockedDayValidator = new LockedValueValidator((BitArray) periodDays.Clone(), lockedDays);
            IBinaryValidator bitArrayValueValidator = new ArrayValueValidator(periodDays);

            return new List<IBinaryValidator>
                       {
                           bitArrayValueValidator,
                           lockedDayValidator,
                           schedulePeriodDayOffLegalStateValidator
                       };
        }
    }
}
