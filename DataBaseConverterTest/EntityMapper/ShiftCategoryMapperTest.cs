using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for ShiftCategoryMapper
    /// </summary>
    [TestFixture]
    public class ShiftCategoryMapperTest : MapperTest<global::Domain.ShiftCategory>
    {
        private ShiftCategoryMapper target;
        private MappedObjectPair mappedObjectPair;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 7; }
        }

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mappedObjectPair = new MappedObjectPair();
            target = new ShiftCategoryMapper(mappedObjectPair);
        }

        /// <summary>
        /// Verifies that this type [can validate number of properties].
        /// </summary>
        [Test]
        public void CanMapOldObjToNewObj()
        {
            string oldName = "Morning";
            Color oldColor = Color.AliceBlue;
            string oldShort = "MO";

            global::Domain.ShiftCategory oldShiftCat = new global::Domain.ShiftCategory(12, oldName,
                                                                                        oldShort,
                                                                                        oldColor,
                                                                                        true,
                                                                                        true, 0);
            IShiftCategory newCat = target.Map(oldShiftCat);
            Assert.AreEqual(oldShiftCat.Name, newCat.Description.Name);
            Assert.AreEqual(oldShiftCat.ColorLayout.ToArgb(), newCat.DisplayColor.ToArgb());
            Assert.AreEqual(oldShort, newCat.Description.ShortName);
        }

        [Test]
        public void CanMapOldObjToNewObjWithOldDeletedShiftCategory()
        {
            string oldName = "Morning";
            Color oldColor = Color.AliceBlue;
            string oldShort = "MO";
            bool inUse = false;

            global::Domain.ShiftCategory oldShiftCat = new global::Domain.ShiftCategory(12, oldName,
                                                                                        oldShort,
                                                                                        oldColor,
                                                                                        true,
                                                                                        inUse, 0);
            IShiftCategory newCat = target.Map(oldShiftCat);
            Assert.AreEqual(oldShiftCat.Name, newCat.Description.Name);
            Assert.AreEqual(oldShiftCat.ColorLayout.ToArgb(), newCat.DisplayColor.ToArgb());
            Assert.AreEqual(oldShort, newCat.Description.ShortName);
            Assert.IsTrue(((IDeleteTag)newCat).IsDeleted);
        }

        [Test]
        public void VerifyMissingNameIsExchangedToDummyValue()
        {
            global::Domain.ShiftCategory oldShiftCat = new global::Domain.ShiftCategory(12, string.Empty,
                                                                            "ffff",
                                                                            Color.DeepSkyBlue,
                                                                            true,
                                                                            true, 0);
            IShiftCategory newCat = target.Map(oldShiftCat);
            Assert.AreEqual("(No name)", newCat.Description.Name);
        }

        [Test]
        public void VerifyJusticeValue()
        {
            global::Domain.ShiftCategory oldShiftCat = new global::Domain.ShiftCategory(12, string.Empty,
                                                                            "ffff",
                                                                            Color.DeepSkyBlue,
                                                                            true,
                                                                            true, 7);
            IShiftCategory newCat = target.Map(oldShiftCat);
            Assert.AreEqual(7, newCat.DayOfWeekJusticeValues[DayOfWeek.Friday]);
        }
    }
}