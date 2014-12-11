using System;
using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.Mapping
{
	[TestFixture]
	public class StudentAvailabilityDayViewModelMappingTest
	{
		private IStudentAvailabilityProvider _studentAvailabilityProvider;
		private Person _person;

		[SetUp]
		public void Setup()
		{
			_studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			_person = new Person();
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new StudentAvailabilityDayViewModelMappingProfile(() => _studentAvailabilityProvider)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var studentAvailabilityDay = new StudentAvailabilityDay(_person, DateOnly.Today, new List<IStudentAvailabilityRestriction>());

			var result = Mapper.Map<StudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailabilityDay);

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
			var studentAvailabilityDay = new StudentAvailabilityDay(_person, DateOnly.Today, new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction });

			_studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDay(studentAvailabilityDay)).Return(studentAvailabilityRestriction);

			var result = Mapper.Map<StudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailabilityDay);

			result.AvailableTimeSpan.Should().Be(studentAvailabilityRestriction.StartTimeLimitation.StartTimeString + " - " + studentAvailabilityRestriction.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapAvailableTimeSpanWhenNoRestriction()
		{
			var studentAvailabilityDay = new StudentAvailabilityDay(_person, DateOnly.Today, new List<IStudentAvailabilityRestriction>());

			_studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDay(studentAvailabilityDay)).Return(null);

			var result = Mapper.Map<StudentAvailabilityDay, StudentAvailabilityDayViewModel>(studentAvailabilityDay);

			result.AvailableTimeSpan.Should().Be.Empty();
		}
	}
}