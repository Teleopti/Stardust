using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Reporting.Core
{
	[TestFixture]
	public class ApplicationPermissionProviderTest
	{
		private IApplicationPermissionProvider target;
		private IPersonRepository _personRepository;
		private ISiteRepository _siteRepository;
		private INow _now;
		private ICurrentDataSource _currentDataSource;
		private IApplicationFunctionResolver _applicationFunctionResolver;
		private Guid personId;
		

		[SetUp]
		public void Setup()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			_now = MockRepository.GenerateMock<INow>();
			_currentDataSource = MockRepository.GenerateMock<ICurrentDataSource>();
			_applicationFunctionResolver = MockRepository.GenerateMock<IApplicationFunctionResolver>();
			personId = Guid.NewGuid();
			target = new ApplicationPermissionProvider(_personRepository, _currentDataSource, _siteRepository, _now, _applicationFunctionResolver);
		}

		[Test]
		public void ReturnsNoPermissionsWhenThereAreNone()
		{
			var fakeDataSource = new FakeDataSource();

			_siteRepository.Stub(r => r.LoadAll()).Return(new ISite[] { });
			_personRepository.Stub(r => r.Get(personId)).Return(new Person());
			_now.Stub(r => r.UtcDateTime()).Return(DateTime.UtcNow);
			_applicationFunctionResolver.Stub(
				r =>
					r.ResolveApplicationFunction(Arg<HashSet<MatrixPermissionHolder>>.Is.Anything, Arg<IApplicationRole>.Is.Anything,
						Arg<IUnitOfWorkFactory>.Is.Anything)).Return(new HashSet<MatrixPermissionHolder>());
			_currentDataSource.Stub(r => r.Current()).Return(fakeDataSource);

			var result = target.GetPermissions(personId);

			result.Should().Be.Empty();
		}

		[Test]
		public void ReturnsPermissions()
		{
			var now = DateTime.UtcNow;
			var dateOnlyNow = new DateOnly(now);
			var person = PersonFactory.AddApplicationRolesAndFunctions(PersonFactory.CreatePersonWithPersonPeriodTeamSite(dateOnlyNow));
			person.SetId(personId);
			var fakeDataSource = new FakeDataSource {Application = new FakeUnitOfWorkFactory {Name = "TestUnitOfWork"} };
			_siteRepository.Stub(r => r.LoadAll()).Return(new[] { person.MyTeam(dateOnlyNow).Site});
			_personRepository.Stub(r => r.Get(personId)).Return(person);
			_applicationFunctionResolver.Stub(
				r =>
					r.ResolveApplicationFunction(Arg<HashSet<MatrixPermissionHolder>>.Is.Anything, Arg<IApplicationRole>.Is.Anything,
						Arg<IUnitOfWorkFactory>.Is.Anything)).Return(new HashSet<MatrixPermissionHolder> {new MatrixPermissionHolder(person, person.MyTeam(dateOnlyNow), true)});
			_now.Stub(r => r.UtcDateTime()).Return(now);
			
			_currentDataSource.Stub(r => r.Current()).Return(fakeDataSource);

			var result = target.GetPermissions(personId);

			result.Should().Not.Be.Empty();
		}
	}
}