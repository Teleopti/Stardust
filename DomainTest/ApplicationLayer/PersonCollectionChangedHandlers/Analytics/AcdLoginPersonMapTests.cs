using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[Ignore]
	public class AcdLoginPersonMapTests
	{
		private FakeAnalyticsPersonPeriodRepository fakeAnalyticsPersonPeriodRepository;
		private AcdLoginPersonTransformer _acdLoginPersonTransformer;

		[SetUp]
		public void SetupTests()
		{
			fakeAnalyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository(
				new DateTime(2015, 01, 01),
				new DateTime(2017, 12, 31));

			_acdLoginPersonTransformer = new AcdLoginPersonTransformer(fakeAnalyticsPersonPeriodRepository);

			fakeAnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 1 });
			fakeAnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 2 });
			fakeAnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 3 });
			fakeAnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 2, PersonId = 1 });
			fakeAnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 3, PersonId = -1 });
		}

		[Test]
		public void CompletlyNewAcdLoginForPersonBridge_Add_NewBridgeCreated()
		{
			var newBridge = new AnalyticsBridgeAcdLoginPerson
			{
				AcdLoginId = 4,
				PersonId = 4
			};
			fakeAnalyticsPersonPeriodRepository.AddBridgeAcdLoginPerson(newBridge);
			var bridge1 = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(4);
			var bridge2 = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(4);
			Assert.AreEqual(bridge1, bridge2);
		}

		[Test]
		public void DeleteAcdLoginForPerson_LastRowOnAcdLogin_OneNotDefinedRowForAcdLogin()
		{
			_acdLoginPersonTransformer.DeleteAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 2, PersonId = 1});

			var bridgeRowsForAcdLogin = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(2);
			Assert.AreEqual(1, bridgeRowsForAcdLogin.Count);
			Assert.AreEqual(2, bridgeRowsForAcdLogin.First().AcdLoginId);
			Assert.AreEqual(-1, bridgeRowsForAcdLogin.First().PersonId);
		}

		[Test]
		public void DeleteAcdLoginForPerson_NotLastRowOnAcdLogin_NoNotDefinedRowForAcdLogin()
		{
			_acdLoginPersonTransformer.DeleteAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 1});
			var bridgeRowsForAcdLogin = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(1);
			Assert.AreEqual(0, bridgeRowsForAcdLogin.Select(a => a.PersonId == -1));
		}

		[Test]
		public void NewAcdLoginForPerson_FirstRowForAcdLogin_OneNotDefinedRowForAcdLogin()
		{
			_acdLoginPersonTransformer.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 3, PersonId = 1});
			var bridgeRowsForAcdLogin = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(3);
			Assert.AreEqual(1, bridgeRowsForAcdLogin.Count);
			Assert.AreEqual(3, bridgeRowsForAcdLogin.First().AcdLoginId);
			Assert.AreEqual(1, bridgeRowsForAcdLogin.First().PersonId);
		}

		[Test]
		public void NewAcdLoginForPerson_SecondRowForAcdLogin_TwoRowsForAcdLoginNoNotDefinedRow()
		{
			_acdLoginPersonTransformer.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 2, PersonId = 3 });
			var bridgeRowsForAcdLogin = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(2);
			Assert.AreEqual(2, bridgeRowsForAcdLogin.Count);
			Assert.AreEqual(0, bridgeRowsForAcdLogin.Select(a => a.PersonId == -1));
		}

		[Test]
		public void NewAcdLoginForPerson_DuplicateRow_SameRowsAsBefore()
		{
			var numberOfRowsBefore = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(1).Count;
			_acdLoginPersonTransformer.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 1, PersonId = 1 });
			var bridgeRowsForAcdLogin = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForAcdLoginPersons(1);
			Assert.AreEqual(numberOfRowsBefore, bridgeRowsForAcdLogin.Count);
		}

		[Test]
		public void NewAcdLoginForPerson_NewRow_AnotherRowForPerson()
		{
			var numberOfRowsBefore = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(1).Count;
			_acdLoginPersonTransformer.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 4, PersonId = 1});
			var bridgeRowsForAcdLogin = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(1);
			Assert.AreEqual(numberOfRowsBefore+1, bridgeRowsForAcdLogin.Count);
		}

		[Test]
		public void DeleteAcdLoginForPerson_DeleteRow_OneLessBridgeRowForPerson()
		{
			var numberOfRowsBefore = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(1).Count;
			_acdLoginPersonTransformer.DeleteAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson { AcdLoginId = 4, PersonId = 1 });
			var bridgeRowsForAcdLogin = fakeAnalyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(1);
			Assert.AreEqual(numberOfRowsBefore - 1, bridgeRowsForAcdLogin.Count);
		}
	}
}