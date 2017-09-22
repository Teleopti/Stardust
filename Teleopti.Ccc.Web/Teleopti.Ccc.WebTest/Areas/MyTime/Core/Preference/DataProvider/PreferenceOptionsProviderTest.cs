using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceOptionsProviderTest
	{
		[Test]
		public void ShouldRetrieveShiftCategoriesFromWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var shiftCategory = new ShiftCategory("sc");
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
        public void ShouldOnlyRetrieveUnDeletedShiftCategoriesFromWorkflowControlSet()
        {
            var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
            var shiftCategory = new ShiftCategory("sc");
            var deletedShiftCategory = new ShiftCategory("dsc");
            deletedShiftCategory.SetDeleted();
            var person = new Person
            {
                WorkflowControlSet = new WorkflowControlSet
                {
                    AllowedPreferenceShiftCategories = new[] { shiftCategory, deletedShiftCategory }
                }
            };
            var target = new PreferenceOptionsProvider(loggedOnUser);

            loggedOnUser.Stub(u => u.CurrentUser()).Return(person);

            var result = target.RetrieveShiftCategoryOptions();

            result.Count().Should().Be(1);
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
        public void ShouldOnlyRetrieveUnDeletedDayOffTemplatesFromWorkflowControlSet()
        {
            var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
            var dayOff = new DayOffTemplate(new Description());
            var deletedDayOff = new DayOffTemplate(new Description());
            deletedDayOff.SetDeleted();

            var person = new Person
            {
                WorkflowControlSet = new WorkflowControlSet
                {
                    AllowedPreferenceDayOffs = new[] { dayOff, deletedDayOff }
                }
            };
            var target = new PreferenceOptionsProvider(loggedOnUser);

            loggedOnUser.Stub(u => u.CurrentUser()).Return(person);

            var result = target.RetrieveDayOffOptions();

            result.Count().Should().Be(1);
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
        public void ShouldOnlyRetrieveUnDeletedAbsencesFromWorkflowControlSet()
        {
            var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
            var absence = new Absence();
            var deletedAbsence = new Absence();
            deletedAbsence.SetDeleted();

            var person = new Person
            {
                WorkflowControlSet = new WorkflowControlSet
                {
                    AllowedPreferenceAbsences = new[] { absence, deletedAbsence }
                }
            };
            var target = new PreferenceOptionsProvider(loggedOnUser);

            loggedOnUser.Stub(u => u.CurrentUser()).Return(person);

            var result = target.RetrieveAbsenceOptions();

            result.Count().Should().Be(1);
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



		[Test]
		public void ShouldRetrieveActivityFromWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var activity = new Activity("a");
			var person = new Person
			{
				WorkflowControlSet = new WorkflowControlSet
				{
					AllowedPreferenceActivity = activity
				}
			};
			var target = new PreferenceOptionsProvider(loggedOnUser);

			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);

			var result = target.RetrieveActivityOptions();

			result.Single().Should().Be(activity);
		}

		[Test]
		public void ShouldReturnEmptyActivityIfNotWorkflowControlSet()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(u => u.CurrentUser()).Return(new Person());
			var target = new PreferenceOptionsProvider(loggedOnUser);

			var result = target.RetrieveActivityOptions();

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnEmptyActivityIfNotActivity()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person
			             	{
			             		WorkflowControlSet = new WorkflowControlSet
			             		                     	{
			             		                     		AllowedPreferenceActivity = null
			             		                     	}
			             	};
			loggedOnUser.Stub(u => u.CurrentUser()).Return(person);
			var target = new PreferenceOptionsProvider(loggedOnUser);

			var result = target.RetrieveActivityOptions();

			result.Should().Have.Count.EqualTo(0);
		}



	}
}