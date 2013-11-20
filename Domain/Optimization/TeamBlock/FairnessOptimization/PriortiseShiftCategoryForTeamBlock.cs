using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IPriortiseShiftCategoryForTeamBlock
    {
        int AveragePriority { get; }
        IDictionary<int, IShiftCategory> GetPriortiseShiftCategories(IList<IShiftCategory> shiftCategories);
        IShiftCategory ShiftCategoryOnPriority(int priority);
        int PriorityOfShiftCategory(IShiftCategory shiftCategory);
        void Clear();
    }

    public class PriortiseShiftCategoryForTeamBlock : IPriortiseShiftCategoryForTeamBlock
    {
        private readonly IShiftCategoryPoints _shiftCategoryPoints;
        private readonly IDictionary<int, IShiftCategory> _result = new Dictionary<int, IShiftCategory>();

        public PriortiseShiftCategoryForTeamBlock(IShiftCategoryPoints shiftCategoryPoints)
        {
            _shiftCategoryPoints = shiftCategoryPoints;
        }

        public int AveragePriority
        {
            get { return (int) Math.Round( _result.Keys.Average(),MidpointRounding.AwayFromZero ) ; }
        }

        /// <summary>
        ///     This method will change in future and a test should be written for that
        /// </summary>
        /// <param name="shiftCategories"></param>
        /// <returns></returns>
        public IDictionary<int, IShiftCategory> GetPriortiseShiftCategories(IList<IShiftCategory> shiftCategories)
        {
            foreach (IShiftCategory shiftCategory in shiftCategories)
            {
                _result.Add(_shiftCategoryPoints.GetPointOfShiftCategory(shiftCategory), shiftCategory);
            }
            return _result;
        }

        public IShiftCategory ShiftCategoryOnPriority(int priority)
        {
            return _result[priority];
        }

        public int PriorityOfShiftCategory(IShiftCategory shiftCategory)
        {
            foreach (var scPair in _result)
                if (scPair.Value == shiftCategory)
                    return scPair.Key;
            return -1;
        }

        public void Clear()
        {
            _result.Clear();
        }
    }
}