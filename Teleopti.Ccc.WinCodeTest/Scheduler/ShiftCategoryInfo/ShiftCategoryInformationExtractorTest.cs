using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryInfo
{
    [TestFixture]
    public class ShiftCategoryInformationExtractorTest
    {
        private MockRepository _mocks;
        private IShiftCategoryInformationExtractor _target;
        private ShiftCategoryStructure _shiftCategoryStructure1;
        private ShiftCategoryStructure _shiftCategoryStructure2;
        private ShiftCategoryStructure _shiftCategoryStructure3;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new ShiftCategoryInformationExtractor();
            
        }

        [Test]
        public void CheckIfCorrectListIsMapped()
        {
            IPerson person1 = PersonFactory.CreatePerson("person1");
            IPerson person2 = PersonFactory.CreatePerson("person2");
            IPerson person3 = PersonFactory.CreatePerson("person3");
            _shiftCategoryStructure1 = new ShiftCategoryStructure(new ShiftCategory("sc1"),DateOnly.Today,person1);
            _shiftCategoryStructure2 = new ShiftCategoryStructure(new ShiftCategory("sc1"),DateOnly.Today,person2);
            _shiftCategoryStructure3 = new ShiftCategoryStructure(new ShiftCategory("sc3"),DateOnly.Today,person3);
            var shiftCategoryStructureList = new List<ShiftCategoryStructure>
                {
                    _shiftCategoryStructure1,
                    _shiftCategoryStructure2,
                    _shiftCategoryStructure3
                };
            _target.ExtractShiftCategoryInformation(shiftCategoryStructureList);
            Assert.AreEqual(_target.ShiftCategories.Count, 2);
        }
    }

    public class ShiftCategoryInformationExtractor : IShiftCategoryInformationExtractor
    {
        private List<string> _shiftCategories;
        private Dictionary<string, DateOnly> _shiftCategoryOnDate;

        public List<String> ShiftCategories
        {
            get { return _shiftCategories; }
        } 
        
        public void ExtractShiftCategoryInformation(List<ShiftCategoryStructure> shiftCategoryStructureList)
        {
            _shiftCategories = new List<string>();
            _shiftCategoryOnDate = new Dictionary<string, DateOnly>();

            foreach (var shiftCategoryStructure in shiftCategoryStructureList)
            {
                if (!_shiftCategories.Contains(shiftCategoryStructure.ShiftCategoryValue.Description.Name))
                {
                    _shiftCategories.Add(shiftCategoryStructure.ShiftCategoryValue.Description.Name);
                }
                
            }
        }
    }

    public interface IShiftCategoryInformationExtractor
    {
        void ExtractShiftCategoryInformation(List<ShiftCategoryStructure> shiftCategoryStructureList);
        List<String> ShiftCategories { get; }
    }
}
