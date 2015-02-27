//namespace Teleopti.Ccc.Sdk.LogicTest
//{
	// we don't bother to test this it will be removed completely as soon as possible
//	[TestFixture]
//	public class AvailableDataSourcesProviderTest
//	{
//		private IAvailableDataSourcesProvider target;
//		private MockRepository mocks;
//		private IApplicationData applicationData;

//		[SetUp]
//		public void Setup()
//		{
//			mocks = new MockRepository();
//			applicationData = mocks.StrictMock<IApplicationData>();
//			target = new AvailableDataSourcesProvider(new ThisApplicationData(applicationData));
//		}

//		[Test]
//		public void VerifyCanGetAvailableDataSources()
//		{
//			var dataSource = mocks.StrictMock<IDataSource>();
//			var dataSources = new List<IDataSource> { dataSource };
//			var unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
//			var unitOfWork = mocks.StrictMock<IUnitOfWork>();
//			using (mocks.Record())
//			{

//				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
//				Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
//				Expect.Call(unitOfWork.Dispose);
//			}
//			using (mocks.Playback())
//			{
//				var result = target.AvailableDataSources();
//				Assert.AreEqual(1, result.Count());
//			}
//		}

//		[Test]
//		public void VerifyCanGetUnavailableDataSources()
//		{
//			var dataSource = mocks.StrictMock<IDataSource>();
//			var dataSources = new List<IDataSource> { dataSource };
//			var unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
//			using (mocks.Record())
//			{
//#pragma warning disable 618
//				Expect.Call(applicationData.RegisteredDataSourceCollection).Return(dataSources);
//#pragma warning restore 618
//				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Throw(new Exception("test",
//																							 SqlExceptionConstructor.
//																								CreateSqlException("Error message", 4060)));
//				Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
//			}
//			using (mocks.Playback())
//			{
//				var result = target.UnavailableDataSources();
//				Assert.AreEqual(1, result.Count());
//			}
//		}
//	}
//}