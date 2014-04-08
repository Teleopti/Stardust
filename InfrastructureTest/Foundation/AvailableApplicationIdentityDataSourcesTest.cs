using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class AvailableApplicationIdentityDataSourcesTest
	{
		private IAvailableApplicationTokenDataSource target;
		private IRepositoryFactory repositoryFactory;

		[SetUp]
		public void Setup()
		{
			repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			target = new AvailableApplicationIdentityDataSource(repositoryFactory);
		}

		[Test]
		public void ShouldReturnTrueWhenUserExistsInDatasource()
		{
			var correctDs = MockRepository.GenerateMock<IDataSource>();
			var uowFactory1 = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow1 = MockRepository.GenerateMock<IUnitOfWork>();
			var personRep1 = MockRepository.GenerateMock<IPersonRepository>();

			correctDs.Stub(x => x.Application).Return(uowFactory1);
			uowFactory1.Stub(x =>x.CreateAndOpenUnitOfWork()).Return(uow1);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow1)).Return(personRep1);

			personRep1.Stub(x => x.TryFindBasicAuthenticatedPerson("roger")).Return(new Person());

			var res = target.IsDataSourceAvailable(correctDs, "roger");
			res.Should().Be.True();
		}
	}
}