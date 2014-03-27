using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Absence=Domain.Absence;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for ActivityMapper
    /// </summary>
    [TestFixture]
    public class ActivityMapperTest : MapperTest<global::Domain.Activity>
    {
        private ActivityMapper _target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = new ActivityMapper(new MappedObjectPair());
        }


        /// <summary>
        /// Determines whether this instance [can map activity6x].
        /// </summary>
        /// <remarks>
        /// Roger have set this to ignored until Anders (?) decides column lenghts and stuff. 
        /// String length checks in Payload entity temporarly gone.
        /// </remarks>
        [Test]
        public void CanMapActivity6XAndShrinkActivityName()
        {
            string oldName = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
            Color oldColor = Color.DeepPink;
            bool oldInWorkTime = false;

            global::Domain.Activity oldAct = new global::Domain.Activity(-1, oldName,
                                                                         oldColor, true, true, false, true, oldInWorkTime, true, false, false);
            oldAct.PaidTime = true;
            IActivity newAct = _target.Map(oldAct);
            //do not test dbid
            //name should be truncated to 25 chars
            //TODO: Should be 25 need further impl. in description object!
            Assert.AreEqual(oldName.Substring(0, 50), newAct.Description.Name);
            //color should be color
            Assert.AreEqual(oldColor.ToArgb(), newAct.DisplayColor.ToArgb());
            Assert.AreEqual(oldAct.RequiresSkill, newAct.RequiresSkill);
            
            //changable
            //deleted
            //allowedPreference
            Assert.IsFalse(newAct.InContractTime);
            Assert.IsTrue(newAct.InReadyTime);
            Assert.IsTrue(newAct.InPaidTime);
            Assert.AreEqual(ReportLevelDetail.None, newAct.ReportLevelDetail);
			Assert.IsFalse(newAct.RequiresSeat);
        }

        [Test]
        public void CanHandleDeleted()
        {
            global::Domain.Activity oldAct = new global::Domain.Activity(-1, "q",
                                                                          Color.Beige, true, true, true, true, false, false, false, false);
            IActivity newAct = _target.Map(oldAct);
            Assert.AreEqual(true, newAct.IsDeleted);
        }

        [Test]
        public void VerifyAbsenceActivity()
        {
            string oldName = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
            Color oldColor = Color.DeepPink;
            bool oldInWorkTime = false;

            global::Domain.Activity oldAct = new global::Domain.Activity(-1, oldName,
                                                                         oldColor, true, true, false, true, oldInWorkTime, true, false, false);
            oldAct.PaidTime = true;

            _target.MappedObjectPair.AbsenceActivity.Add(
                new Absence(-1, "test", "t", false, Color.Empty, false, false, false, false, false, oldAct), oldAct);

            IActivity newAct = _target.Map(oldAct);

            Assert.IsNull(newAct);
        }

        [Test]
        public void VerifyActivityIsLunch()
        {
            global::Domain.Activity oldAct = new global::Domain.Activity(-1, "q",
                                                                          Color.Beige, true, true, true, true, false, false, true, false);
            IActivity newAct = _target.Map(oldAct);
            Assert.AreEqual(ReportLevelDetail.Lunch, newAct.ReportLevelDetail);
        }

        [Test]
        public void VerifyActivityIsShortBreak()
        {
            global::Domain.Activity oldAct = new global::Domain.Activity(-1, "q",
                                                                          Color.Beige, true, true, true, true, false, false, false, true);
            IActivity newAct = _target.Map(oldAct);
            Assert.AreEqual(ReportLevelDetail.ShortBreak, newAct.ReportLevelDetail);
        }

        [Test]
        public void VerifyActivityIsLunchWhenBoth()
        {
            global::Domain.Activity oldAct = new global::Domain.Activity(-1, "q",
                                                                          Color.Beige, true, true, true, true, false, false, true, true);
            IActivity newAct = _target.Map(oldAct);
            Assert.AreEqual(ReportLevelDetail.Lunch, newAct.ReportLevelDetail);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 12; }
        }

    }
}