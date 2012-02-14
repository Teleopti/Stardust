using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class PainterHelperTest
    {
        private PainterHelper _target;

        [Test]
        public void VerifyHasSchedule()
        {
            PreferenceCellData cellDataShift = new PreferenceCellData{HasShift = true};
            PreferenceCellData cellDataAbcence = new PreferenceCellData{HasAbsence = true};
            PreferenceCellData cellDataDayOff = new PreferenceCellData{HasDayOff = true};
            PreferenceCellData cellDataAll = new PreferenceCellData{HasShift = true, HasAbsence = true, HasDayOff = true};
            PreferenceCellData cellDataNoSchedule = new PreferenceCellData();

            _target = new PainterHelper(cellDataShift);
            Assert.IsTrue(_target.HasSchedule());

            _target = new PainterHelper(cellDataAbcence);
            Assert.IsTrue(_target.HasSchedule());

            _target = new PainterHelper(cellDataDayOff);
            Assert.IsTrue(_target.HasSchedule());

            _target = new PainterHelper(cellDataAll);
            Assert.IsTrue(_target.HasSchedule());

            _target = new PainterHelper(cellDataNoSchedule);
            Assert.IsFalse(_target.HasSchedule());
        }

        [Test]
        public void VerifyExtendedPreferenceExists()
        {
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            ITimeLimitationValidator lengthValidator = new TimeLengthValidator();
            PreferenceCellData cellDataWithExtendedPreference = new PreferenceCellData();
            cellDataWithExtendedPreference.Preference = new Preference();
            cellDataWithExtendedPreference.Preference.StartTimeLimitation = new TimeLimitation(validator);
            cellDataWithExtendedPreference.Preference.EndTimeLimitation = new TimeLimitation(validator);
            cellDataWithExtendedPreference.Preference.EndTimeLimitation.MaxTime = new TimeSpan(17, 0, 0);
            cellDataWithExtendedPreference.Preference.WorkTimeLimitation = new TimeLimitation(lengthValidator);

            _target = new PainterHelper(cellDataWithExtendedPreference);
            Assert.IsTrue(_target.HasExtendedPreference());

            PreferenceCellData cellDataWithExtendedPreferenceAndDayOffPreference = new PreferenceCellData();
            cellDataWithExtendedPreferenceAndDayOffPreference.Preference = new Preference();
            cellDataWithExtendedPreferenceAndDayOffPreference.Preference.StartTimeLimitation = new TimeLimitation(validator);
            cellDataWithExtendedPreferenceAndDayOffPreference.Preference.EndTimeLimitation = new TimeLimitation(validator);
            cellDataWithExtendedPreferenceAndDayOffPreference.Preference.EndTimeLimitation.MaxTime = new TimeSpan(17, 0, 0);
            cellDataWithExtendedPreferenceAndDayOffPreference.Preference.WorkTimeLimitation = new TimeLimitation(lengthValidator);
            cellDataWithExtendedPreferenceAndDayOffPreference.Preference.DayOff = new DayOff("Free","DO","xx", Color.DodgerBlue);

            _target = new PainterHelper(cellDataWithExtendedPreferenceAndDayOffPreference);
            Assert.IsFalse(_target.HasExtendedPreference());

            PreferenceCellData cellDataWithoutExtendedPreference = new PreferenceCellData();
            _target = new PainterHelper(cellDataWithoutExtendedPreference);
            Assert.IsFalse(_target.HasExtendedPreference());
        }

        [Test]
        public void VerifyCanPaintActivityPreferencePainter()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintActivityPreference());
            cellData.Preference = new Preference();
            Assert.IsFalse(_target.CanPaintActivityPreference());
            cellData.Preference.Activity = new Activity("xx", "Lunch");
            Assert.IsTrue(_target.CanPaintActivityPreference());
        }

        [Test]
        public void VerifyCanPaintMustHave()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintMustHave());
            cellData.Preference = new Preference();
            Assert.IsFalse(_target.CanPaintMustHave());
            cellData.Preference.MustHave = true;
            Assert.IsTrue(_target.CanPaintMustHave());
        }

        [Test]
        public void VerifyCanPaintAbsence()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintAbsence());
            cellData.HasAbsence = true;
            cellData.HasDayOff = true;
            Assert.IsFalse(_target.CanPaintAbsence());
            cellData.HasDayOff = false;
            Assert.IsTrue(_target.CanPaintAbsence());
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
            _target = new PainterHelper(cellData);
            cellData.EffectiveRestriction = null;
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(false));
            cellData.HasShift = false;
            cellData.HasDayOff = false;
            cellData.HasAbsence = false;
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(false));
            cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.Preference = new Preference();
            cellData.Preference.DayOff = new DayOff("Free", "DO", "xx", Color.Gray);
            cellData.HasShift = true;
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(false));
            cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.HasShift = true;
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            ITimeLimitationValidator lengthValidator = new TimeLengthValidator();
            cellData.EffectiveRestriction = new EffectiveRestriction(new TimeLimitation(validator), new TimeLimitation(validator), new TimeLimitation(lengthValidator));
            Assert.IsTrue(_target.CanPaintEffectiveRestriction(false));
        }

        [Test]
        public void VerifyCanPaintEffectiveRestrictionRightToLeft()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.EffectiveRestriction = null;
            Assert.IsFalse(_target.CanPaintEffectiveRestrictionRightToLeft(true));
            cellData.HasShift = false;
            cellData.HasDayOff = false;
            cellData.HasAbsence = false;
            Assert.IsFalse(_target.CanPaintEffectiveRestrictionRightToLeft(true));
            cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.Preference = new Preference();
            cellData.Preference.DayOff = new DayOff("Free", "DO", "xx", Color.Gray);
            cellData.HasShift = true;
            Assert.IsFalse(_target.CanPaintEffectiveRestrictionRightToLeft(true));
            cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.HasShift = true;
            ITimeLimitationValidator validator = new TimeOfDayValidator(true);
            ITimeLimitationValidator lengthValidator = new TimeLengthValidator();
            cellData.EffectiveRestriction = new EffectiveRestriction(new TimeLimitation(validator), new TimeLimitation(validator), new TimeLimitation(lengthValidator));
            Assert.IsTrue(_target.CanPaintEffectiveRestrictionRightToLeft(true));
        }

        [Test]
        public void VerifyCanPaintPersonalAssignment()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintPersonalAssignment());
            cellData.HasPersonalAssignmentOnly = true;
            Assert.IsTrue(_target.CanPaintPersonalAssignment());
        }

        [Test]
        public void VerifyCanPaintPreferredPaintDayOff()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.HasDayOff = false;
            cellData.HasShift = false;
            cellData.HasAbsence = false;
            cellData.Preference = null;
            Assert.IsFalse(_target.CanPaintPreferredDayOff());
            cellData.Preference = new Preference();
            Assert.IsFalse(_target.CanPaintPreferredDayOff());
            cellData.Preference.DayOff = new DayOff("Free", "DO", "xx", Color.Gray);
            Assert.IsTrue(_target.CanPaintPreferredDayOff());
        }

        [Test]
        public void VerifyCanPaintPreferredPaintAbsence()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.HasDayOff = false;
            cellData.HasShift = false;
            cellData.HasAbsence = false;
            cellData.Preference = null;
            Assert.IsFalse(_target.CanPaintPreferredAbsence());
            cellData.Preference = new Preference();
            Assert.IsFalse(_target.CanPaintPreferredAbsence());
            cellData.Preference.Absence = new Absence("Absence", "AB", "xx", Color.Gray);
            Assert.IsTrue(_target.CanPaintPreferredAbsence());
        }

        [Test]
        public void VerifyCanPaintPreferredExtended()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            cellData.HasDayOff = true;
            cellData.Preference = new Preference();
            Assert.IsFalse(_target.CanPaintPreferredExtended());
            cellData.Preference.StartTimeLimitation = new TimeLimitation(validator);
            cellData.Preference.StartTimeLimitation.MinTime = new TimeSpan(7, 0, 0, 0);
            Assert.IsFalse(_target.CanPaintPreferredExtended());
            cellData.HasShift = false;
            Assert.IsFalse(_target.CanPaintPreferredExtended());
            cellData.HasDayOff = false;
            Assert.IsTrue(_target.CanPaintPreferredExtended());
            cellData.HasAbsence = true;
            Assert.IsFalse(_target.CanPaintPreferredExtended());
            cellData.HasAbsence = false;
            Assert.IsTrue(_target.CanPaintPreferredExtended());
        }

        [Test]
        public void VerifyCanPaintExtendedPreferenceWhenOnlyActivityPreference()
        {
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            ITimeLimitationValidator lengthValidator = new TimeLengthValidator();
            PreferenceCellData cellData = new PreferenceCellData();
            cellData.Preference = new Preference();
            cellData.Preference.StartTimeLimitation = new TimeLimitation(validator);
            cellData.Preference.EndTimeLimitation = new TimeLimitation(validator);
            cellData.Preference.WorkTimeLimitation = new TimeLimitation(lengthValidator);
            cellData.Preference.Activity = new Activity("xx", "Lunch");
            _target = new PainterHelper(cellData);
            Assert.IsTrue(_target.CanPaintPreferredExtended());
        }
        [Test]
        public void VerifyCanPaintExtendedPreferenceWhenActivityPreferenceAndShiftCategory()
        {
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            ITimeLimitationValidator lengthValidator = new TimeLengthValidator();
            PreferenceCellData cellData = new PreferenceCellData();
            cellData.Preference = new Preference();
            cellData.Preference.StartTimeLimitation = new TimeLimitation(validator);
            cellData.Preference.EndTimeLimitation = new TimeLimitation(validator);
            cellData.Preference.WorkTimeLimitation = new TimeLimitation(lengthValidator);
            cellData.Preference.Activity = new Activity("xx", "Lunch");
            cellData.Preference.ShiftCategory = new ShiftCategory("Day", "DY", "xxx", Color.DodgerBlue);
            _target = new PainterHelper(cellData);
            Assert.IsTrue(_target.CanPaintPreferredExtended());
        }

        [Test]
        public void VerifyCanPaintPreferredShiftCategory()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.HasShift = true;
            Assert.IsFalse(_target.CanPaintPreferredShiftCategory());
            cellData.HasShift = false;
            cellData.HasDayOff = true;
            Assert.IsFalse(_target.CanPaintPreferredShiftCategory());
            cellData.HasShift = false;
            cellData.HasDayOff = false;
            cellData.HasAbsence = true;
            Assert.IsFalse(_target.CanPaintPreferredShiftCategory());
            cellData.HasShift = false;
            cellData.HasDayOff = false;
            cellData.HasAbsence = false;
            cellData.Preference = null;
            Assert.IsFalse(_target.CanPaintPreferredShiftCategory());
            cellData.Preference = new Preference();
            cellData.Preference.ShiftCategory = null;
            Assert.IsFalse(_target.CanPaintPreferredShiftCategory());
            cellData.Preference.ShiftCategory = new ShiftCategory("Day", "DY", "xx", Color.DeepSkyBlue);
            Assert.IsTrue(_target.CanPaintPreferredShiftCategory());
        }
        [Test]
        public void VerifyCannotPaintPreferredShiftCategoryWhenActivityPreferenceAndShiftCategory()
        {
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            ITimeLimitationValidator lengthValidator = new TimeLengthValidator();
            PreferenceCellData cellData = new PreferenceCellData();
            cellData.Preference = new Preference();
            cellData.Preference.StartTimeLimitation = new TimeLimitation(validator);
            cellData.Preference.EndTimeLimitation = new TimeLimitation(validator);
            cellData.Preference.WorkTimeLimitation = new TimeLimitation(lengthValidator);
            cellData.Preference.Activity = new Activity("xx", "Lunch");
            cellData.Preference.ShiftCategory = new ShiftCategory("Day", "DY", "xxx", Color.DodgerBlue);
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintPreferredShiftCategory());
        }

        [Test]
        public void VerifyCanPaintScheduledDayOff()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData); ;
            cellData.HasDayOff = false;
            Assert.IsFalse(_target.CanPaintScheduledDayOff());
            cellData.HasDayOff = true;
            Assert.IsTrue(_target.CanPaintScheduledDayOff());
            cellData.HasShift = true;
            Assert.IsTrue(_target.CanPaintScheduledDayOff());
        }

        [Test]
        public void VerifyCanPaintScheduledShift()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.HasShift = false;
            Assert.IsFalse(_target.CanPaintScheduledShift());
            cellData.HasDayOff = true;
            cellData.HasShift = true;
            Assert.IsFalse(_target.CanPaintScheduledShift());
            cellData.HasDayOff = false;
            cellData.HasAbsence = true;
            Assert.IsTrue(_target.CanPaintScheduledShift());
        }

        [Test]
        public void VerifyCanPaintExtendedPreferenceWithTemplate()
        {
            PreferenceCellData cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintExtendedPreferenceTemplate());
            cellData.Preference = new Preference();
            Assert.IsFalse(_target.CanPaintExtendedPreferenceTemplate());
            cellData.Preference.TemplateName = "My template";
            Assert.IsTrue(_target.CanPaintExtendedPreferenceTemplate());
        }

        [Test]
        public void ShouldPaintViolatesNightlyRest()
        {
            var cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            Assert.IsFalse(_target.CanPaintViolatesNightlyRest());
            cellData.ViolatesNightlyRest = true;
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            ITimeLimitationValidator lengthValidator = new TimeLengthValidator();
            cellData.EffectiveRestriction = new EffectiveRestriction(new TimeLimitation(validator), new TimeLimitation(validator), new TimeLimitation(lengthValidator));
            Assert.IsTrue(_target.CanPaintViolatesNightlyRest());
        }

        [Test]
        public void ShouldNotPaintEffectiveRestrictionWhenViolatesNightlyRest()
        {
            var cellData = new PreferenceCellData();
            _target = new PainterHelper(cellData);
            cellData.HasShift = true;
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            ITimeLimitationValidator lengthValidator = new TimeLengthValidator();
            cellData.EffectiveRestriction = new EffectiveRestriction(new TimeLimitation(validator), new TimeLimitation(validator), new TimeLimitation(lengthValidator));
            Assert.IsTrue(_target.CanPaintEffectiveRestriction(false));

            cellData.ViolatesNightlyRest = true;
            Assert.IsFalse(_target.CanPaintEffectiveRestriction(false));
        }
    }
}
