using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class VirtualSchedulePeriodProviderTest
	{
		[Test]
		public void ShouldGetCurrentOrNextVirtualPeriodForDate()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3));

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.VirtualSchedulePeriodOrNext(DateOnly.Today)).Return(virtualSchedulePeriod);
			virtualSchedulePeriod.Stub(x => x.DateOnlyPeriod).Return(period);

			var target = new VirtualSchedulePeriodProvider(loggedOnUser, null);

			var result = target.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today);

			result.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldGetVirtualSchedulePeriodForDate()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.VirtualSchedulePeriodOrNext(DateOnly.Today)).Return(virtualSchedulePeriod);

			var target = new VirtualSchedulePeriodProvider(loggedOnUser, null);

			var result = target.VirtualSchedulePeriodForDate(DateOnly.Today);

			result.Should().Be.EqualTo(virtualSchedulePeriod);
		}

		[Test]
		public void ShouldDetermineThatPersonHasSchedulePeriods()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();

			personProvider.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>(new[] { MockRepository.GenerateMock<ISchedulePeriod>() }));

			var target = new VirtualSchedulePeriodProvider(personProvider, null);

			var result = target.MissingSchedulePeriod();

			result.Should().Be.False();
		}

		[Test]
		public void ShouldDetermineThatPersonHasNoSchedulePeriods()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();

			personProvider.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.PersonSchedulePeriodCollection).Return(new List<ISchedulePeriod>());

			var target = new VirtualSchedulePeriodProvider(personProvider, null);

			var result = target.MissingSchedulePeriod();

			result.Should().Be.True();
		}

 		[Test]
		public void ShouldDetermineThatPersonHasNoPersonPeriod()
 		{
 			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
 			var person = MockRepository.GenerateMock<IPerson>();
 
 			personProvider.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.PersonPeriodCollection).Return(null);
 
 			var target = new VirtualSchedulePeriodProvider(personProvider, null);
 
			var result = target.MissingPersonPeriod(DateOnly.Today);
 
 			result.Should().Be.True();
 		}
 
 		[Test]
		public void ShouldDetermineThatPersonHasPersonPeriod()
 		{
 			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
 			var person = MockRepository.GenerateMock<IPerson>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
 
 			personProvider.Stub(x => x.CurrentUser()).Return(person);
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(7));
			personPeriod.Stub(x => x.Period).Return(period);
			person.Stub(x => x.PersonPeriodCollection).Return(new List<IPersonPeriod>(new[] { personPeriod }));
			
			var target = new VirtualSchedulePeriodProvider(personProvider, null);

			var result = target.MissingPersonPeriod(DateOnly.Today.AddDays(2));

			result.Should().Be.False();
		}

		[Test]
		public void ShouldDetermineThatPersonMissesPersonPeriodOnSpecificDate()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();

			personProvider.Stub(x => x.CurrentUser()).Return(person);
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(7));
			personPeriod.Stub(x => x.Period).Return(period);
			person.Stub(x => x.PersonPeriodCollection).Return(new List<IPersonPeriod>(new[] { personPeriod }));
 
 			var target = new VirtualSchedulePeriodProvider(personProvider, null);
 
			var result = target.MissingPersonPeriod(DateOnly.Today.AddDays(8));
 
			result.Should().Be.True();
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
			defaultDateCalculator.Stub(x => x.Calculate(person.WorkflowControlSet, VirtualSchedulePeriodProvider.StudentAvailabilityPeriod, new List<IPersonPeriod>())).Return(date);

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
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today));
			personProvider.Stub(x => x.CurrentUser()).Return(person);
			defaultDateCalculator.Stub(
				x =>
				x.Calculate(person.WorkflowControlSet, VirtualSchedulePeriodProvider.PreferencePeriod, person.PersonPeriodCollection))
			                     .Return(date);

			var target = new VirtualSchedulePeriodProvider(personProvider, defaultDateCalculator);

			var result = target.CalculatePreferenceDefaultDate();

			result.Should().Be.EqualTo(date);
		}
	}
}