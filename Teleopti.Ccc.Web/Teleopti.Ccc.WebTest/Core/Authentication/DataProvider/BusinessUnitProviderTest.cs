using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.DataProvider
{
	[TestFixture]
	public class BusinessUnitProviderTest
	{
		private IPerson person;
		private IPermissionInformation permissionInformation;
		private MockRepository mocks;
		private IBusinessUnitProvider target;
		private IDataSource dataSource;
		private IRepositoryFactory repositoryFactory;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();
			//don't really like this one, but logic is too much "inside" - easier to mock
			person = mocks.DynamicMock<IPerson>();
			permissionInformation = mocks.StrictMock<IPermissionInformation>();
			dataSource = mocks.StrictMock<IDataSource>();
			target = new BusinessUnitProvider(repositoryFactory);
		}

	
		[Test]
		public void ShouldReturnPersonsBusinessUnitCollectionIfNotSystemUser()
		{
			var businessUnits = new List<IBusinessUnit>();

			using(mocks.Record())
			{
				Expect.Call(person.PermissionInformation)
					.Return(permissionInformation);
				Expect.Call(permissionInformation.HasAccessToAllBusinessUnits())
					.Return(false);
				Expect.Call(permissionInformation.BusinessUnitAccessCollection())
					.Return(businessUnits);

			}

			using(mocks.Playback())
			{
				var result =target.RetrieveBusinessUnitsForPerson(dataSource, person);

				result.Should().Be.SameInstanceAs(businessUnits);
			}
		}

		[Test]
		public void ShouldReturnAllBusinessUnitsInDataSourceIfSystemUser()
		{
			var businessUnits = new List<IBusinessUnit>();
			var unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var businessUnitRepository = mocks.DynamicMock<IBusinessUnitRepository>();

			using (mocks.Record())
			{
				Expect.Call(person.PermissionInformation)
					.Return(permissionInformation);
				Expect.Call(permissionInformation.HasAccessToAllBusinessUnits())
					.Return(true);
				Expect.Call(dataSource.Application)
					.Return(unitOfWorkFactory);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork())
					.Return(uow);
				Expect.Call(repositoryFactory.CreateBusinessUnitRepository(uow))
					.Return(businessUnitRepository);
				Expect.Call(businessUnitRepository.LoadAllBusinessUnitSortedByName())
					.Return(businessUnits);
			}

			using (mocks.Playback())
			{
				var result = target.RetrieveBusinessUnitsForPerson(dataSource, person);

				result.Should().Be.SameInstanceAs(businessUnits);
			}
		}
	}
}