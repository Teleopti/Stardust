using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class OvertimeRequestFormMappingTest
	{
		private ILoggedOnUser _loggedOnUser;
		private Person _person;
		private IUserTimeZone _userTimeZone;
		private MultiplicatorDefinitionSet _multiplicatorDefinitionSet;
		private IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private OvertimeRequestFormMapper target;

		[SetUp]
		public void Setup()
		{
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
		public void ShouldMapPerson()
		{
			const string subject = "Overtime request";
			const string message = "I want work overtime!";
			const int startTimeInMinute = 75;
			const int endTimeInMinute = 135;
			var today = DateTime.Now.Date;
			var form = new OvertimeRequestForm
			{
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

			var result = target.Map(form);

			result.Person.Should().Be.SameInstanceAs(_person);
			result.GetSubject(new NoFormatting()).Should().Be(subject);
			result.GetMessage(new NoFormatting()).Should().Be(message);
			result.StatusText.Should().Be("New");

			var overtimeRequest = result.Request as IOvertimeRequest;
			overtimeRequest.Should().Not.Be.Null();
			overtimeRequest.MultiplicatorDefinitionSet.Should().Be.SameInstanceAs(_multiplicatorDefinitionSet);
			overtimeRequest.Period.StartDateTimeLocal(_userTimeZone.TimeZone()).Should().Be
				.EqualTo(today.AddMinutes(startTimeInMinute));
			overtimeRequest.Period.EndDateTimeLocal(_userTimeZone.TimeZone()).Should().Be
				.EqualTo(today.AddMinutes(endTimeInMinute));
		}
	}
}