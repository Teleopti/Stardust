using System;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    public class GlobalSettingDataRepositoryTest :  DatabaseTest
    {
        private ISettingDataRepository rep;

        protected override void SetupForRepositoryTest()
        {
            rep = new GlobalSettingDataRepository(UnitOfWork);
        }

        [Test]
        public void VerifyNotExistingKeyGivesBackNewGlobal()
        {
            testData s = rep.FindValueByKey("not existing", dummyValue());
            Assert.AreEqual(dummyValue().Data, s.Data);
            Assert.AreEqual("not existing", ((ISettingValue)s).BelongsTo.Key);
            Assert.IsAssignableFrom<GlobalSettingData>(((ISettingValue)s).BelongsTo);
        }

        [Test]
        public void VerifyGlobalGiveHit()
        {
            ISettingData s = new GlobalSettingData("ny");
            PersistAndRemoveFromUnitOfWork(s);
            Assert.AreEqual(s, ((ISettingValue)rep.FindValueByKey("ny", dummyValue())).BelongsTo);
        }

        [Test]
        public void VerifyFindByKey()
        {
            ISettingData s1 = new GlobalSettingData("rätt");
            s1.SetValue(new testData{Data = 44});
            ISettingData s2 = new GlobalSettingData("fel");
            s2.SetValue(new testData { Data = 67 });
            PersistAndRemoveFromUnitOfWork(s1);
            PersistAndRemoveFromUnitOfWork(s2);
            Assert.AreEqual(44, ((GlobalSettingDataRepository)rep).FindByKey("rätt").GetValue(new testData()).Data);
            Assert.IsNull(((GlobalSettingDataRepository)rep).FindByKey("non existing"));
        }

        [Test]
        public void VerifyPersistSettingValueWhenOld()
        {
            const string key ="theOne";
            testData aNewOne = rep.FindValueByKey(key, dummyValue());

            aNewOne.Data = 45;
            rep.PersistSettingValue(aNewOne);
            Session.Flush();
            Session.Clear();
            Assert.AreEqual(45, rep.FindValueByKey(key, dummyValue()).Data);
            
        }

        

        [Test]
        public void IncorrectVersionShouldSilentlyReturnDefaultValue()
        {
            const string key = "theOne";
            testData aNewOne = rep.FindValueByKey(key, dummyValue());
            aNewOne.Data = 45;
            rep.PersistSettingValue(aNewOne);
            Session.Flush();
            Session.Clear();

            testData2 secondDefault = new testData2 {Data = 14};
            testData2 result = rep.FindValueByKey(key, secondDefault);
            Assert.IsTrue(result.GetType().Equals(typeof(testData2)));
            Assert.AreEqual(14, result.Data);
        }


        //keeplaests
        [Test]
        public void VerifyPersistSettingKeyValueWhenOld()
        {
            const string key = "theOne";
            testData aNewOne = rep.FindValueByKey(key, dummyValue());

            aNewOne.Data = 45;
            rep.PersistSettingValue(key, aNewOne);
            Session.Flush();
            Session.Clear();
            Assert.AreEqual(45, rep.FindValueByKey(key, dummyValue()).Data);

        }

        [Test]
        public void IncorrectVersionShouldSilentlyReturnDefaultKeyValue()
        {
            const string key = "theOne";
            testData aNewOne = rep.FindValueByKey(key, dummyValue());
            aNewOne.Data = 45;
            rep.PersistSettingValue(key, aNewOne);

            rep.PersistSettingValue(key, aNewOne);
            Session.Flush();
            Session.Clear();

            testData2 secondDefault = new testData2 { Data = 14 };
            testData2 result = rep.FindValueByKey(key, secondDefault);
            Assert.IsTrue(result.GetType().Equals(typeof(testData2)));
            Assert.AreEqual(14, result.Data);
        }

		  [Test]
		  public void SaveGlobalSettingForTwoBU()
		  {
			  var businessUnitRepository = new BusinessUnitRepository(UnitOfWork);
			  var personRepository = new PersonRepository( new ThisUnitOfWork(UnitOfWork));
			  const string key = "SMS";

			  var businessUnit1 = BusinessUnitFactory.CreateSimpleBusinessUnit("BU1");
			  var loggedOnPerson = PersonFactory.CreatePerson("person for bu1");
			  personRepository.Add(loggedOnPerson);
			  businessUnitRepository.Add(businessUnit1);
			  changeBusinessUnit(businessUnit1, loggedOnPerson);
			  var testData1 = new testData {Data = 123};
			  testData forBu1 = rep.FindValueByKey(key, testData1);
			  rep.PersistSettingValue(key, forBu1);
			 
			  
			  Session.Flush();
			  Session.Clear();
			  Assert.AreEqual(123, rep.FindValueByKey(key, testData1).Data);

			  var businessUnit2 = BusinessUnitFactory.CreateSimpleBusinessUnit("BU2");
			  businessUnitRepository.Add(businessUnit2);
			  loggedOnPerson = PersonFactory.CreatePerson("person for bu2");
			  personRepository.Add(loggedOnPerson);
			  changeBusinessUnit(businessUnit2, loggedOnPerson);
			  var testData2 = new testData { Data = 456 };
			  testData forBu2 = rep.FindValueByKey(key, testData2);
			  rep.PersistSettingValue(key, forBu2);
			  Session.Flush();
			  Session.Clear();
			  Assert.AreEqual(456, rep.FindValueByKey(key, testData2).Data);
		  }

	    private void changeBusinessUnit(IBusinessUnit businessUnit,IPerson person)
	    {
			 var identity = new TeleoptiIdentity("test user", new FakeDataSource(), businessUnit, WindowsIdentity.GetCurrent(), null);
			 //var threadPreviousPerson = ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person;
			 var principalForTest = new TeleoptiPrincipal(identity, person);
			 Thread.CurrentPrincipal = new TeleoptiPrincipal(identity, person);
			 ((TeleoptiPrincipal)TeleoptiPrincipal.CurrentPrincipal).ChangePrincipal(principalForTest);
	    }

        private static testData dummyValue()
        {
            return new testData { Data = 123 };
        }

        [Serializable]
        private class testData : SettingValue
        {
            public int Data { get; set; }
        }

        [Serializable]
        private class testData2 : testData{}
    }
}
