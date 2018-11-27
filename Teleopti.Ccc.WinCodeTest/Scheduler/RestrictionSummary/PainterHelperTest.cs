using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.RestrictionSummary
{
    [TestFixture]
    public class PainterHelperTest
    {
        private PainterHelper _target;

        public static RestrictionSchedulingOptions GetSchedulingOptions(bool useScheduling, bool usePreference, bool useRotation, bool useAvailability, bool useStudentAvailability)
        {
            RestrictionSchedulingOptions schedulingOptions = new RestrictionSchedulingOptions();
            schedulingOptions.UseScheduling = useScheduling;
            schedulingOptions.UseAvailability = useAvailability;
            schedulingOptions.UsePreferences = usePreference;
            schedulingOptions.UseRotations = useRotation;
            schedulingOptions.UseStudentAvailability = useStudentAvailability;
            return schedulingOptions;
        }

        [Test]
        public void VerifyCanPaintActivityPreferencePainter()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            cellData.SchedulingOption = GetSchedulingOptions(false, false, false, false, false);
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintActivityPreference());

            cellData.HasActivityPreference = true;
            cellData.SchedulingOption = GetSchedulingOptions(false, false, false, false, false);
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintActivityPreference());

            cellData.HasActivityPreference = true;
            cellData.SchedulingOption = GetSchedulingOptions(false, true, false, false, false);
            _target = new PainterHelper(cellData);
            Assert.IsTrue(_target.CanPaintActivityPreference());
        }

        [Test]
        public void VerifyCanPaintMustHave()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintMustHave());

            cellData.EffectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                     new WorkTimeLimitation(), null, null, null,
                                                                     new List<IActivityRestriction>());
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintMustHave());

            cellData.MustHavePreference = true;
            _target = new PainterHelper(cellData);
            Assert.IsTrue(_target.CanPaintMustHave());
        }


		[Test]
        public void ShouldPaintPreferredAbsence()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            cellData.SchedulingOption = GetSchedulingOptions(true, true, true, true, true);
            cellData.HasDayOff = true;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintPreferredAbsence());

            cellData.SchedulingOption = GetSchedulingOptions(false, true, true, true, true);
            cellData.HasAbsenceOnContractDayOff = true;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintPreferredAbsence());

            cellData.SchedulingOption = GetSchedulingOptions(false, true, true, true, true);
            cellData.EffectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                     new WorkTimeLimitation(), null, DayOffFactory.CreateDayOff(),AbsenceFactory.CreateAbsence("hej"),
                                                                     new List<IActivityRestriction>());
            cellData.HasAbsenceOnContractDayOff = false;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintPreferredAbsence());

            cellData.SchedulingOption = GetSchedulingOptions(false, true, true, true, true);
            cellData.EffectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                     new WorkTimeLimitation(), null, null, AbsenceFactory.CreateAbsence("hej"),
                                                                     new List<IActivityRestriction>());
            cellData.HasAbsenceOnContractDayOff = false;
            _target = new PainterHelper(cellData);
            Assert.IsTrue(_target.CanPaintPreferredAbsence());
        }


        [Test]
        public void VerifyCanPaintDisabled()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintDisabled());
            cellData.Enabled = false;
            Assert.IsTrue(_target.CanPaintDisabled());
        }

        [Test]
        public void VerifyCanPaintEffectiveRestriction()
        {
            PreferenceCellData cellData = new PreferenceCellData();

            cellData.SchedulingOption = GetSchedulingOptions(true, true, true, true, true);
            cellData.HasFullDayAbsence = true;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(false));

            cellData.SchedulingOption = GetSchedulingOptions(true, true, true, true, true);
            cellData.EffectiveRestriction = null;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(false));

            cellData.HasShift = true;
            cellData.HasDayOff = false;
            cellData.HasAbsence = false;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(false));

            cellData = new PreferenceCellData();
            cellData.SchedulingOption = GetSchedulingOptions(true, true, true, true, true);
            cellData.EffectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                     new WorkTimeLimitation(), null, null, null,
                                                                     new List<IActivityRestriction>());
            _target = new PainterHelper(cellData);
            Assert.IsTrue(_target.CanPaintEffectiveRestriction(false));

            cellData.EffectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                     new WorkTimeLimitation(), null,
                                                                     new DayOffTemplate(new Description("Day Off")), null,
                                                                     new List<IActivityRestriction>());
            cellData.HasShift = true;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(false));
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(true));
        }

        [Test]
        public void VerifyCanPaintEffectiveRestrictionRightToLeft()
        {
            PreferenceCellData cellData = new PreferenceCellData();

            cellData.SchedulingOption = GetSchedulingOptions(true, true, true, true, true);
            cellData.HasFullDayAbsence = true;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintEffectiveRestrictionRightToLeft(true));

            cellData.SchedulingOption = GetSchedulingOptions(true, true, true, true, true);
            cellData.EffectiveRestriction = null;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintEffectiveRestrictionRightToLeft(true));

            cellData.HasShift = true;
            cellData.HasDayOff = false;
            cellData.HasAbsence = false;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintEffectiveRestrictionRightToLeft(true));

            cellData = new PreferenceCellData();
            cellData.SchedulingOption = GetSchedulingOptions(true, true, true, true, true);
            cellData.EffectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                     new WorkTimeLimitation(), null, null, null,
                                                                     new List<IActivityRestriction>());
            _target = new PainterHelper(cellData);
            Assert.IsTrue(_target.CanPaintEffectiveRestrictionRightToLeft(true));

            cellData.EffectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                     new WorkTimeLimitation(), null,
                                                                     new DayOffTemplate(new Description("Day Off")), null,
                                                                     new List<IActivityRestriction>());
            cellData.HasShift = false;
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintEffectiveRestrictionRightToLeft(false));
            Assert.IsTrue(_target.CanPaintEffectiveRestrictionRightToLeft(true));
        }
    }
}