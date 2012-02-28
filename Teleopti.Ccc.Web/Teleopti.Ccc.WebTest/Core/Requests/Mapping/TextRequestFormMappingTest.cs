using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class TextRequestFormMappingTest
	{
		private ILoggedOnUser loggedOnUser;
		private Person person;
		private IUserTimeZone userTimeZone;
		private TextRequestFormMappingProfile.TextRequestFormToPersonRequest textRequestFormToPersonRequest;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			textRequestFormToPersonRequest =
				new TextRequestFormMappingProfile.TextRequestFormToPersonRequest(() => Mapper.Engine, () => loggedOnUser);

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(
				new TextRequestFormMappingProfile(
					() => Mapper.Engine, 
					() => loggedOnUser, 
					() => userTimeZone,
					() => textRequestFormToPersonRequest
					)));
		}	

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapPerson()
		{
			var result = Mapper.Map<TextRequestForm, IPersonRequest>(new TextRequestForm());

			result.Person.Should().Be.SameInstanceAs(person);
		}
		
		[Test]
		public void ShouldMapSubject()
		{
			var form = new TextRequestForm {Subject = "Test"};

			var result = Mapper.Map<TextRequestForm, IPersonRequest>(form);

			result.GetSubject(new NoFormatting()).Should().Be("Test");
		}

		[Test]
		public void ShouldMapToTextRequest()
		{
			var result = Mapper.Map<TextRequestForm, IPersonRequest>(new TextRequestForm());

			result.Request.Should().Be.OfType<TextRequest>();
		}

		[Test]
		public void ShouldMapAsPending()
		{
			var result = Mapper.Map<TextRequestForm, IPersonRequest>(new TextRequestForm());

			result.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldMapToUtcPeriod()
		{
			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", ""));
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var form = new TextRequestForm
			           	{
							Period = new DateTimePeriodForm
							         	{
											StartDate = DateOnly.Today,
											StartTime = new TimeOfDay(TimeSpan.FromHours(12)),
											EndDate = DateOnly.Today,
											EndTime = new TimeOfDay(TimeSpan.FromHours(13))
										}
			           	};
			var result = Mapper.Map<TextRequestForm, IPersonRequest>(form);

			var expected = new DateTimePeriod(
				TimeZoneHelper.ConvertToUtc(form.Period.StartDate.Date.Add(form.Period.StartTime.Time), timeZone),
				TimeZoneHelper.ConvertToUtc(form.Period.EndDate.Date.Add(form.Period.EndTime.Time), timeZone)
				);
			result.Request.Period.Should().Be(expected);
		}

		[Test]
		public void ShouldMapMessage()
		{
			var form = new TextRequestForm {Message = "Message"};
			
			var result = Mapper.Map<TextRequestForm, IPersonRequest>(form);

			result.GetMessage(new NoFormatting()).Should().Be("Message");
		}

		[Test]
		public void ShouldMapId()
		{
			var id = Guid.NewGuid();
			var form = new TextRequestForm {EntityId = id};

			var result = Mapper.Map<TextRequestForm, IPersonRequest>(form);

			result.Id.Should().Be(id);
		}

		[Test]
		public void ShouldMapToDestination()
		{
			var destination = new PersonRequest(new Person());

			var result = Mapper.Map<TextRequestForm, IPersonRequest>(new TextRequestForm(), destination);

			result.Should().Be.SameInstanceAs(destination);
		}

		[Test]
		public void ShouldMapMessageToDestination()
		{
			var form = new TextRequestForm {Message = "message"};
			var destination = new PersonRequest(new Person());

			var result = Mapper.Map<TextRequestForm, IPersonRequest>(form, destination);

			result.GetMessage(new NoFormatting()).Should().Be.EqualTo(form.Message);
		}

		[Test]
		public void ShouldMapSubjectToDestination()
		{
			var form = new TextRequestForm {Subject = "subject"};
			var destination = new PersonRequest(new Person());

			var result = Mapper.Map<TextRequestForm, IPersonRequest>(form, destination);

			result.GetSubject(new NoFormatting()).Should().Be.EqualTo(form.Subject);
		}
	}
}
