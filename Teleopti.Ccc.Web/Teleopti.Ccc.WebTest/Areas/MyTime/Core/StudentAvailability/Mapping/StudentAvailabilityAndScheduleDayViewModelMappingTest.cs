using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.Mapping
{
	[TestFixture]
	public class StudentAvailabilityAndScheduleDayViewModelMappingTest
	{
		private IStudentAvailabilityProvider _studentAvailabilityProvider;

		[SetUp]
		public void Setup()
		{
			_studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();

			Mapper.Reset();
			Mapper.Initialize(c =>
				{
					c.AddProfile(new StudentAvailabilityAndScheduleDayViewModelMappingProfile());
					c.AddProfile(new StudentAvailabilityDayViewModelMappingProfile(() => _studentAvailabilityProvider));
				});
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);

			var result = Mapper.Map<IScheduleDay, StudentAvailabilityAndScheduleDayViewModel>(scheduleDay);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPreferenceViewModel()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			var studentAvailabilityDay=new StudentAvailabilityDay(new Person(), DateOnly.Today, new List<IStudentAvailabilityRestriction> {studentAvailabilityRestriction});
			var personRestrictionCollection =
				new ReadOnlyObservableCollection<IScheduleData>(new ObservableCollection<IScheduleData>(new[] { studentAvailabilityDay }));

			scheduleDay.Stub(x => x.PersonRestrictionCollection()).Return(personRestrictionCollection);

			var result = Mapper.Map<IScheduleDay, StudentAvailabilityAndScheduleDayViewModel>(scheduleDay);

			result.StudentAvailability.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldAllowMultipleStudentAvailabilityDayLoaded()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			var studentAvailabilityDay1 = new StudentAvailabilityDay(new Person(), DateOnly.Today, new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction });
			var studentAvailabilityDay2 = new StudentAvailabilityDay(new Person(), DateOnly.Today, new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction });
			var personRestrictionCollection =
				new ReadOnlyObservableCollection<IScheduleData>(new ObservableCollection<IScheduleData>(new[] { studentAvailabilityDay1, studentAvailabilityDay2 }));

			scheduleDay.Stub(x => x.PersonRestrictionCollection()).Return(personRestrictionCollection);

			var result = Mapper.Map<IScheduleDay, StudentAvailabilityAndScheduleDayViewModel>(scheduleDay);

			result.StudentAvailability.Should().Not.Be.Null();
		}
	}
}