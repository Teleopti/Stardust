using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCodeTest.AgentPreference;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentStudentAvailability
{
    [TestFixture]
    public class StudentAvailabilityModelTest
    {
        private StudentAvailabilityModel _target;
        private IScheduleHelper _scheduleHelper;
        private static PersonDto _loggedOnPersonDto;

        [SetUp]
        public void Setup()
        {
            CreatPerson();
            _scheduleHelper = new ScheduleHelperFake();
            _target = new StudentAvailabilityModel(_loggedOnPersonDto, _scheduleHelper);
        }

        
        [Test]
        public void ShouldValidateByNotUsingStudentAvailability()
        {
            var date = new DateTime(2010, 05, 27, 0, 0, 0, DateTimeKind.Unspecified);
            var scheduleHelper = MockRepository.GenerateMock<IScheduleHelper>();
            scheduleHelper.Stub(x => x.Validate(_loggedOnPersonDto, new DateOnly(date), true)).Return(new List<ValidatedSchedulePartDto>());
            
            _target.LoadPeriod(date, scheduleHelper);

            scheduleHelper.AssertWasCalled(x => x.Validate(_loggedOnPersonDto, new DateOnly(date), true));
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
            firstPeriod.StartDate.DateTimeSpecified = true;
            firstPeriod.EndDate = firstEnd;
            firstPeriod.EndDate.DateTimeSpecified = true;
            DateOnlyPeriodDto secondPeriod = new DateOnlyPeriodDto();
            secondPeriod.StartDate = secondStart;
            secondPeriod.StartDate.DateTimeSpecified = true;
            secondPeriod.EndDate = secondEnd;
            secondPeriod.EndDate.DateTimeSpecified = true;
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

        #endregion
    }
}
