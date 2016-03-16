using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    public class PersonalSettingDataRepositoryTest : DatabaseTest
    {
        //dessa har nyckel "rätt"
        private ISettingData personalForPerson;
        private ISettingDataRepository rep;
        private testData persValue;
        private testData defaultValue;

        [Test]
        public void VerifyPersonalGiveHit()
        {
            setupFields();
            Assert.AreEqual(personalForPerson, ((ISettingValue)rep.FindValueByKey("rätt", defaultValue)).BelongsTo);
        }

        [Test]
        public void VerifyGlobalGiveHit()
        {
            setupFields();
            ISettingData s = new GlobalSettingData("ny");
            PersistAndRemoveFromUnitOfWork(s);
            Assert.AreEqual(s, ((ISettingValue)rep.FindValueByKey("ny", defaultValue)).BelongsTo);
        }


        [Test]
        public void VerifyNotExistingKeyGivesBackNewPersonal()
        {
            setupFields();
            testData s = rep.FindValueByKey("not existing", defaultValue);
            Assert.AreEqual(defaultValue.Data, s.Data);
            Assert.AreEqual("not existing", ((ISettingValue)s).BelongsTo.Key);
            Assert.IsAssignableFrom<PersonalSettingData>(((ISettingValue)s).BelongsTo);
        }

        [Test]
        public void VerifyFindValueByKeyWhenNew()
        {
            setupFields();

            testData s = rep.FindValueByKey("roger", defaultValue);
            s.Data = 44;
            Assert.AreEqual(((ISettingValue)s).BelongsTo, rep.PersistSettingValue(s));
            Session.Flush();
            Session.Clear();
            Assert.AreEqual(44, rep.FindValueByKey("roger", defaultValue).Data);
        }


        [Test]
        public void VerifyFindKeyValueByKeyWhenNew()
        {
            setupFields();

            testData s = rep.FindValueByKey("roger", defaultValue);
            s.Data = 44;
            Assert.AreEqual(((ISettingValue)s).BelongsTo, rep.PersistSettingValue(s));
            Session.Flush();
            Session.Clear();
            Assert.AreEqual(44, rep.FindValueByKey("roger", defaultValue).Data);
        }

        [Test]
        public void VerifyPersistSettingValueWhenOld()
        {
            setupFields();
            persValue.Data = 45;
            rep.PersistSettingValue(persValue);
            Session.Flush();
            Session.Clear();
            Assert.AreEqual(45, rep.FindValueByKey(personalForPerson.Key, defaultValue).Data);
        }

        [Test]
        public void SimulateTwoThreadsCreatingNewPersonalSettingLastShouldWin()
        {
            defaultValue = new testData();
            CleanUpAfterTest();
            rep = new PersonalSettingDataRepository(UnitOfWork);
            testData looser = rep.FindValueByKey("roger", defaultValue);
            looser.Data = -100;
            testData winner = rep.FindValueByKey("roger", defaultValue);
            winner.Data = 100;
            try
            {
                rep.PersistSettingValue(looser);
                UnitOfWork.PersistAll();

                using (IUnitOfWork secondUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    rep = new PersonalSettingDataRepository(secondUow);
                    rep.PersistSettingValue(winner);
                    secondUow.PersistAll();
                }
                using (IUnitOfWork readUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    rep = new PersonalSettingDataRepository(readUow);
                    testData read = rep.FindValueByKey("roger", defaultValue);
                    Assert.AreEqual(winner.Data, read.Data);
                    Assert.AreEqual(((ISettingValue)winner).BelongsTo, ((ISettingValue)read).BelongsTo);
                }

            }
            finally
            {
                using (IUnitOfWork cleanUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    cleanUow.FetchSession().Delete(((ISettingValue)winner).BelongsTo);
                    cleanUow.PersistAll();
                }
            }

        }

        [Test]
        public void SimulateTwoThreadsCreatingNewPersonalSettingLastShouldWinKey()
        {
            defaultValue = new testData();
            CleanUpAfterTest();
            rep = new PersonalSettingDataRepository(UnitOfWork);
            testData looser = rep.FindValueByKey("roger", defaultValue);
            looser.Data = -100;
            testData winner = rep.FindValueByKey("roger", defaultValue);
            winner.Data = 100;
            try
            {
                rep.PersistSettingValue("roger",looser);
                UnitOfWork.PersistAll();

                using (IUnitOfWork secondUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    rep = new PersonalSettingDataRepository(secondUow);
                    rep.PersistSettingValue("roger",winner);
                    secondUow.PersistAll();
                }
                using (IUnitOfWork readUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    rep = new PersonalSettingDataRepository(readUow);
                    testData read = rep.FindValueByKey("roger", defaultValue);
                    Assert.AreEqual(winner.Data, read.Data);
                    Assert.AreEqual(((ISettingValue)winner).BelongsTo, ((ISettingValue)read).BelongsTo);
                }

            }
            finally
            {
                using (IUnitOfWork cleanUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    cleanUow.FetchSession().Delete(((ISettingValue)winner).BelongsTo);
                    cleanUow.PersistAll();
                }
            }

        }

        [Test]
        public void VerifyLargeDataAmountPersonalData()
        {
            var data = new largeData(20000);
            rep = new PersonalSettingDataRepository(UnitOfWork);
            var corr = rep.FindValueByKey("theOne", data);
            rep.PersistSettingValue(corr);
            Session.Flush();
            UnitOfWork.Clear();
            var loaded = rep.FindValueByKey("theOne", new largeData(1));
            Assert.AreEqual(20000, loaded.ByteSize());
            Assert.IsTrue(loaded.LastByte());
        }

       

        [Serializable]
        private class largeData : SettingValue
        {
            private readonly bool[] boolArr;
            private readonly int noOfBits;

            public largeData(int bytes)
            {
                noOfBits = bytes*8;
                boolArr = new bool[noOfBits];
                boolArr[noOfBits-1] = true;
            }

            public int ByteSize()
            {
                return boolArr.Length/8;
            }

            public bool LastByte()
            {
                return boolArr[noOfBits - 1];
            }
        }

		[Test]
		public void VerifyFindValueByKeyAndPerson()
		{
			setupFieldsForExternalPerson();

			var rep = new PersonalSettingDataRepository(UnitOfWork);
			testData s = rep.FindValueByKeyAndOwnerPerson("rätt", TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(new ThisUnitOfWork(UnitOfWork))), new testData());
			Assert.AreEqual(((ISettingValue)s).BelongsTo, rep.PersistSettingValue(s));
			Session.Flush();
			Session.Clear();
			Assert.AreEqual(44, rep.FindValueByKey("rätt", defaultValue).Data);
		}

		[Test]
		public void VerifyNotFindValueByKeyAndPersonThenUseDefault()
		{
			setupFieldsForExternalPerson();

			var rep = new PersonalSettingDataRepository(UnitOfWork);
			testData s = rep.FindValueByKeyAndOwnerPerson("idonotexist", TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(new ThisUnitOfWork(UnitOfWork))), new testData { Data = -1});

			s.Data.Should().Be.EqualTo(-1);
		}


        [Serializable]
        private class testData : SettingValue
        {
            public int Data { get; set; }
        }


        private void setupFields()
        {
            rep = new PersonalSettingDataRepository(UnitOfWork);
            defaultValue = new testData { Data = 435 };
            persValue = rep.FindValueByKey("rätt", defaultValue);
            persValue.Data = 33;
            rep.PersistSettingValue(persValue);

            ISettingData global = new GlobalSettingData("rätt");
            global.SetValue(new testData {Data = 44});
            PersistAndRemoveFromUnitOfWork(global);

            Session.Flush();
            personalForPerson = ((ISettingValue)persValue).BelongsTo;
        }

		private void setupFieldsForExternalPerson()
		{
			rep = new PersonalSettingDataRepository(UnitOfWork);
			var person = TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(new ThisUnitOfWork(UnitOfWork)));
			ISettingData personalSettingData = new PersonalSettingData("rätt", person);
			personalSettingData.SetValue(new testData { Data = 44 });
			PersistAndRemoveFromUnitOfWork(personalSettingData);

			Session.Flush();
		}
    }
}
