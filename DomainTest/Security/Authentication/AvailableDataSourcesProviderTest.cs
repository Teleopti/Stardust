using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
	[TestFixture]
	public class AvailableDataSourcesProviderTest
	{
		private IAvailableDataSourcesProvider target;
		private IApplicationData applicationData;

		[SetUp]
		public void Setup()
		{
			applicationData = MockRepository.GenerateMock<IApplicationData>();
			target = new AvailableDataSourcesProvider(new ThisApplicationData(applicationData));
		}

		[Test]
		public void VerifyCanGetAvailableDataSources()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory =  MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			
			applicationData.Stub(x => x.DataSource("Teleopti WFM")).Return(dataSource);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWork.Stub(x => x.Dispose());
			
			var result = target.AvailableDataSources();
			Assert.AreEqual(1, result.Count());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), Test]
		public void VerifyCanGetUnavailableDataSources()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();

			applicationData.Stub(x => x.DataSource("Teleopti WFM")).Return(dataSource);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Throw(new Exception("test",
																			SqlExceptionConstructor.
																			CreateSqlException("Error message", 4060)));
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);

			var result = target.UnavailableDataSources();
			Assert.AreEqual(1, result.Count());
		}
	}
}