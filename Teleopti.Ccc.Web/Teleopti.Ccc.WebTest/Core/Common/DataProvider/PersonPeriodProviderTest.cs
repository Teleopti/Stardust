using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class PersonPeriodProviderTest
	{
		[Test]
		public void ShouldReturnTrueIfUserHasPersonPeriod()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var date = DateOnly.Today;

			personProvider.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(date, date))).Return(new ReadOnlyCollectionBuilder<IPersonPeriod>(new[] {MockRepository.GenerateMock<IPersonPeriod>()}));

			var target = new PersonPeriodProvider(personProvider);

			var result = target.HasPersonPeriod(date);

			result.Should().Be.True();
		}

		[Test]
		public void ShouldReturnFalseIfUserHasNoPersonPeriod()
		{
			var personProvider = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var date = DateOnly.Today;

			personProvider.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(date, date))).Return(new ReadOnlyCollectionBuilder<IPersonPeriod>());

			var target = new PersonPeriodProvider(personProvider);

			var result = target.HasPersonPeriod(date);

			result.Should().Be.False();
		}
	}
}
