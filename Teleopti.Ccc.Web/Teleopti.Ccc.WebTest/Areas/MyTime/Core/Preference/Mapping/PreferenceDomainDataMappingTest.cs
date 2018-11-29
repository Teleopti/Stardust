using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceDomainDataMappingTest
	{
		private IVirtualSchedulePeriodProvider virtualScheduleProvider;
		private FakePreferenceDayRepository preferenceDayRepository;
		private IPerson person;
		private PreferenceDomainDataMapper target;

		[SetUp]
		public void Setup()
		{
			virtualScheduleProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
 
			person = new Person
			         	{
			         		WorkflowControlSet = new WorkflowControlSet(null)
			         		                     	{
			         		                     		PreferencePeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)),
			         		                     		PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
			         		                     	}
			         	};

			preferenceDayRepository = new FakePreferenceDayRepository();
			var mustHaveRestrictionProvider = new MustHaveRestrictionProvider(preferenceDayRepository);

			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			target = new PreferenceDomainDataMapper(virtualScheduleProvider,loggedOnUser,mustHaveRestrictionProvider);
		}
		
		[Test]
		public void ShouldMapSelectedDate()
		{
			var result = target.Map(DateOnly.Today);

			result.SelectedDate.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldMapPeriod()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);

			var result = target.Map(DateOnly.Today);

			result.Period.Should().Be(period);
		}

		[Test]
		public void ShouldMapWorkflowControlSet()
		{
			var result = target.Map(DateOnly.Today);

			result.WorkflowControlSet.Should().Be.SameInstanceAs(person.WorkflowControlSet);
		}

		[Test]
		public void ShouldMapDays()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3));

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);

			var result = target.Map(DateOnly.Today);

			var expected = DateOnly.Today.DateRange(4);
			result.Days.Select(d => d.Date).Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldMapMaxMustHave()
		{
			const int maxMustHave = 8;
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.MustHavePreference).Return(maxMustHave);
			virtualScheduleProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);

			var result = target.Map(DateOnly.Today);

			result.MaxMustHave.Should().Be.EqualTo(maxMustHave);
		}

		[Test]
		public void ShouldReturnCorrectCurrentMustHave()
		{
			preferenceDayRepository.Add(new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction
			{
				MustHave = true
			}));
			preferenceDayRepository.Add(new PreferenceDay(person,DateOnly.Today.AddDays(1),new PreferenceRestriction
			{
				MustHave = false
			}));

			person.AddSchedulePeriod(new SchedulePeriod(DateOnly.Today.AddDays(-1), SchedulePeriodType.Day, 10000 ));

			const int maxMustHave = 8;
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			virtualSchedulePeriod.Stub(x => x.MustHavePreference).Return(maxMustHave);
			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today))
				.Return(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3)));

			var result = target.Map(DateOnly.Today);
			result.CurrentMustHave.Should().Be.EqualTo(1);
		}
	}
}