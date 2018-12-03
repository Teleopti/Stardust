using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;


namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class PreferenceProviderTest
	{
		
		[Test]
		public void ShouldGetPreferenceForDate()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			var preferenceDay = new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction());

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, person)).Return(new[] {preferenceDay});

			var target = new PreferenceProvider(preferenceDayRepository, loggedOnUser);

			var result = target.GetPreferencesForDate(DateOnly.Today);

			result.Should().Be(preferenceDay);
		}

		[Test]
		public void ShouldReturnNullIfNoneFound()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();

			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new IPreferenceDay[] { });

			var target = new PreferenceProvider(preferenceDayRepository, MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.GetPreferencesForDate(DateOnly.Today);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetPreferencesForPeriod()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			var preferenceDays = new[] {new PreferenceDay(person, DateOnly.Today, new PreferenceRestriction())};
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1));

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(period, new List<IPerson>() {person})).Return(preferenceDays);

			var target = new PreferenceProvider(preferenceDayRepository, loggedOnUser);

			var result = target.GetPreferencesForPeriod(period);

			result.Should().Be.SameInstanceAs(preferenceDays);
		}

	}
}