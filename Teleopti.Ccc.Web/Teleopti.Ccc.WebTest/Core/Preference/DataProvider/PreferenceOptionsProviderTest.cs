using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceOptionsProviderTest
	{
		[Test]
		public void ShouldRetrieveShiftCategoriesFromWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var shiftCategory = new ShiftCategory(" ");
			var person = new Person {
			             		WorkflowControlSet = new WorkflowControlSet {
									AllowedPreferenceShiftCategories = new[] { shiftCategory }
			             		                     	}
			             	};
			var target = new PreferenceOptionsProvider(loggedOnUser);

			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);

			var result = target.RetrieveShiftCategoryOptions();

			result.Single().Should().Be(shiftCategory);
		}

		[Test]
		public void ShouldRetrieveDayOffTemplatesFromWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var dayOff = new DayOffTemplate(new Description());
			var person = new Person
			{
				WorkflowControlSet = new WorkflowControlSet
				{
					AllowedPreferenceDayOffs = new[] { dayOff }
				}
			};
			var target = new PreferenceOptionsProvider(loggedOnUser);

			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);

			var result = target.RetrieveDayOffOptions();

			result.Single().Should().Be(dayOff);
		}


		[Test]
		public void ShouldRetrieveAbsencesFromWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var absence = new Absence();
			var person = new Person
			{
				WorkflowControlSet = new WorkflowControlSet
				{
					AllowedPreferenceAbsences = new[] { absence }
				}
			};
			var target = new PreferenceOptionsProvider(loggedOnUser);

			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);

			var result = target.RetrieveAbsenceOptions();

			result.Single().Should().Be(absence);
		}

		[Test]
		public void ShouldReturnEmptyShiftCategoriesIfNotWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(u => u.CurrentUser()).Return(new Person());
			var target = new PreferenceOptionsProvider(loggedOnUser);

			var result = target.RetrieveShiftCategoryOptions();

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnEmptyDayOffIfNotWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(u => u.CurrentUser()).Return(new Person());
			var target = new PreferenceOptionsProvider(loggedOnUser);

			var result = target.RetrieveDayOffOptions();

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnEmptyAbsenceIfNotWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(u => u.CurrentUser()).Return(new Person());
			var target = new PreferenceOptionsProvider(loggedOnUser);

			var result = target.RetrieveAbsenceOptions();

			result.Should().Be.Empty();
		}

	}
}