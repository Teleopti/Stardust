using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
    public interface IShiftCategoryInformationExtractor
    {
        void ExtractShiftCategoryInformation(List<ShiftCategoryStructure> shiftCategoryStructureList);
        List<String> ShiftCategories { get; }
        List<IPerson> PersonInvolved { get; }
        List<DateOnly> Dates { get; }
    }
    
    public class ShiftCategoryInformationExtractor : IShiftCategoryInformationExtractor
    {
        private List<string> _shiftCategories;
        private List<IPerson> _persons; 
        private List<DateOnly> _dates ; 

        public List<string> ShiftCategories
        {
            get { return _shiftCategories; }
        }

        public List<IPerson> PersonInvolved
        {
            get { return _persons; }
        }

        public List<DateOnly> Dates
        {
            get { return _dates; }
        }

        public void ExtractShiftCategoryInformation(List<ShiftCategoryStructure> shiftCategoryStructureList)
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