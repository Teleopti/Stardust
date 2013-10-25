using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for AbsenceMapper
    /// </summary>
    [TestFixture]
    public class AbsenceMapperTest : MapperTest<global::Domain.Absence>
    {
        private AbsenceMapper target;
        private IList<DataRow> confidentials;

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
            confidentials = new List<DataRow>();
            target = new AbsenceMapper(new MappedObjectPair(), confidentials);
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
                                                                       oldColor, true, false, false, oldInWorkTime, true, null);
            oldAbs.PaidTime = true;

            IAbsence newAbsence = target.Map(oldAbs);


            Assert.AreEqual(oldAbs.Name, newAbsence.Description.Name);
            Assert.AreEqual(oldAbs.ShortName, newAbsence.Description.ShortName);
            Assert.AreEqual(oldAbs.LayoutColor.ToArgb(), newAbsence.DisplayColor.ToArgb());
            Assert.IsTrue(newAbsence.InContractTime);
            Assert.IsTrue(newAbsence.Requestable);
            Assert.IsTrue(newAbsence.InPaidTime);
            Assert.IsTrue(newAbsence.InWorkTime);
        }

        /// <summary>
        /// Determines whether this instance [can shrink name].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2007-10-19
        /// </remarks>
        [Test]
        public void CanShrinkName()
        {
            string oldShortName = "SE";
            string oldName = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
            Color oldColor = Color.Red;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, false, false, true, true, null);

            IAbsence newAbsence = target.Map(oldAbs);
            //TODO: Should be 25 need further impl. in description object!
            Assert.AreEqual(50, newAbsence.Description.Name.Length);
        }

        /// <summary>
        /// Determines whether this instance [can handle empty name].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2007-11-06
        /// </remarks>
        [Test]
        public void CanHandleEmptyName()
        {
            string oldShortName = "qq";
            string oldName = "";
            Color oldColor = Color.Red;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, false, false, true, true, null);

            IAbsence newAbsence = target.Map(oldAbs);
            Assert.Greater(newAbsence.Description.Name.Length, 0);
        }

        /// <summary>
        /// Determines whether this instance [can handle null name].
        /// </summary>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2007-11-06
        /// </remarks>
        [Test]
        public void CanHandleNullName()
        {
            string oldShortName = "qq";
            string oldName = null;
            Color oldColor = Color.Red;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, false, false, true, true, null);

            IAbsence newAbsence = target.Map(oldAbs);
            Assert.Greater(newAbsence.Description.Name.Length, 0);
        }

        [Test]
        public void DoNotConvertWhenCountRulesIsTrue()
        {
            string oldShortName = "qq";
            string oldName = null;
            Color oldColor = Color.Red;

            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, oldName, oldShortName, true,
                                                                       oldColor, true, true, false, true, true, null);

            IAbsence newAbsence = target.Map(oldAbs);
            Assert.IsNull(newAbsence);
        }

        [Test]
        public void CanHandleDeleted()
        {
            global::Domain.Absence oldAbs = new global::Domain.Absence(-1, "", "", true,
                                                                       Color.Firebrick, true, false, true, false, true, null);
            IAbsence newAbsence = target.Map(oldAbs);
            Assert.AreEqual(true, ((IDeleteTag)newAbsence).IsDeleted);
        }

        [Test]
        public void CanHandleConfidential()
        {
            createDataRow(1);
            global::Domain.Absence oldAbs = new global::Domain.Absence(1, "", "", true,
                                                           Color.Firebrick, true, false, true, false, true, null);
            IAbsence newAbsence = target.Map(oldAbs);
            Assert.IsTrue(newAbsence.Confidential);
        }

        private void createDataRow(int id)
        {
            DataTable t2 = new DataTable("table");
            t2.Locale = CultureInfo.CurrentCulture;
            t2.Columns.Add("abs_id", typeof(int));
            t2.Columns.Add("private_desc", typeof(string));
            DataRow row = t2.NewRow();
            row["abs_id"] = id;
            row["private_desc"] = "banan";
            confidentials.Add(row);
        }
    }
}