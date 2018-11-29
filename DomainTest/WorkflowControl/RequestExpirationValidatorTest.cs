using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;

using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[DomainTest]
	public class RequestExpirationValidatorTest
	{
		private FakeGlobalSettingDataRepository _globalSettingDataRepository;

		private IScenario _scenario;
		private PersonRequestFactory _personRequestFactory;
		private DateTime _now;
		private DateTime _utcNow;
		private IPerson _person;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void SetUp()
		{
			_globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			_scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			_personRequestFactory = new PersonRequestFactory();

			_person = createPerson(15);
			_timeZone = _person.PermissionInformation.DefaultTimeZone();
			_now = new DateTime(2016, 10, 14, 8, 0, 0);
			_utcNow = _now.Subtract(_timeZone.GetUtcOffset(_now));

			setDefaultStartTimeEndTime(8, 17);
		}

		[Test]
		public void ShouldReturnFalseForExpiredFullDayAbsenceWithScheduleDay()
		{
			var scheduleStartTime = _now.Date.AddHours(8);
			var scheduleEndTime = _now.Date.AddHours(17);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
			, _scenario, createUtcTimePeriod(scheduleStartTime, scheduleEndTime));
			var result = executeValidateFullDayAbsence(assignment);
			result.IsValid.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueForValidFullDayAbsenceWithScheduleDay()
		{
			var scheduleStartTime = _now.Date.AddHours(9);
			var scheduleEndTime = _now.Date.AddHours(17);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person
				, _scenario, createUtcTimePeriod(scheduleStartTime, scheduleEndTime));
			var result = executeValidateFullDayAbsence(assignment);
			result.IsValid.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseForExpiredFullDayAbsenceWithNoScheduleDay()
		{
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(_person, _scenario, new DateOnly(_now));
			var result = executeValidateFullDayAbsence(assignment);
			result.IsValid.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueForValidFullDayAbsenceWithNoScheduleDay()
		{
			setDefaultStartTimeEndTime(9, 17);
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(_person, _scenario, new DateOnly(_now));
			var result = executeValidateFullDayAbsence(assignment);
			result.IsValid.Should().Be(true);
		}

		[Test]
		public void ShouldReturnFalseForExpiredPartDayAbsence()
		{
			var scheduleStartTime = _now.Date.AddHours(8);
			var scheduleEndTime = _now.Date.AddHours(17);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				_scenario, createUtcTimePeriod(scheduleStartTime, scheduleEndTime));
			var result = executeValidatePartDayAbsence(8, 17, assignment);
			result.IsValid.Should().Be(false);
		}

		[Test]
		public void ShouldReturnTrueForValidPartDayAbsence()
		{
			var scheduleStartTime = _now.Date.AddHours(8);
			var scheduleEndTime = _now.Date.AddHours(17);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
				_scenario, createUtcTimePeriod(scheduleStartTime, scheduleEndTime));
			var result = executeValidatePartDayAbsence(9, 17, assignment);
			result.IsValid.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForExpiredFullDayAbsenceWithoutExpiredThresholdSetting()
		{
			_person = createPerson();
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(_person, _scenario, new DateOnly(_now));
			var result = executeValidateFullDayAbsence(assignment);
			result.IsValid.Should().Be(true);
		}

		[Test]
		public void ShouldReturnTrueForExpiredPartDayAbsenceWithoutExpiredThresholdSetting()
		{
			_person = createPerson();
			var assignment = PersonAssignmentFactory.CreatePersonAssignment(_person, _scenario, new DateOnly(_now));
			var result = executeValidatePartDayAbsence(8, 17, assignment);
			result.IsValid.Should().Be(true);
		}

		private DateTimePeriod createUtcTimePeriod(DateTime startTime, DateTime endTime)
		{
			return new DateTimePeriod(TimeZoneHelper.ConvertToUtc(startTime, _timeZone),
				TimeZoneHelper.ConvertToUtc(endTime, _timeZone));
		}

		private IValidatedRequest executeValidateFullDayAbsence(IPersonAssignment assignment)
		{
			var absenceRequest = createFullDayAbsenceRequest(_person);
			return executeValidate(absenceRequest, assignment);
		}

		private IValidatedRequest executeValidatePartDayAbsence(int startHour, int endHour, IPersonAssignment assignment)
		{
			var absenceRequest = createPartDayAbsenceRequest(_person, startHour, endHour);
			return executeValidate(absenceRequest, assignment);
		}

		private IValidatedRequest executeValidate(IAbsenceRequest absenceRequest, IPersonAssignment assignment)
		{
			var schedulingResultStateHolder = createSchedulingResultStateHolder(assignment);
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(schedulingResultStateHolder,null, null, null,
				null);
			return getTarget().Validate(absenceRequest, requiredForHandlingAbsenceRequest);
		}

		private RequestExpirationValidator getTarget()
		{
			var now = new ThisIsNow(_utcNow);
			var target = new RequestExpirationValidator(new ExpiredRequestValidator(_globalSettingDataRepository, now));
			return target;
		}

		private IPerson createPerson(int? absenceRequestExpiredThreshold = null)
		{
			var person = PersonFactory.CreatePersonWithId();
			person.WorkflowControlSet = new WorkflowControlSet
			{
				AbsenceRequestExpiredThreshold = absenceRequestExpiredThreshold
			}.WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			return person;
		}

		private IAbsenceRequest createFullDayAbsenceRequest(IPerson person)
		{
			var requestStartTime = _now.Date;
			var requestEndTime = _now.Date.AddDays(1).AddMinutes(-1);
			var period = createUtcTimePeriod(requestStartTime, requestEndTime);
			return createAbsenceRequest(period, person);
		}

		private IAbsenceRequest createPartDayAbsenceRequest(IPerson person, int startHour, int endHour)
		{
			var requestStartTime = _now.Date.AddHours(startHour);
			var requestEndTime = _now.Date.AddHours(endHour);
			var period = createUtcTimePeriod(requestStartTime, requestEndTime);
			return createAbsenceRequest(period, person);
		}

		private IAbsenceRequest createAbsenceRequest(DateTimePeriod period, IPerson person)
		{
			var absence = AbsenceFactory.CreateAbsenceWithId();
			var absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, period);
			var request = _personRequestFactory.CreatePersonRequest(person);
			absenceRequest.SetParent(request);
			return absenceRequest;
		}

		private ISchedulingResultStateHolder createSchedulingResultStateHolder(IPersonAssignment assignment)
		{
			var schedulingResultStateHolder = new SchedulingResultStateHolder();
			schedulingResultStateHolder.Schedules = ScheduleDictionaryForTest.WithPersonAssignment(_scenario,
				assignment.Period.StartDateTime,
				assignment);
			return schedulingResultStateHolder;
		}

		private void setDefaultStartTimeEndTime(int startHour, int endHour)
		{
			_globalSettingDataRepository.PersistSettingValue("FullDayAbsenceRequestStartTime",
				new TimeSpanSetting(TimeSpan.FromHours(startHour)));

			_globalSettingDataRepository.PersistSettingValue("FullDayAbsenceRequestEndTime",
				new TimeSpanSetting(TimeSpan.FromHours(endHour)));
		}
	}
}
