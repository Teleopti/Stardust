using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.ViewModelFactory
{
	[TestFixture]
	public class StudentAvailabilityViewModelFactoryTest
	{
		[Test]
		public void ShoudCreateViewModelByMapping()
		{
			var scheduleProvider = new FakeScheduleProvider();
			var loggedOnUser = new FakeLoggedOnUser();
			PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(loggedOnUser.CurrentUser(),new DateOnly(2000, 1, 1));
			var now = new Now();
			var target =
				new StudentAvailabilityViewModelFactory(
					new StudentAvailabilityViewModelMapper(scheduleProvider,
						new DefaultScenarioForStudentAvailabilityScheduleProvider(scheduleProvider),
						new VirtualSchedulePeriodProvider(loggedOnUser, new DefaultDateCalculator(now)),
						loggedOnUser, now),
					new StudentAvailabilityDayFeedbackViewModelMapper(
						new StudentAvailabilityFeedbackProvider(
							new WorkTimeMinMaxCalculator(
								new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate()))),
								new WorkTimeMinMaxRestrictionCreator(
									new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(), new RestrictionCombiner()))),
							loggedOnUser, scheduleProvider, new FakePersonRuleSetBagProvider())),
					new StudentAvailabilityDayViewModelMapper(
						new DefaultScenarioForStudentAvailabilityScheduleProvider(scheduleProvider)),
					new DefaultScenarioForStudentAvailabilityScheduleProvider(scheduleProvider));
			var date = DateOnly.Today;
			
			target.CreateViewModel(date).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCreateDayViewModel()
		{
			var studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			var scheduleProvider = new FakeScheduleProvider();
			var loggedOnUser = new FakeLoggedOnUser();
			PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(loggedOnUser.CurrentUser(), new DateOnly(2000, 1, 1));
			var now = new Now();
			var target =
				new StudentAvailabilityViewModelFactory(
					new StudentAvailabilityViewModelMapper(scheduleProvider,
						studentAvailabilityProvider,
						new VirtualSchedulePeriodProvider(loggedOnUser, new DefaultDateCalculator(now)),
						loggedOnUser, now),
					new StudentAvailabilityDayFeedbackViewModelMapper(
						new StudentAvailabilityFeedbackProvider(
							new WorkTimeMinMaxCalculator(
								new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate()))),
								new WorkTimeMinMaxRestrictionCreator(
									new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(), new RestrictionCombiner()))),
							loggedOnUser, scheduleProvider, new FakePersonRuleSetBagProvider())),
					new StudentAvailabilityDayViewModelMapper(
						studentAvailabilityProvider),
					studentAvailabilityProvider);
			var date = DateOnly.Today;
			var studentAvailabilityDay = new StudentAvailabilityDay(new Person(), date, new List<IStudentAvailabilityRestriction> {new StudentAvailabilityRestriction()});
			
			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityDayForDate(date)).Return(studentAvailabilityDay);
			
			target.CreateDayViewModel(date).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldCreateStudentAvailabilityAndSchedulesViewModels()
		{
			var studentAvailabilityProvider = MockRepository.GenerateMock<IStudentAvailabilityProvider>();
			var scheduleProvider = new FakeScheduleProvider();
			var loggedOnUser = new FakeLoggedOnUser();
			PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(loggedOnUser.CurrentUser(), new DateOnly(2000, 1, 1));
			var now = new Now();
			var target =
				new StudentAvailabilityViewModelFactory(
					new StudentAvailabilityViewModelMapper(scheduleProvider,
						studentAvailabilityProvider,
						new VirtualSchedulePeriodProvider(loggedOnUser, new DefaultDateCalculator(now)),
						loggedOnUser, now),
					new StudentAvailabilityDayFeedbackViewModelMapper(
						new StudentAvailabilityFeedbackProvider(
							new WorkTimeMinMaxCalculator(
								new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate()))),
								new WorkTimeMinMaxRestrictionCreator(
									new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(), new RestrictionCombiner()))),
							loggedOnUser, scheduleProvider, new FakePersonRuleSetBagProvider())),
					new StudentAvailabilityDayViewModelMapper(
						studentAvailabilityProvider),
					studentAvailabilityProvider);
			var date = DateOnly.Today;
			var studentAvailabilityDays = new List<StudentAvailabilityDay>
			{
				new StudentAvailabilityDay(new Person(), date, new List<IStudentAvailabilityRestriction>())
			};
			
			var period = new DateOnlyPeriod(date, date.AddDays(1));
			studentAvailabilityProvider.Stub(x => x.GetStudentAvailabilityDayForPeriod(period))
				.Return(studentAvailabilityDays);
			
			target.CreateStudentAvailabilityAndSchedulesViewModels(period.StartDate, period.EndDate).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldCreateFeedbackDayViewModelByMapping()
		{
			var scheduleProvider = new FakeScheduleProvider();
			var loggedOnUser = new FakeLoggedOnUser();
			PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(loggedOnUser.CurrentUser(), new DateOnly(2000, 1, 1));
			var now = new Now();
			var target =
				new StudentAvailabilityViewModelFactory(
					new StudentAvailabilityViewModelMapper(scheduleProvider,
						new DefaultScenarioForStudentAvailabilityScheduleProvider(scheduleProvider),
						new VirtualSchedulePeriodProvider(loggedOnUser, new DefaultDateCalculator(now)),
						loggedOnUser, now),
					new StudentAvailabilityDayFeedbackViewModelMapper(
						new StudentAvailabilityFeedbackProvider(
							new WorkTimeMinMaxCalculator(
								new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate()))),
								new WorkTimeMinMaxRestrictionCreator(
									new EffectiveRestrictionForDisplayCreator(new RestrictionRetrievalOperation(), new RestrictionCombiner()))),
							loggedOnUser, scheduleProvider, new FakePersonRuleSetBagProvider())),
					new StudentAvailabilityDayViewModelMapper(
						new DefaultScenarioForStudentAvailabilityScheduleProvider(scheduleProvider)),
					new DefaultScenarioForStudentAvailabilityScheduleProvider(scheduleProvider));
			
			target.CreateDayFeedbackViewModel(DateOnly.Today).Should().Not.Be.Null();
		}
	}
}