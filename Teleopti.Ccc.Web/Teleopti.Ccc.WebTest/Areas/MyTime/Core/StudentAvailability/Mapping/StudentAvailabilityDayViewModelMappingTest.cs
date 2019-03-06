using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.Mapping
{
	[TestFixture]
	public class StudentAvailabilityDayViewModelMappingTest
	{
		private IStudentAvailabilityProvider _studentAvailabilityProvider;
		private Person _person;
		private StudentAvailabilityDayViewModelMapper target;

		[SetUp]
		public void Setup()
		{
			_studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			_person = new Person();
			target = new StudentAvailabilityDayViewModelMapper(_studentAvailabilityProvider, new BankHolidayCalendarViewModelMapper());
		}

		[Test]
		public void ShouldMapDate()
		{
			var studentAvailabilityDay = new StudentAvailabilityDay(_person, DateOnly.Today, new List<IStudentAvailabilityRestriction>());

			var result = target.Map(studentAvailabilityDay, null);

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

			var result = target.Map(studentAvailabilityDay, null);

			result.AvailableTimeSpan.Should().Be(studentAvailabilityRestriction.StartTimeLimitation.StartTimeString + " - " + studentAvailabilityRestriction.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapAvailableTimeSpanWhenNoRestriction()
		{
			var studentAvailabilityDay = new StudentAvailabilityDay(_person, DateOnly.Today, new List<IStudentAvailabilityRestriction>());

			_studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityForDay(studentAvailabilityDay)).Return(null);

			var result = target.Map(studentAvailabilityDay, null);

			result.AvailableTimeSpan.Should().Be.Empty();
		}
	}
}