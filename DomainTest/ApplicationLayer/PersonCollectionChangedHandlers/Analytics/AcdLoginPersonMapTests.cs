using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AcdLoginPersonMapTests : ISetup
	{
		public AcdLoginPersonTransformer Target;
		public FakeAnalyticsPersonPeriodRepository AnalyticsPersonPeriodRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AcdLoginPersonTransformer>();
		}

		private void setupTests()
		{
			AnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 1 });
			AnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 2 });
			AnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 3 });
			AnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 2, PersonId = 1 });
			AnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 3, PersonId = -1 });
		}

		[Test]
		public void CompletlyNewAcdLoginForPersonBridge_Add_NewBridgeCreated()
		{
			setupTests();

			var newBridge = new AnalyticsBridgeAcdLoginPerson
			{
				AcdLoginId = 4,
				PersonId = 4
			};
			AnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(newBridge);
			var bridge1 = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(4);
			var bridge2 = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(4);
			Assert.AreEqual(bridge1, bridge2);
		}

		[Test]
		public void DeleteAcdLoginForPerson_LastRowOnAcdLogin_OneNotDefinedRowForAcdLogin()
		{
			setupTests();

			Target.DeleteAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 2, PersonId = 1 });

			var bridgeRowsForAcdLogin = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(2);
			Assert.AreEqual(1, bridgeRowsForAcdLogin.Count);
			Assert.AreEqual(2, bridgeRowsForAcdLogin.First().AcdLoginId);
			Assert.AreEqual(-1, bridgeRowsForAcdLogin.First().PersonId);
			Assert.AreEqual(AnalyticsDate.Eternity.DateDate, bridgeRowsForAcdLogin.First().DatasourceUpdateDate);
		}

		[Test]
		public void DeleteAcdLoginForPerson_NotLastRowOnAcdLogin_NoNotDefinedRowForAcdLogin()
		{
			setupTests();

			Target.DeleteAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 1 });
			var bridgeRowsForAcdLogin = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(1);
			Assert.AreEqual(0, bridgeRowsForAcdLogin.Count(a => a.PersonId == -1));
		}

		[Test]
		public void NewAcdLoginForPerson_FirstRowForAcdLogin_OneRowForAcdLogin()
		{
			setupTests();

			Target.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 3, PersonId = 1 });
			var bridgeRowsForAcdLogin = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(3);
			Assert.AreEqual(1, bridgeRowsForAcdLogin.Count);
			Assert.AreEqual(3, bridgeRowsForAcdLogin.First().AcdLoginId);
			Assert.AreEqual(1, bridgeRowsForAcdLogin.First().PersonId);
		}

		[Test]
		public void NewAcdLoginForPerson_SecondRowForAcdLogin_TwoRowsForAcdLoginNoNotDefinedRow()
		{
			setupTests();

			Target.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 2, PersonId = 3 });
			var bridgeRowsForAcdLogin = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(2);
			Assert.AreEqual(2, bridgeRowsForAcdLogin.Count);
			Assert.AreEqual(0, bridgeRowsForAcdLogin.Count(a => a.PersonId == -1));
		}

		[Test]
		public void NewAcdLoginForPerson_DuplicateRow_SameRowsAsBefore()
		{
			setupTests();

			var numberOfRowsBefore = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(1).Count;
			Target.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 1 });
			var bridgeRowsForAcdLogin = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(1);
			Assert.AreEqual(numberOfRowsBefore, bridgeRowsForAcdLogin.Count);
		}

		[Test]
		public void NewAcdLoginForPerson_NewRow_AnotherRowForPerson()
		{
			setupTests();

			var numberOfRowsBefore = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(1).Count;
			Target.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 4, PersonId = 1 });
			var bridgeRowsForAcdLogin = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(1);
			Assert.AreEqual(numberOfRowsBefore + 1, bridgeRowsForAcdLogin.Count);
		}

		[Test]
		public void DeleteAcdLoginForPerson_DeleteRow_OneLessBridgeRowForPerson()
		{
			setupTests();

			var numberOfRowsBefore = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(1).Count;
			Target.DeleteAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 2, PersonId = 1 });
			var bridgeRowsForAcdLogin = AnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(1);
			Assert.AreEqual(numberOfRowsBefore - 1, bridgeRowsForAcdLogin.Count);
		}
	}
}