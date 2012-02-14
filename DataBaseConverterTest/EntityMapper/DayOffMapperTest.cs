using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for DayOffMapper
    /// </summary>
    [TestFixture]
    public class DayOffMapperTest : MapperTest<global::Domain.Absence>
    {
        private DayOffMapper target;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 12; }
        }

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new DayOffMapper(new MappedObjectPair());
        }

        /// <summary>
        /// Verifies that a correct mapping between 6x object and Raptor object i performed
        /// </summary>
        [Test]
        public void CanMapAbsence6X()
        {
            string oldShortName = "SE";
            string oldName = "Semester";
            Color oldColor = Color.Red;
            bool oldInWorkTime = true;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, true, false, oldInWorkTime, true, null);
            oldAbs.PaidTime = true;

            IDayOffTemplate newDayOff = target.Map(oldAbs);

            Assert.AreEqual(oldAbs.Name, newDayOff.Description.Name);
            Assert.AreEqual(oldAbs.ShortName, newDayOff.Description.ShortName);
        }

        /// <summary>
        /// Determines whether this instance [can shrink name].
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        [Test]
        public void CanShrinkName()
        {
            string oldShortName = "SE";
            string oldName = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
            Color oldColor = Color.Red;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, true, false, true, true, null);

            IDayOffTemplate newDayOff = target.Map(oldAbs);
            //TODO: Should be 25 need further impl. in description object!
            Assert.AreEqual(50, newDayOff.Description.Name.Length);
        }

        /// <summary>
        /// Determines whether this instance [can handle empty name].
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        [Test]
        public void CanHandleEmptyName()
        {
            string oldShortName = "qq";
            string oldName = "";
            Color oldColor = Color.Red;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, true, false, true, true, null);

            IDayOffTemplate newDayOff = target.Map(oldAbs);
            Assert.Greater(newDayOff.Description.Name.Length, 0);
        }

        /// <summary>
        /// Determines whether this instance [can handle null name].
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        [Test]
        public void CanHandleNullName()
        {
            string oldShortName = "qq";
            string oldName = null;
            Color oldColor = Color.Red;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, true, false, true, true, null);

            IDayOffTemplate newDayOff = target.Map(oldAbs);
            Assert.Greater(newDayOff.Description.Name.Length, 0);
        }

        /// <summary>
        /// Does the not convert when count rules isfalse.
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        [Test]
        public void DoNotConvertWhenCountRulesIsFalse()
        {
            string oldShortName = "qq";
            string oldName = null;
            Color oldColor = Color.Red;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, false, false, true, true, null);

            IDayOffTemplate newDayOff = target.Map(oldAbs);
            Assert.IsNull(newDayOff);
        }

        /// <summary>
        /// Determines whether this instance [can handle deleted].
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-11-26
        /// </remarks>
        [Test]
        public void CanHandleDeleted()
        {
            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, "", "", true,
                                                                       Color.Firebrick, true, true, true, false, true, null);
            IDayOffTemplate newDayOff = target.Map(oldAbs);
            Assert.AreEqual(true, ((IDeleteTag)newDayOff).IsDeleted);
        }

    }
}