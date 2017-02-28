﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class TextRequestFormMappingTest
	{
		private ILoggedOnUser loggedOnUser;
		private IUserTimeZone userTimeZone;
		private TextRequestFormMapper target;
		private IPerson person;

		[SetUp]
		public void Setup()
		{
			person = new Person();
			loggedOnUser = new FakeLoggedOnUser(person);
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			
			target = new TextRequestFormMapper(loggedOnUser,userTimeZone,new DateTimePeriodFormMapper(userTimeZone));
		}	
		
		[Test]
		public void ShouldMapPerson()
		{
			var result = target.Map(new TextRequestForm());

			result.Person.Should().Be.SameInstanceAs(person);
		}
		
		[Test]
		public void ShouldMapSubject()
		{
			var form = new TextRequestForm {Subject = "Test"};

			var result = target.Map(form);

			result.GetSubject(new NoFormatting()).Should().Be("Test");
		}

		[Test]
		public void ShouldMapToTextRequest()
		{
			var result = target.Map(new TextRequestForm());

			result.Request.Should().Be.OfType<TextRequest>();
		}

		[Test]
		public void ShouldMapAsPending()
		{
			var result = target.Map(new TextRequestForm());

			result.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldMapPeriod()
		{
			var timeZone = TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", "");
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
			var result = target.Map(form);

			var expected = new DateTimePeriodFormMapper(new FakeUserTimeZone(timeZone)).Map(form.Period);
			result.Request.Period.Should().Be(expected);
		}

		[Test]
		public void ShouldMapMessage()
		{
			var form = new TextRequestForm {Message = "Message"};
			
			var result = target.Map(form);

			result.GetMessage(new NoFormatting()).Should().Be("Message");
		}

		[Test]
		public void ShouldMapId()
		{
			var id = Guid.NewGuid();
			var form = new TextRequestForm {EntityId = id};

			var result = target.Map(form);

			result.Id.Should().Be(id);
		}

		[Test]
		public void ShouldMapToDestination()
		{
			var destination = new PersonRequest(new Person());

			var result = target.Map(new TextRequestForm(), destination);

			result.Should().Be.SameInstanceAs(destination);
		}

		[Test]
		public void ShouldMapMessageToDestination()
		{
			var form = new TextRequestForm {Message = "message"};
			var destination = new PersonRequest(new Person());

			var result = target.Map(form, destination);

			result.GetMessage(new NoFormatting()).Should().Be.EqualTo(form.Message);
		}

		[Test]
		public void ShouldMapSubjectToDestination()
		{
			var form = new TextRequestForm {Subject = "subject"};
			var destination = new PersonRequest(new Person());

			var result = target.Map(form, destination);

			result.GetSubject(new NoFormatting()).Should().Be.EqualTo(form.Subject);
		}

		[Test]
		public void ShouldMapFullDayRequest()
		{
			var timeZone = TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", "");
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var form = new TextRequestForm { FullDay = true };

			var result = target.Map(form);

			var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 0, 0, 0), timeZone);
			var endTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 23, 59, 0), timeZone);

			var expected = new DateTimePeriod(startTime, endTime);

			result.Request.Period.Should().Be(expected);
		}
	}
}
