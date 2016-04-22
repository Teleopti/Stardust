using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Budgeting
{
	public class AbsenceRequestBudgetGroupValidationHelperTest
	{
		[Test]
		public void ShouldReturnPersonPeriodOrBudgetGroupIsNullInUserCulture()
		{
			var cultureOne = new CultureInfo("sv-SE");
			var validRequestOne = AbsenceRequestBudgetGroupValidationHelper.PersonPeriodOrBudgetGroupIsNull(cultureOne, null);
			validRequestOne.ValidationErrors.Should()
				.Be.EqualTo("Det finns ingen budgetgrupp för dig.");

			var cultureTwo = new CultureInfo("fr-BE");
			var validRequestTwo = AbsenceRequestBudgetGroupValidationHelper.PersonPeriodOrBudgetGroupIsNull(cultureTwo, null);
			validRequestTwo.ValidationErrors.Should()
				.Be.EqualTo("Il n'y a pas de Groupe Budget pour vous.");
		}

		[Test]
		public void ShouldReturnBudgetDaysAreNullInUserCulture()
		{
			var cultureOne = new CultureInfo("sv-SE");
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2016, 4, 25), new DateOnly(2016, 4, 25));
			var validRequestOne = AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(cultureOne, dateOnlyPeriod);
			validRequestOne.ValidationErrors.Should()
				.Be.EqualTo("Det finns ingen budget för denna period 4/25/2016 - 4/25/2016");

			var cultureTwo = new CultureInfo("fr-BE");
			var validRequestTwo = AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNull(cultureTwo, dateOnlyPeriod);
			validRequestTwo.ValidationErrors.Should()
				.Be.EqualTo("Il n'y a pas de budget pour cette période 4/25/2016 - 4/25/2016");
		}

		[Test]
		public void ShouldReturnBudgetDaysAreNotEqualToRequestedPeriodDaysInUserCulture()
		{
			var cultureOne = new CultureInfo("sv-SE");
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2016, 4, 25), new DateOnly(2016, 4, 25));
			var validRequestOne = AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(cultureOne, dateOnlyPeriod);
			validRequestOne.ValidationErrors.Should()
				.Be.EqualTo("En eller flera dagar under den efterfrågade perioden 4/25/2016 - 4/25/2016 har ingen tilldelning.");

			var cultureTwo = new CultureInfo("fr-BE");
			var validRequestTwo = AbsenceRequestBudgetGroupValidationHelper.BudgetDaysAreNotEqualToRequestedPeriodDays(cultureTwo, dateOnlyPeriod);
			validRequestTwo.ValidationErrors.Should()
				.Be.EqualTo("Un jour ou plus de cette période demandée 4/25/2016 - 4/25/2016 n'a pas de permission budgetée.");
		}
	}
}