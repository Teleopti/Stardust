using System;
using System.Collections.Generic;
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
		private MockRepository mocks;
		private IApplicationData applicationData;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			applicationData = mocks.StrictMock<IApplicationData>();
			target = new AvailableDataSourcesProvider(new ThisApplicationData(applicationData));
		}

		[Test]
		public void VerifyCanGetAvailableDataSources()
		{
			var dataSource = mocks.StrictMock<IDataSource>();
			var dataSources = new List<IDataSource> { dataSource };
			var unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
			var unitOfWork = mocks.StrictMock<IUnitOfWork>();
			using (mocks.Record())
			{
				Expect.Call(applicationData.RegisteredDataSourceCollection).Return(dataSources);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
				Expect.Call(unitOfWork.Dispose);
			}
			using (mocks.Playback())
			{
				var result = target.AvailableDataSources();
				Assert.AreEqual(1, result.Count());
			}
		}

		[Test]
		public void VerifyCanGetUnavailableDataSources()
		{
			var dataSource = mocks.StrictMock<IDataSource>();
			var dataSources = new List<IDataSource> { dataSource };
			var unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
			using (mocks.Record())
			{
				Expect.Call(applicationData.RegisteredDataSourceCollection).Return(dataSources);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Throw(new Exception("test",
																							 SqlExceptionConstructor.
																								CreateSqlException("Error message", 4060)));
				Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.UnavailableDataSources();
				Assert.AreEqual(1, result.Count());
			}
		}
	}
}