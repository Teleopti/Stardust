using System;
using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.StudentAvailability.Mapping
{
	[TestFixture]
	public class StudentAvailabilityDayFormResultMappingTest
	{
		private IStudentAvailabilityProvider studentAvailabilityProvider;

		[SetUp]
		public void Setup()
		{
			studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new StudentAvailabilityDayFormResultMappingProfile(() => studentAvailabilityProvider)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var studentAvailabilityDay = new StudentAvailabilityDay(null, DateOnly.Today, new List<IStudentAvailabilityRestriction>());

			var result = Mapper.Map<StudentAvailabilityDay, StudentAvailabilityDayFormResult>(studentAvailabilityDay);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapAvailableTimeSpan()
		{
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction
			                                     	{
			                                     		StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), null),
			                                     		EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(18))
			                                     	};
			var studentAvailabilityDay = new StudentAvailabilityDay(null, DateOnly.Today, new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction });

			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDay(studentAvailabilityDay)).Return(studentAvailabilityRestriction);

			var result = Mapper.Map<StudentAvailabilityDay, StudentAvailabilityDayFormResult>(studentAvailabilityDay);

			result.AvailableTimeSpan.Should().Be(studentAvailabilityRestriction.StartTimeLimitation.StartTimeString + " - " + studentAvailabilityRestriction.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapAvailableTimeSpanWhenNoRestriction()
		{
			var studentAvailabilityDay = new StudentAvailabilityDay(null, DateOnly.Today, new List<IStudentAvailabilityRestriction>());

			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDay(studentAvailabilityDay)).Return(null);

			var result = Mapper.Map<StudentAvailabilityDay, StudentAvailabilityDayFormResult>(studentAvailabilityDay);

			result.AvailableTimeSpan.Should().Be.Null();
		}
	}
}