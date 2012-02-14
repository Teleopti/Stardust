using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class ShiftCategorySorterTest
    {
        IList<IShiftCategory> _shiftCategories;

        [SetUp]
        public void Setup()
        {
            _shiftCategories = new List<IShiftCategory>();
        }

        [Test]
        public void VerifySort()
        {
            IShiftCategory shiftCategory1 = new ShiftCategory("a");
            IShiftCategory shiftCategory2 = new ShiftCategory("b");
            IShiftCategory shiftCategory3 = new ShiftCategory("c");

            _shiftCategories.Add(shiftCategory2);
            _shiftCategories.Add(shiftCategory1);
            _shiftCategories.Add(shiftCategory3);

            ((List<IShiftCategory>)_shiftCategories).Sort(new ShiftCategorySorter());

            Assert.AreSame(shiftCategory1, _shiftCategories[0]);
            Assert.AreSame(shiftCategory2, _shiftCategories[1]);
            Assert.AreSame(shiftCategory3, _shiftCategories[2]);

        }
    }
}
