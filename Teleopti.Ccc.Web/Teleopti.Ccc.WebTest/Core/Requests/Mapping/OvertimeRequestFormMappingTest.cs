using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class OvertimeRequestFormMappingTest
	{
		private const string subject = "Overtime request";
		private const string message = "I want work overtime!";
		private const int startTimeInMinute = 75;
		private const int endTimeInMinute = 135;

		private ILoggedOnUser _loggedOnUser;
		private Person _person;
		private IUserTimeZone _userTimeZone;
		private MultiplicatorDefinitionSet _multiplicatorDefinitionSet;
		private IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private OvertimeRequestFormMapper target;

		private ITextFormatter _formatter;

		[SetUp]
		public void Setup()
		{
			_formatter = new NoFormatting();

			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_userTimeZone.Stub(t => t.TimeZone()).Return(TimeZoneInfo.Local);

			_person = new Person();

			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(_person);

			_multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("Test Definition Set", MultiplicatorType.Overtime);
			_multiplicatorDefinitionSetRepository = MockRepository.GenerateMock<IMultiplicatorDefinitionSetRepository>();
			_multiplicatorDefinitionSetRepository.Stub(x => x.Get(Guid.NewGuid())).IgnoreArguments()
				.Return(_multiplicatorDefinitionSet);

			target = new OvertimeRequestFormMapper(_loggedOnUser, _multiplicatorDefinitionSetRepository, _userTimeZone);
		}

		[Test]
		public void ShouldMapNewCreatedOvertimeRequest()
		{
			var today = DateTime.Now.Date;
			var form = createOvertimeRequestForm(today);

			var result = target.Map(form, null);

			result.Id.Should().Be(null);
			result.Person.Should().Be.SameInstanceAs(_person);
			result.StatusText.Should().Be("New");
			result.GetSubject(_formatter).Should().Be(subject);
			result.GetMessage(_formatter).Should().Be(message);

			checkOvertimeRequest(result, today);
		}

		[Test]
		public void ShouldMapExistingOvertimeRequest()
		{
			var today = DateTime.Now.Date;
			var requestId = Guid.NewGuid();
			var form = createOvertimeRequestForm(today, requestId);

			var anotherPerson = new Person();
			var personRequest = createPersonRequest(requestId, anotherPerson);

			var result = target.Map(form, personRequest);
			result.Id.Should().Be(requestId);
			result.StatusText.Should().Be("Pending");
			result.Person.Should().Be.SameInstanceAs(anotherPerson);
			result.GetSubject(_formatter).Should().Be(subject);
			result.GetMessage(_formatter).Should().Be(message);

			checkOvertimeRequest(result, today);
		}

		private void checkOvertimeRequest(IPersonRequest result, DateTime today)
		{
			var overtimeRequest = result.Request as IOvertimeRequest;
			overtimeRequest.Should().Not.Be.Null();
			overtimeRequest.MultiplicatorDefinitionSet.Should().Be.SameInstanceAs(_multiplicatorDefinitionSet);
			overtimeRequest.Period.StartDateTimeLocal(_userTimeZone.TimeZone()).Should().Be
				.EqualTo(today.AddMinutes(startTimeInMinute));
			overtimeRequest.Period.EndDateTimeLocal(_userTimeZone.TimeZone()).Should().Be
				.EqualTo(today.AddMinutes(endTimeInMinute));
		}

		private static OvertimeRequestForm createOvertimeRequestForm(DateTime today, Guid? requestId = null)
		{
			return new OvertimeRequestForm
			{
				Id = requestId,
				Subject = subject,
				Message = message,
				MultiplicatorDefinitionSet = Guid.NewGuid(),
				Period = new DateTimePeriodForm
				{
					StartDate = new DateOnly(today),
					StartTime = new TimeOfDay(TimeSpan.FromMinutes(startTimeInMinute)),
					EndDate = new DateOnly(today),
					EndTime = new TimeOfDay(TimeSpan.FromMinutes(endTimeInMinute))
				}
			};
		}

		private IPersonRequest createPersonRequest(Guid requestId, IPerson person)
		{
			var personRequestFactory = new PersonRequestFactory();
			var personRequest = personRequestFactory.CreatePersonRequest(person);
			personRequest.SetId(requestId);
			personRequest.Pending();
			return personRequest;
		}
	}
}