using System;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.StudentAvailability.Mapping
{
	[TestFixture]
	public class StudentAvailabilityDayViewModelMappingTest
	{
		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new StudentAvailabilityDayViewModelMappingProfile()));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapStartTime()
		{
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction
			                                     	{
			                                     		StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), null),
			                                     	};

			var result = Mapper.Map<IStudentAvailabilityRestriction, StudentAvailabilityDayViewModel>(studentAvailabilityRestriction);

			result.StartTime.Should().Be.EqualTo("07:00");
		}

		[Test]
		public void ShouldMapEndTime()
		{
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction
			                                        {
														EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(18)),
			                                        };

			var result = Mapper.Map<IStudentAvailabilityRestriction, StudentAvailabilityDayViewModel>(studentAvailabilityRestriction);

			result.EndTime.Should().Be.EqualTo("18:00");
		}

		[Test]
		public void ShouldMapNextDay()
		{
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction
			          								{
														EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(26)),
			          								};
			var result = Mapper.Map<IStudentAvailabilityRestriction, StudentAvailabilityDayViewModel>(studentAvailabilityRestriction);

			result.EndTime.Should().Be.EqualTo("02:00");
			result.NextDay.Should().Be.True();
		}
	}
}