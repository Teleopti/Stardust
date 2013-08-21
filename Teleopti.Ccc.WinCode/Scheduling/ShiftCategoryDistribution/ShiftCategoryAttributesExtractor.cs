using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public interface IShiftCategoryAttributesExtractor
    {
        void ExtractShiftCategoryInformation(IList<ShiftCategoryStructure> shiftCategoryStructureList);
        IList<String> ShiftCategories { get; }
        IList<IPerson> PersonInvolved { get; }
        IList<DateOnly> Dates { get; }
    }
    
    public class ShiftCategoryAttributesExtractor : IShiftCategoryAttributesExtractor
    {
        private IList<string> _shiftCategories;
        private IList<IPerson> _persons; 
        private IList<DateOnly> _dates ; 

        public IList<string> ShiftCategories
        {
            get { return _shiftCategories; }
        }

        public IList<IPerson> PersonInvolved
        {
            get { return _persons; }
        }

        public IList<DateOnly> Dates
        {
            get { return _dates; }
        }

        public void ExtractShiftCategoryInformation(IList<ShiftCategoryStructure> shiftCategoryStructureList)
        {
            _shiftCategories = new List<string>();
            _persons  = new List<IPerson>();
            _dates = new List<DateOnly>();

            foreach (var shiftCategoryStructure in shiftCategoryStructureList)
            {
                if (!_shiftCategories.Contains(shiftCategoryStructure.ShiftCategoryValue.Description.Name))
                {
                    _shiftCategories.Add(shiftCategoryStructure.ShiftCategoryValue.Description.Name);
                }

                if (!_persons.Contains(shiftCategoryStructure.PersonValue))
                {
                    _persons.Add(shiftCategoryStructure.PersonValue );
                }

                if (!_dates.Contains(shiftCategoryStructure.DateOnlyValue))
                {
                    _dates.Add(shiftCategoryStructure.DateOnlyValue );
                }
                
            }
        }
    }
}