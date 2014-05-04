using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class AvailableWindowsDataSourcesTest
	{
		private IAvailableIdentityDataSources target;
		private MockRepository mocks;
		private IRepositoryFactory repositoryFactory;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();
			target = new AvailableIdentityDataSources(repositoryFactory);
		}

		[Test]
		public void ShouldReturnCorrectDataSources()
		{
			var correctDs = mocks.DynamicMock<IDataSource>();
			var incorrectDs = mocks.DynamicMock<IDataSource>();
			var uowFactory1 = mocks.DynamicMock<IUnitOfWorkFactory>();
			var uow1 = mocks.DynamicMock<IUnitOfWork>();
			var personRep1 = mocks.DynamicMock<IPersonRepository>();
			var uowFactory2 = mocks.DynamicMock<IUnitOfWorkFactory>();
			var uow2 = mocks.DynamicMock<IUnitOfWork>();
			var personRep2 = mocks.DynamicMock<IPersonRepository>();

			using(mocks.Record())
			{
				Expect.Call(correctDs.Application).Return(uowFactory1);
				Expect.Call(uowFactory1.CreateAndOpenUnitOfWork()).Return(uow1);
				Expect.Call(repositoryFactory.CreatePersonRepository(uow1)).Return(personRep1);

				Expect.Call(incorrectDs.Application).Return(uowFactory2);
				Expect.Call(uowFactory2.CreateAndOpenUnitOfWork()).Return(uow2);
				Expect.Call(repositoryFactory.CreatePersonRepository(uow2)).Return(personRep2);

				Expect.Call(personRep1.DoesIdentityExists(@"roger\moore")).Return(true);
				Expect.Call(personRep2.DoesIdentityExists(@"roger\moore")).Return(false);
			}

			using(mocks.Playback())
			{
				var res = target.AvailableDataSources(new[] {correctDs, incorrectDs}, @"roger\moore");
				res.Should().Have.SameValuesAs(new[] {correctDs});
			}
		}
	}
}