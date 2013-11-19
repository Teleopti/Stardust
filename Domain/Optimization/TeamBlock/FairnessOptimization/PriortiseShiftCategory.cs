using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface IPriortiseShiftCategory
    {
        int HigestPriority { get; }
        int LowestPriority { get; }
        int AveragePriority { get; }
        IDictionary<int, IShiftCategory> GetPriortiseShiftCategories(IList<IShiftCategory> shiftCategories);
        IShiftCategory ShiftCategoryOnPriority(int priority);
        int PriorityOfShiftCategory(IShiftCategory shiftCategory);
        void Clear();
    }

    public class PriortiseShiftCategory : IPriortiseShiftCategory
    {
        private readonly IDictionary<int, IShiftCategory> _result = new Dictionary<int, IShiftCategory>();

        public int AveragePriority
        {
            get { return (int) _result.Keys.Average(); }
        }

        /// <summary>
        ///     This method will change in future and a test should be written for that
        /// </summary>
        /// <param name="shiftCategories"></param>
        /// <returns></returns>
        public IDictionary<int, IShiftCategory> GetPriortiseShiftCategories(IList<IShiftCategory> shiftCategories)
        {
            int priority = 1;
            foreach (IShiftCategory shiftCategory in shiftCategories)
            {
                _result.Add(priority, shiftCategory);
                priority++;
            }
            return _result;
        }

        public int HigestPriority
        {
            get { return _result.Keys.Max(); }
        }

        public int LowestPriority
        {
            get { return _result.Keys.Min(); }
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