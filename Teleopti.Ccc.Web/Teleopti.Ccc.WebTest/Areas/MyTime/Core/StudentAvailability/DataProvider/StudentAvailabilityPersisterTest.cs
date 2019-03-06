using System.Collections.Generic;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	[TestFixture]
	public class StudentAvailabilityPersisterTest
	{
		[Test]
		public void ShouldAddStudentAvailability()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, loggedOnUser, new StudentAvailabilityDayFormMapper(loggedOnUser), new StudentAvailabilityDayViewModelMapper(new DefaultScenarioForStudentAvailabilityScheduleProvider(new FakeScheduleProvider()), new BankHolidayCalendarViewModelMapper()));
			var form = new StudentAvailabilityDayInput();
			
			target.Persist(form);

			studentAvailabilityDayRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldReturnFormResultModelOnAdd()
		{
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new StudentAvailabilityPersister(new FakeStudentAvailabilityDayRepository(), loggedOnUser, new StudentAvailabilityDayFormMapper(loggedOnUser), new StudentAvailabilityDayViewModelMapper(new DefaultScenarioForStudentAvailabilityScheduleProvider(new FakeScheduleProvider()), new BankHolidayCalendarViewModelMapper()));
			var form = new StudentAvailabilityDayInput();
			
			target.Persist(form).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldUpdateExistingStudentAvailability()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var form = new StudentAvailabilityDayInput {Date = DateOnly.Today};
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, loggedOnUser, new StudentAvailabilityDayFormMapper(loggedOnUser), new StudentAvailabilityDayViewModelMapper(new DefaultScenarioForStudentAvailabilityScheduleProvider(new FakeScheduleProvider()), new BankHolidayCalendarViewModelMapper()));

			studentAvailabilityDayRepository.Stub(x => x.Find(form.Date, null)).Return(new List<IStudentAvailabilityDay> { studentAvailabilityDay });
			
			target.Persist(form);
		}

		[Test]
		public void ShouldReturnFormResultModelOnUpdate()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var form = new StudentAvailabilityDayInput { Date = DateOnly.Today };
			var loggedOnUser = new FakeLoggedOnUser();
			var studentAvailabilityDay = new StudentAvailabilityDay(loggedOnUser.CurrentUser(),DateOnly.Today, new List<IStudentAvailabilityRestriction> {new StudentAvailabilityRestriction()});
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, loggedOnUser, new StudentAvailabilityDayFormMapper(loggedOnUser), new StudentAvailabilityDayViewModelMapper(new DefaultScenarioForStudentAvailabilityScheduleProvider(new FakeScheduleProvider()), new BankHolidayCalendarViewModelMapper()));
			
			studentAvailabilityDayRepository.Stub(x => x.Find(form.Date, loggedOnUser.CurrentUser())).Return(new List<IStudentAvailabilityDay> { studentAvailabilityDay });
			
			target.Persist(form).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDelete()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, loggedOnUser, new StudentAvailabilityDayFormMapper(loggedOnUser), new StudentAvailabilityDayViewModelMapper(new DefaultScenarioForStudentAvailabilityScheduleProvider(new FakeScheduleProvider()), new BankHolidayCalendarViewModelMapper()));

			studentAvailabilityDayRepository.Stub(x => x.Find(DateOnly.Today, loggedOnUser.CurrentUser())).Return(new List<IStudentAvailabilityDay> { studentAvailabilityDay });

			target.Delete(DateOnly.Today);

			studentAvailabilityDayRepository.AssertWasCalled(x => x.Remove(studentAvailabilityDay));
		}

		[Test]
		public void ShouldReturnEmptyInputResultOnDelete()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var studentAvailabilityDay = MockRepository.GenerateMock<IStudentAvailabilityDay>();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, loggedOnUser, new StudentAvailabilityDayFormMapper(loggedOnUser), new StudentAvailabilityDayViewModelMapper(new DefaultScenarioForStudentAvailabilityScheduleProvider(new FakeScheduleProvider()), new BankHolidayCalendarViewModelMapper()));

			studentAvailabilityDayRepository.Stub(x => x.Find(DateOnly.Today, loggedOnUser.CurrentUser())).Return(new List<IStudentAvailabilityDay> { studentAvailabilityDay });

			var result = target.Delete(DateOnly.Today);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
			result.AvailableTimeSpan.Should().Be.Null();
		}

		[Test]
		public void ShouldThrowHttp404OIfStudentAvailabilityDoesNotExists()
		{
			var studentAvailabilityDayRepository = MockRepository.GenerateMock<IStudentAvailabilityDayRepository>();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new StudentAvailabilityPersister(studentAvailabilityDayRepository, loggedOnUser, new StudentAvailabilityDayFormMapper(loggedOnUser), new StudentAvailabilityDayViewModelMapper(new DefaultScenarioForStudentAvailabilityScheduleProvider(new FakeScheduleProvider()), new BankHolidayCalendarViewModelMapper()));

			studentAvailabilityDayRepository.Stub(x => x.Find(DateOnly.Today, loggedOnUser.CurrentUser())).Return(new List<IStudentAvailabilityDay>());

			var exception = Assert.Throws<HttpException>(() => target.Delete(DateOnly.Today));
			exception.GetHttpCode().Should().Be(404);
		}
	}
}
