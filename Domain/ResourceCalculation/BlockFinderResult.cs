using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class BlockFinderResult : IBlockFinderResult
    {
        private readonly IShiftCategory _shiftCategory;
        private readonly IList<DateOnly> _dateOnlyList;
        private readonly IDictionary<string, IWorkShiftFinderResult> _workShiftFinderResult;

        private BlockFinderResult(){}

        public BlockFinderResult(IShiftCategory shiftCategory, IList<DateOnly> dateOnlyList, IDictionary<string, IWorkShiftFinderResult> workShiftFinderResult)
            : this()
        {
            _shiftCategory = shiftCategory;
            _dateOnlyList = dateOnlyList;
            _workShiftFinderResult = workShiftFinderResult;
        }

        public IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
        }

        public IList<DateOnly> BlockDays
        {
            get { return _dateOnlyList; }
        }

        public IDictionary<string, IWorkShiftFinderResult> WorkShiftFinderResult
        {
            get { return _workShiftFinderResult; }
        }
    }
}