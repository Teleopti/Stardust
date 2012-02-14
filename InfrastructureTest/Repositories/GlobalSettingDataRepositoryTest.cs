using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

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
