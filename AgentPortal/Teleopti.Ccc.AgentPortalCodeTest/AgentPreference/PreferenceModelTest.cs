using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;
using System.Drawing;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class PreferenceModelTest
    {
        private PreferenceModel _target;
        private IScheduleHelper _scheduleHelper;
        private static PersonDto _loggedOnPersonDto;

        [SetUp]
        public void Setup()
        {
            CreatPerson();
            _scheduleHelper = new ScheduleHelperFake();
            _target = new PreferenceModel(_loggedOnPersonDto, _scheduleHelper);
        }

        [Test]
        public void CanCreateInstanceAndInitialPropertiesAreSet()
        {
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.CellDataCollection.Count == 0);
            Assert.IsNotNull(_target.CurrentCultureInfo());
            Assert.IsNotNull(_target.CurrentUICultureInfo());
        }

        [Test]
        public void CanGetLoggedOnPerson()
        {
            Assert.IsNotNull(_target.LoggedOnPerson);
        }

        [Test]
        public void VerifyCanGetProperties()
        {
            Assert.IsNotNull(_target.CellDataCollection);
            Assert.IsNotNull(_target.ShiftCategories);
            Assert.IsNotNull(_target.DaysOff);
            Assert.IsNotNull(_target.FirstDateCurrentPeriod);
            Assert.IsTrue(_target.NumberOfMustHaveCurrentPeriod == 0);
            Assert.IsTrue(_target.MaxMustHaveCurrentPeriod == 0);
        }

        [Test]
        public void VerifyLoadPeriod()
        {
            ITimeLimitationValidator timeLimitationValidator = new TimeOfDayValidator(false);
            ITimeLimitationValidator lengthLimitationValidator = new TimeLengthValidator();
            TimeLimitationDto startMinTime = new TimeLimitationDto { MinTime = TimeSpan.FromHours(6) };
            TimeLimitationDto endMaxTime = new TimeLimitationDto { MaxTime = TimeSpan.FromHours(8) };
            TimeLimitationDto workMaxTime = new TimeLimitationDto { MaxTime = TimeSpan.FromHours(1) };
            TimeLimitation startTimeLimitation = new TimeLimitation(timeLimitationValidator, startMinTime);
            TimeLimitation endTimeLimitation = new TimeLimitation(timeLimitationValidator, endMaxTime);
            TimeLimitation workTimeLimitation = new TimeLimitation(lengthLimitationValidator, workMaxTime);

            DateOnlyDto dateOnlyDto1;
            DateOnlyDto dateOnlyDto2;
            DateOnlyDto dateOnlyDto3;
            DateOnlyDto dateOnlyDto4;

            ((ScheduleHelperFake)_scheduleHelper).ValidatedSchedulePartDto = ValidatedParts(out dateOnlyDto1, out dateOnlyDto2, out dateOnlyDto3, out dateOnlyDto4);

            _target.LoadPeriod(new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified), _scheduleHelper);
            Assert.AreEqual(4, _target.CellDataCollection.Count);
            Assert.AreEqual(Color.FromArgb(255, 0, 128, 192), _target.CellDataCollection[1].Preference.ShiftCategory.DisplayColor);
            Assert.AreEqual(dateOnlyDto1.DateTime, _target.CellDataCollection[0].TheDate);
            Assert.AreEqual(dateOnlyDto2.DateTime, _target.CellDataCollection[1].TheDate);
            Assert.IsNotNull(_target.CellDataCollection[0].Preference.DayOff);
            Assert.IsNull(_target.CellDataCollection[1].Preference.DayOff);
            Assert.IsTrue(_target.CellDataCollection[0].Legal);
            Assert.IsFalse(_target.CellDataCollection[0].IsInsidePeriod);
            Assert.IsTrue(_target.CellDataCollection[1].IsInsidePeriod);
            Assert.AreEqual(2400, _target.CellDataCollection[0].WeeklyMax.TotalMinutes);
            Assert.AreEqual(9600, _target.CellDataCollection[0].PeriodTarget.TotalMinutes);
            Assert.IsFalse(_target.CellDataCollection[0].Enabled);
            Assert.IsFalse(_target.CellDataCollection[0].HasAbsence);
            Assert.AreEqual(0, _target.CellDataCollection[0].MaxMustHave);
            Assert.AreEqual(3, _target.CellDataCollection[1].MaxMustHave);
            Assert.AreEqual(dateOnlyDto1.DateTime, _target.CellDataCollection[0].TheDate);
            Assert.AreEqual(dateOnlyDto2.DateTime, _target.CellDataCollection[1].TheDate);
            Assert.AreEqual(dateOnlyDto3.DateTime, _target.CellDataCollection[2].TheDate);
            Assert.AreEqual(dateOnlyDto4.DateTime, _target.CellDataCollection[3].TheDate);
            Assert.IsFalse(_target.CellDataCollection[0].HasPersonalAssignmentOnly);
            Assert.IsFalse(_target.CellDataCollection[1].HasPersonalAssignmentOnly);
            Assert.IsTrue(_target.CellDataCollection[2].HasPersonalAssignmentOnly);
            Assert.IsNull(_target.CellDataCollection[0].TipText);
            Assert.IsNull(_target.CellDataCollection[1].TipText);
            Assert.IsNull(_target.CellDataCollection[3].TipText);
            Assert.AreEqual("TipText", _target.CellDataCollection[2].TipText);
            Assert.IsNull(_target.CellDataCollection[0].Preference.Activity);
            Assert.IsNotNull(_target.CellDataCollection[1].Preference.Activity);
            Assert.IsNotNull(_target.CellDataCollection[2].Preference.Activity);
            Assert.IsNotNull(_target.CellDataCollection[3].Preference.Activity);
            Assert.AreEqual(startTimeLimitation.MinTime, _target.CellDataCollection[1].Preference.ActivityStartTimeLimitation.MinTime);
            Assert.AreEqual(endTimeLimitation.MaxTime, _target.CellDataCollection[1].Preference.ActivityEndTimeLimitation.MaxTime);
            Assert.AreEqual(workTimeLimitation.MaxTime, _target.CellDataCollection[1].Preference.ActivityTimeLimitation.MinTime);
            Assert.AreEqual(new TimeLimitation(timeLimitationValidator).MaxTime, _target.CellDataCollection[1].Preference.ActivityTimeLimitation.MaxTime);
            Assert.IsTrue(_target.CellDataCollection[0].IsWorkday);
            Assert.IsTrue(_target.CellDataCollection[1].IsWorkday);
            Assert.IsTrue(_target.CellDataCollection[2].IsWorkday);
            Assert.IsTrue(_target.CellDataCollection[3].IsWorkday);
            Assert.IsTrue(_target.CellDataCollection[0].ViolatesNightlyRest);
            Assert.IsTrue(_target.CellDataCollection[1].ViolatesNightlyRest);
            Assert.IsTrue(_target.CellDataCollection[2].ViolatesNightlyRest);
            Assert.IsTrue(_target.CellDataCollection[3].ViolatesNightlyRest);
        }

        [Test]
        public void VerifyAddMustHaveIncreasesNumberOfMustHaveCurrentPeriodByOne()
        {
            int orgNumberOfMustHave = _target.MaxMustHaveCurrentPeriod;
            _target.AddMustHave();
            Assert.IsTrue(_target.NumberOfMustHaveCurrentPeriod == orgNumberOfMustHave + 1);
        }
        [Test]
        public void VerifyRemoveMustHaveDecreasesNumberOfMustHaveCurrentPeriodByOne()
        {
            int orgNumberOfMustHave = _target.MaxMustHaveCurrentPeriod;
            _target.RemoveMustHave();
            Assert.IsTrue(_target.NumberOfMustHaveCurrentPeriod == orgNumberOfMustHave - 1);
        }

        [Test]
        public void VerifyCanReturnPeriodIsValidState()
        {
            Assert.IsTrue(_target.PeriodIsValid());
            DateOnlyDto dateOnlyDto = new DateOnlyDto();
            dateOnlyDto.DateTime = new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified);
            ValidatedSchedulePartDto part = CreatValidatedSchedulePartDto(dateOnlyDto, false, false, false,
                                                                                  false, true, true, true, true);
            ((ScheduleHelperFake)_scheduleHelper).ValidatedSchedulePartDto = new List<ValidatedSchedulePartDto> { part };
            _target.LoadPeriod(dateOnlyDto.DateTime, _scheduleHelper);
            _target.CellDataCollection[0].EffectiveRestriction = new EffectiveRestriction();

            //When Invalid EffectiveRestriction
            Assert.IsFalse(_target.PeriodIsValid());

        }

        [Test]
        public void VerifyCurrentTime()
        {
            DateOnlyDto dateOnlyDto1;
            DateOnlyDto dateOnlyDto2;
            DateOnlyDto dateOnlyDto3;
            DateOnlyDto dateOnlyDto4;

            ((ScheduleHelperFake)_scheduleHelper).ValidatedSchedulePartDto = ValidatedParts(out dateOnlyDto1, out dateOnlyDto2, out dateOnlyDto3, out dateOnlyDto4);

            _target.LoadPeriod(new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified), _scheduleHelper);
            TimePeriod expectedPeriod = new TimePeriod(24, 0, 24, 0);
            Assert.AreEqual(expectedPeriod.StartTime, _target.CurrentPeriodTime().StartTime);
            Assert.AreEqual(expectedPeriod.EndTime, _target.CurrentPeriodTime().EndTime);
        }
        [Test]
        public void VerifyPeriodTargetTime()
        {
            DateOnlyDto dateOnlyDto1;
            DateOnlyDto dateOnlyDto2;
            DateOnlyDto dateOnlyDto3;
            DateOnlyDto dateOnlyDto4;

            ((ScheduleHelperFake)_scheduleHelper).ValidatedSchedulePartDto = ValidatedParts(out dateOnlyDto1, out dateOnlyDto2, out dateOnlyDto3, out dateOnlyDto4);

            Assert.IsTrue(_target.CellDataCollection.Count == 0);
            Assert.AreEqual(TimeSpan.FromMinutes(0), _target.PeriodTargetTime());
            _target.LoadPeriod(new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified), _scheduleHelper);
            Assert.AreEqual(TimeSpan.FromMinutes(9600), _target.PeriodTargetTime());
        }

        [Test]
        public void VerifyGetDateOnlyInPeriodReturnsLastDateOfLastPeriodIfAppliedDateIsAfterTerminalDate()
        {
            DateTime dateTimeOutsidePeriod = new DateTime(2010, 12, 31);
            DateOnly expectedDate = new DateOnly(
                _loggedOnPersonDto.PersonPeriodCollection.Last().Period.
                    EndDate.DateTime);
            DateOnly dateOnly = _target.GetDateOnlyInPeriod(dateTimeOutsidePeriod);
            Assert.AreEqual(expectedDate, dateOnly);
        }

        [Test]
        public void VerifyGetDateOnlyInPeriodReturnsSecondDateOfLastPeriodIfAppliedDateIsBeforeFirstPeriod()
        {
            DateTime dateTimeOutsidePeriod = new DateTime(2007, 12, 31);
            DateOnly expectedDate = new DateOnly(
                _loggedOnPersonDto.PersonPeriodCollection.Last().Period.
                    StartDate.DateTime.AddDays(1));
            DateOnly dateOnly = _target.GetDateOnlyInPeriod(dateTimeOutsidePeriod);
            Assert.AreEqual(expectedDate, dateOnly);
        }

        [Test]
        public void VerifyGetDateOnlyInPeriodReturnsSameDateAsAppliedWhenInsidePeriod()
        {
            DateTime dateTimeInPeriod = new DateTime(2010, 03, 01);
            DateOnly expectedDate = new DateOnly(dateTimeInPeriod);

            DateOnly dateOnly = _target.GetDateOnlyInPeriod(dateTimeInPeriod);
            Assert.AreEqual(expectedDate, dateOnly);
        }

        [Test]
        public void ShouldLoadAbsencePreference()
        {
            var dateOnlyDto = new DateOnlyDto();
            dateOnlyDto.DateTime = new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified);
            ValidatedSchedulePartDto absenceDto = CreateValidatedSchedulePartDtoAbsencePreference(dateOnlyDto);

            ((ScheduleHelperFake)_scheduleHelper).ValidatedSchedulePartDto = new List<ValidatedSchedulePartDto> {absenceDto};

            _target.LoadPeriod(new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified), _scheduleHelper);

            Assert.AreEqual(1, _target.CellDataCollection.Count);
            Assert.AreEqual(dateOnlyDto.DateTime, _target.CellDataCollection[0].TheDate);
            Assert.IsNotNull(_target.CellDataCollection[0].Preference.Absence); 
        }

        [Test]
        public void ShouldValidateByNotUsingStudentAvailability()
        {
            var date = new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified);
            var scheduleHelper = MockRepository.GenerateMock<IScheduleHelper>();
            scheduleHelper.Stub(x => x.Validate(_loggedOnPersonDto, new DateOnly(date), false)).Return(new List<ValidatedSchedulePartDto>());
            
            _target.LoadPeriod(date, scheduleHelper);

            scheduleHelper.AssertWasCalled(x=> x.Validate(_loggedOnPersonDto, new DateOnly(date), false));
        }

        #region Setup

        private static void CreatPerson()
        {
            DateOnlyDto firstStart = new DateOnlyDto();
            DateOnlyDto firstEnd = new DateOnlyDto();
            firstStart.DateTime = new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Local);
            firstEnd.DateTime = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Local);
            DateOnlyDto secondStart = new DateOnlyDto();
            DateOnlyDto secondEnd = new DateOnlyDto();
            secondStart.DateTime = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Local);
            secondEnd.DateTime = new DateTime(2010, 7, 1, 0, 0, 0, DateTimeKind.Local);
            DateOnlyPeriodDto firstPeriod = new DateOnlyPeriodDto();
            firstPeriod.StartDate = firstStart;
            firstPeriod.EndDate = firstEnd;
            DateOnlyPeriodDto secondPeriod = new DateOnlyPeriodDto();
            secondPeriod.StartDate = secondStart;
            secondPeriod.EndDate = secondEnd;
            _loggedOnPersonDto = new PersonDto();
            _loggedOnPersonDto.UICultureLanguageId = 1053;
            _loggedOnPersonDto.CultureLanguageId = 1053;
            _loggedOnPersonDto.TimeZoneId = "W. Europe Standard Time";
            PersonPeriodDto firstPersonPeriodDto = new PersonPeriodDto();
            firstPersonPeriodDto.Period = firstPeriod;
            PersonPeriodDto secondPersonPeriodDto = new PersonPeriodDto();
            secondPersonPeriodDto.Period = secondPeriod;
            _loggedOnPersonDto.PersonPeriodCollection = new PersonPeriodDto[2];
            _loggedOnPersonDto.PersonPeriodCollection[0] = firstPersonPeriodDto;
            _loggedOnPersonDto.PersonPeriodCollection[1] = secondPersonPeriodDto;

        }

        private static ValidatedSchedulePartDto CreateValidatedSchedulePartDtoAbsencePreference(DateOnlyDto dateOnly)
        {
            var validatedSchedulePartDto = new ValidatedSchedulePartDto();
            validatedSchedulePartDto.DateOnly = dateOnly;
            validatedSchedulePartDto.HasAbsence = true;
            validatedSchedulePartDto.HasDayOff = false;
            validatedSchedulePartDto.HasPersonalAssignmentOnly = false;
            validatedSchedulePartDto.HasShift = false;
            validatedSchedulePartDto.IsPreferenceEditable = false;
            validatedSchedulePartDto.IsInsidePeriod = true;
            validatedSchedulePartDto.LegalState = true;
            validatedSchedulePartDto.MinStartTimeMinute = 420;
            validatedSchedulePartDto.MaxStartTimeMinute = 480;
            validatedSchedulePartDto.MinEndTimeMinute = 1020;
            validatedSchedulePartDto.MaxEndTimeMinute = 1020;
            validatedSchedulePartDto.MinWorkTimeInMinutes = 480;
            validatedSchedulePartDto.MaxWorkTimeInMinutes = 480;
            validatedSchedulePartDto.WeekMaxInMinutes = 2400;
            validatedSchedulePartDto.PeriodTargetInMinutes = 9600;
            validatedSchedulePartDto.MustHave = 3;
            validatedSchedulePartDto.PreferenceRestriction = new PreferenceRestrictionDto();
            validatedSchedulePartDto.PreferenceRestriction.RestrictionDate = dateOnly;

            validatedSchedulePartDto.PreferenceRestriction.Absence = new AbsenceDto();
            validatedSchedulePartDto.PreferenceRestriction.Absence.Id = Guid.NewGuid();
            validatedSchedulePartDto.PreferenceRestriction.Absence.Name = "name";
            validatedSchedulePartDto.PreferenceRestriction.Absence.ShortName = "shortName";
            validatedSchedulePartDto.PreferenceRestriction.Absence.DisplayColor = new ColorDto(Color.DodgerBlue);
            validatedSchedulePartDto.PreferenceRestriction.Absence.DisplayColor.Alpha = 255;
            validatedSchedulePartDto.PreferenceRestriction.Absence.DisplayColor.Red = 0;
            validatedSchedulePartDto.PreferenceRestriction.Absence.DisplayColor.Green = 128;
            validatedSchedulePartDto.PreferenceRestriction.Absence.DisplayColor.Blue = 192;

            validatedSchedulePartDto.PreferenceRestriction.StartTimeLimitation = new TimeLimitationDto();
            validatedSchedulePartDto.PreferenceRestriction.EndTimeLimitation = new TimeLimitationDto();
            validatedSchedulePartDto.PreferenceRestriction.WorkTimeLimitation = new TimeLimitationDto();
            validatedSchedulePartDto.PreferenceRestriction.MustHave = true;
            validatedSchedulePartDto.ScheduledItemName = "ScheduledItemName";
            validatedSchedulePartDto.ScheduledItemShortName = "XX";
            validatedSchedulePartDto.TipText = "TipText";
            validatedSchedulePartDto.DisplayColor = new ColorDto(Color.DodgerBlue);

            return validatedSchedulePartDto;
        }

        private static ValidatedSchedulePartDto CreatValidatedSchedulePartDto(DateOnlyDto dateOnly, bool hasAbscence, bool hasDayOff, bool hasPersonalAssignmentOnly,
            bool hasShift, bool editable, bool insidePeriod, bool legalState, bool dayOffPreference)
        {
            ValidatedSchedulePartDto validatedSchedulePartDto = new ValidatedSchedulePartDto();
            validatedSchedulePartDto.DateOnly = dateOnly;
            validatedSchedulePartDto.HasAbsence = hasAbscence;
            validatedSchedulePartDto.HasDayOff = hasDayOff;
            validatedSchedulePartDto.HasPersonalAssignmentOnly = hasPersonalAssignmentOnly;
            validatedSchedulePartDto.HasShift = hasShift;
            validatedSchedulePartDto.IsPreferenceEditable = editable;
            validatedSchedulePartDto.IsInsidePeriod = insidePeriod;
            validatedSchedulePartDto.LegalState = legalState;
            validatedSchedulePartDto.MinStartTimeMinute = 420;
            validatedSchedulePartDto.MaxStartTimeMinute = 480;
            validatedSchedulePartDto.MinEndTimeMinute = 1020;
            validatedSchedulePartDto.MaxEndTimeMinute = 1020;
            validatedSchedulePartDto.MinWorkTimeInMinutes = 480;
            validatedSchedulePartDto.MaxWorkTimeInMinutes = 480;
	        validatedSchedulePartDto.MinContractTimeInMinutes = 480;
	        validatedSchedulePartDto.MaxContractTimeInMinutes = 480;
            validatedSchedulePartDto.WeekMaxInMinutes = 2400;
            validatedSchedulePartDto.PeriodTargetInMinutes = 9600;
            validatedSchedulePartDto.MustHave = 3;
            validatedSchedulePartDto.PreferenceRestriction = new PreferenceRestrictionDto();
            validatedSchedulePartDto.PreferenceRestriction.RestrictionDate = dateOnly;
            if (!dayOffPreference)
            {
                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory = new ShiftCategoryDto();
                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.Name = "Day";
                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.Id = Guid.NewGuid();
                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.DisplayColor = new ColorDto(Color.DodgerBlue);
                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.DisplayColor.Alpha = 255;
                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.DisplayColor.Red = 0;
                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.DisplayColor.Green = 128;
                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.DisplayColor.Blue = 192;
                ActivityRestrictionDto activityRestrictionDto = new ActivityRestrictionDto();
                activityRestrictionDto.Activity = new ActivityDto();
                activityRestrictionDto.Activity.Description = "Lunch";
                activityRestrictionDto.Activity.Id = Guid.NewGuid();
                activityRestrictionDto.StartTimeLimitation = new TimeLimitationDto { MinTime = TimeSpan.FromHours(6) };
                activityRestrictionDto.EndTimeLimitation = new TimeLimitationDto { MaxTime = TimeSpan.FromHours(8) };
                activityRestrictionDto.WorkTimeLimitation = new TimeLimitationDto { MinTime = TimeSpan.FromHours(1) };
                validatedSchedulePartDto.PreferenceRestriction.ActivityRestrictionCollection.Add(activityRestrictionDto);
            }
            else
            {
                validatedSchedulePartDto.PreferenceRestriction.DayOff = new DayOffInfoDto();
                validatedSchedulePartDto.PreferenceRestriction.DayOff.Id = Guid.NewGuid();
                validatedSchedulePartDto.PreferenceRestriction.DayOff.Name = "DayOff";
                validatedSchedulePartDto.PreferenceRestriction.DayOff.ShortName = "DO";
            }
            validatedSchedulePartDto.PreferenceRestriction.StartTimeLimitation = new TimeLimitationDto();
            validatedSchedulePartDto.PreferenceRestriction.EndTimeLimitation = new TimeLimitationDto();
            validatedSchedulePartDto.PreferenceRestriction.WorkTimeLimitation = new TimeLimitationDto();
            validatedSchedulePartDto.PreferenceRestriction.MustHave = true;
            validatedSchedulePartDto.ScheduledItemName = "ScheduledItemName";
            validatedSchedulePartDto.ScheduledItemShortName = "XX";
            validatedSchedulePartDto.TipText = "TipText";
            validatedSchedulePartDto.DisplayColor = new ColorDto(Color.DodgerBlue);
            validatedSchedulePartDto.IsWorkday = true;
            validatedSchedulePartDto.ViolatesNightlyRest = true;

            return validatedSchedulePartDto;
        }

        private static IList<ValidatedSchedulePartDto> ValidatedParts(out DateOnlyDto dateOnlyDto1, out DateOnlyDto dateOnlyDto2, out DateOnlyDto dateOnlyDto3, out DateOnlyDto dateOnlyDto4)
        {
            dateOnlyDto1 = new DateOnlyDto();
            dateOnlyDto1.DateTime = new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified);
            ValidatedSchedulePartDto dayOffPartDto = CreatValidatedSchedulePartDto(dateOnlyDto1, false, true, false,
                                                                                   false, false, false, true, true);
            dateOnlyDto2 = new DateOnlyDto();
            dateOnlyDto2.DateTime = new DateTime(2010, 05, 28, 0, 0, 0, DateTimeKind.Unspecified);
            ValidatedSchedulePartDto shiftPartDto = CreatValidatedSchedulePartDto(dateOnlyDto2, false, false, false,
                                                                                  true, true, true, true, false);
            dateOnlyDto3 = new DateOnlyDto();
            dateOnlyDto3.DateTime = new DateTime(2010, 05, 29, 0, 0, 0, DateTimeKind.Unspecified);
            ValidatedSchedulePartDto personalOnlyPartDto = CreatValidatedSchedulePartDto(dateOnlyDto3, false, false, true,
                                                                                         false, true, true, true, false);
            dateOnlyDto4 = new DateOnlyDto();
            dateOnlyDto4.DateTime = new DateTime(2010, 05, 30, 0, 0, 0, DateTimeKind.Unspecified);
            ValidatedSchedulePartDto noSchedulePartDto = CreatValidatedSchedulePartDto(dateOnlyDto4, false, false, false,
                                                                                       false, true, true, true, false);

            return new List<ValidatedSchedulePartDto> { dayOffPartDto, shiftPartDto, personalOnlyPartDto, noSchedulePartDto };
        }
        #endregion
    }

    internal class ScheduleHelperFake : IScheduleHelper
    {
        public ICollection<ValidatedSchedulePartDto> Validate(PersonDto loggedOnPerson, DateOnly dateInPeriod, bool useStudentAvailability)
        {
            return ValidatedSchedulePartDto;
        }

        public IList<ValidatedSchedulePartDto> ValidatedSchedulePartDto { get; set; }
    }
}
