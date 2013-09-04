using System;
using System.Globalization;
using System.Linq.Expressions;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;

namespace Teleopti.Ccc.WebTest.Core.Settings
{
	[TestFixture]
	public class PersonPersisterTest
	{
		[Test]
		public void ShouldUpdateCulture()
		{
			var person = new Person();
			var target = new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), MockRepository.GenerateMock<IMakeRegionalFromPerson>());

			target.UpdateCulture(person, CultureInfo.GetCultureInfo("en-GB"));

			person.PermissionInformation.Culture().Should().Be(CultureInfo.GetCultureInfo("en-GB"));
		}

		[Test]
		public void ShouldInvalidateRegionalCacheWhenUpdatingCulture()
		{
			var cacheFactory = MockRepository.GenerateMock<IMbCacheFactory>();
			var makeRegionalFromPerson = MockRepository.GenerateMock<IMakeRegionalFromPerson>();
			var person = new Person();
			var target = new PersonPersister(cacheFactory, makeRegionalFromPerson);

			target.UpdateCulture(person, CultureInfo.CurrentCulture);

			// would be simpler in moq...
			cacheFactory.AssertWasCalled(x =>
			                             x.Invalidate(
			                             	Arg<IMakeRegionalFromPerson>.Is.Same(makeRegionalFromPerson),
			                             	Arg<Expression<Func<IMakeRegionalFromPerson, object>>>.Is.NotNull,
			                             	Arg<bool>.Is.Equal(true)
			                             	));
		}
		[Test]
		public void ShouldUpdateUICulture()
		{
			var person = new Person();
			var target = new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), MockRepository.GenerateMock<IMakeRegionalFromPerson>());

			target.UpdateUICulture(person, CultureInfo.GetCultureInfo("en-GB"));

			person.PermissionInformation.UICulture().Should().Be(CultureInfo.GetCultureInfo("en-GB"));
		}

		[Test]
		public void ShouldInvalidateRegionalCacheWhenUpdatingUICulture()
		{
			var cacheFactory = MockRepository.GenerateMock<IMbCacheFactory>();
			var makeRegionalFromPerson = MockRepository.GenerateMock<IMakeRegionalFromPerson>();
			var person = new Person();
			var target = new PersonPersister(cacheFactory, makeRegionalFromPerson);

			target.UpdateUICulture(person, CultureInfo.CurrentCulture);

			// would be simpler in moq...
			cacheFactory.AssertWasCalled(x =>
			                             x.Invalidate(
			                             	Arg<IMakeRegionalFromPerson>.Is.Same(makeRegionalFromPerson),
			                             	Arg<Expression<Func<IMakeRegionalFromPerson, object>>>.Is.NotNull,
			                             	Arg<bool>.Is.Equal(true)
			                             	));
		}

	}
}