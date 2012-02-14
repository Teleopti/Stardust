using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.StudentAvailability.DataProvider
{
	[TestFixture]
	public class VirtualSchedulePeriodProviderTest
	{
		[Test]
		public void ShouldGetPeriodForDate()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var date = DateOnly.Today;
			var period = new DateOnlyPeriod(date, date.AddDays(3));

			personProvider.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.VirtualSchedulePeriodOrNext(date)).Return(virtualSchedulePeriod);
			virtualSchedulePeriod.Stub(x => x.DateOnlyPeriod).Return(period);

			var target = new VirtualSchedulePeriodProvider(personProvider, null);

			var result = target.GetCurrentOrNextVirtualPeriodForDate(date);

			result.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldDetermineThatPersonHasSchedulePeriods()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();

			personProvider.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>(new[] { MockRepository.GenerateMock<ISchedulePeriod>() }));

			var target = new VirtualSchedulePeriodProvider(personProvider, null);

			var result = target.HasSchedulePeriod();

			result.Should().Be.True();
		}

		[Test]
		public void ShouldDetermineThatPersonHasNoSchedulePeriods()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();

			personProvider.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

			var target = new VirtualSchedulePeriodProvider(personProvider, null);

			var result = target.HasSchedulePeriod();

			result.Should().Be.False();
		}

		[Test]
		public void ShouldCalculateDefaultDateForStudentAvailabilityUsingCalculator()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var defaultDateCalculator = MockRepository.GenerateMock<IDefaultDateCalculator>();
			var date = DateOnly.Today.AddDays(1);
			var person = new Person
							{
								WorkflowControlSet = new WorkflowControlSet(null)
							};
			personProvider.Stub(x => x.CurrentUser()).Return(person);
			defaultDateCalculator.Stub(x => x.Calculate(person.WorkflowControlSet, VirtualSchedulePeriodProvider.StudentAvailabilityPeriod)).Return(date);

			var target = new VirtualSchedulePeriodProvider(personProvider, defaultDateCalculator);

			var result = target.CalculateStudentAvailabilityDefaultDate();

			result.Should().Be.EqualTo(date);
		}

		[Test]
		public void ShouldCalculateDefaultDateForPreferencesUsingCalculator()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var defaultDateCalculator = MockRepository.GenerateMock<IDefaultDateCalculator>();
			var date = DateOnly.Today.AddDays(1);
			var person = new Person
			{
				WorkflowControlSet = new WorkflowControlSet(null)
			};
			personProvider.Stub(x => x.CurrentUser()).Return(person);
			defaultDateCalculator.Stub(x => x.Calculate(person.WorkflowControlSet, VirtualSchedulePeriodProvider.PreferencePeriod)).Return(date);

			var target = new VirtualSchedulePeriodProvider(personProvider, defaultDateCalculator);

			var result = target.CalculatePreferenceDefaultDate();

			result.Should().Be.EqualTo(date);
		}

	}
}