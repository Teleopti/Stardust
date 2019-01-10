using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
    public class PersonalSettingDataRepositoryTest
    {
        //dessa har nyckel "rätt"
        public ISettingDataRepository Rep;
	    public IPersonalSettingDataRepository PersonalSettingData;
	    public IPersonRepository Persons;
		public WithUnitOfWork WithUnitOfWork;

		private ISettingData _personalForPerson;
		private testData persValue;
        private testData defaultValue;

        [Test]
        public void VerifyPersonalGiveHit()
        {
            setupFields();
	        var result = WithUnitOfWork.Get(() => Rep.FindValueByKey("rätt", defaultValue));
            Assert.AreEqual(_personalForPerson, ((ISettingValue)result).BelongsTo);
        }

        [Test]
        public void VerifyGlobalGiveHit()
        {
            setupFields();
			ISettingData s = new GlobalSettingData("ny");
			s.SetValue(defaultValue);
			WithUnitOfWork.Do(() => s = Rep.PersistSettingValue("ny", s.GetValue(defaultValue)));

	        var result = WithUnitOfWork.Get(() => Rep.FindValueByKey("ny", defaultValue));
            Assert.AreEqual(s, ((ISettingValue)result).BelongsTo);
        }


        [Test]
        public void VerifyNotExistingKeyGivesBackNewPersonal()
        {
            setupFields();
            testData s = WithUnitOfWork.Get(() => Rep.FindValueByKey("not existing", defaultValue));
            Assert.AreEqual(defaultValue.Data, s.Data);
            Assert.AreEqual("not existing", ((ISettingValue)s).BelongsTo.Key);
            Assert.IsAssignableFrom<PersonalSettingData>(((ISettingValue)s).BelongsTo);
        }

        [Test]
        public void VerifyFindValueByKeyWhenNew()
        {
            setupFields();

            testData s = WithUnitOfWork.Get(() => Rep.FindValueByKey("roger", defaultValue));
            s.Data = 44;
	        WithUnitOfWork.Do(() => Rep.PersistSettingValue(s));
	        var result = WithUnitOfWork.Get(() => Rep.FindValueByKey("roger", defaultValue).Data);
            Assert.AreEqual(44, result);
        }


        [Test]
        public void VerifyFindKeyValueByKeyWhenNew()
        {
            setupFields();

            testData s = WithUnitOfWork.Get(() => Rep.FindValueByKey("roger", defaultValue));
            s.Data = 44;
	        WithUnitOfWork.Do(() => Rep.PersistSettingValue(s));
	        var result = WithUnitOfWork.Get(() => Rep.FindValueByKey("roger", defaultValue));
            Assert.AreEqual(44, result.Data);
        }

        [Test]
        public void VerifyPersistSettingValueWhenOld()
        {
            setupFields();
            persValue.Data = 45;
            WithUnitOfWork.Do(() => Rep.PersistSettingValue(persValue));
	        var result = WithUnitOfWork.Get(() => Rep.FindValueByKey(_personalForPerson.Key, defaultValue).Data);
            Assert.AreEqual(45, result);
        }

        [Test]
        public void SimulateTwoThreadsCreatingNewPersonalSettingLastShouldWin()
        {
            defaultValue = new testData();
            testData looser = WithUnitOfWork.Get(() => Rep.FindValueByKey("roger", defaultValue));
            looser.Data = -100;
            testData winner = WithUnitOfWork.Get(() => Rep.FindValueByKey("roger", defaultValue));
            winner.Data = 100;

			WithUnitOfWork.Do(() => Rep.PersistSettingValue(looser));

			WithUnitOfWork.Do(() => Rep.PersistSettingValue(winner));
			WithUnitOfWork.Do(() =>
			{
				testData read = Rep.FindValueByKey("roger", defaultValue);
				Assert.AreEqual(winner.Data, read.Data);
				Assert.AreEqual(((ISettingValue)winner).BelongsTo, ((ISettingValue)read).BelongsTo);
			});

        }

        [Test]
        public void SimulateTwoThreadsCreatingNewPersonalSettingLastShouldWinKey()
        {
            defaultValue = new testData();
            testData looser = WithUnitOfWork.Get(() => Rep.FindValueByKey("roger", defaultValue));
            looser.Data = -100;
            testData winner = WithUnitOfWork.Get(() => Rep.FindValueByKey("roger", defaultValue));
            winner.Data = 100;

			WithUnitOfWork.Do(() => Rep.PersistSettingValue("roger", looser));
			WithUnitOfWork.Do(() => Rep.PersistSettingValue("roger", winner));
			WithUnitOfWork.Do(() =>
			{
				testData read = Rep.FindValueByKey("roger", defaultValue);
				Assert.AreEqual(winner.Data, read.Data);
				Assert.AreEqual(((ISettingValue)winner).BelongsTo, ((ISettingValue)read).BelongsTo);
			});
        }

        [Test]
        public void VerifyLargeDataAmountPersonalData()
        {
            var data = new largeData(20000);
            var corr = WithUnitOfWork.Get(() => Rep.FindValueByKey("theOne", data));
            WithUnitOfWork.Do(() => Rep.PersistSettingValue(corr));

			var loaded = WithUnitOfWork.Get(() => Rep.FindValueByKey("theOne", new largeData(1)));
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
			
			testData s = WithUnitOfWork.Get(() =>
				PersonalSettingData.FindValueByKeyAndOwnerPerson("rätt", Persons.Get(TeleoptiPrincipal.CurrentPrincipal.PersonId), new testData()));
			var result1 = WithUnitOfWork.Get(() => PersonalSettingData.PersistSettingValue(s));
			Assert.AreEqual(((ISettingValue)s).BelongsTo, result1);
			var result2 = WithUnitOfWork.Get(() => PersonalSettingData.FindValueByKey("rätt", defaultValue).Data);
			Assert.AreEqual(44, result2);
		}

		[Test]
		public void VerifyNotFindValueByKeyAndPersonThenUseDefault()
		{
			setupFieldsForExternalPerson();
			
			testData s = WithUnitOfWork.Get(() => 
				PersonalSettingData.FindValueByKeyAndOwnerPerson("idonotexist", Persons.Get(TeleoptiPrincipal.CurrentPrincipal.PersonId), new testData { Data = -1 }));

			s.Data.Should().Be.EqualTo(-1);
		}


        [Serializable]
        private class testData : SettingValue
        {
            public int Data { get; set; }
        }


        private void setupFields()
        {
            defaultValue = new testData { Data = 435 };
            persValue = WithUnitOfWork.Get(() => Rep.FindValueByKey("rätt", defaultValue));
            persValue.Data = 33;
            WithUnitOfWork.Do(() => Rep.PersistSettingValue(persValue));

			ISettingData global = new GlobalSettingData("rätt");
			global.SetValue(new testData { Data = 44 });
			WithUnitOfWork.Do(() => Rep.PersistSettingValue("rätt", global.GetValue(defaultValue)));

            _personalForPerson = ((ISettingValue)persValue).BelongsTo;
        }

		private void setupFieldsForExternalPerson()
		{
			var person = WithUnitOfWork.Get(() => Persons.Get(TeleoptiPrincipal.CurrentPrincipal.PersonId));
			ISettingData personalSettingData = new PersonalSettingData("rätt", person);
			personalSettingData.SetValue(new testData { Data = 44 });
			WithUnitOfWork.Do(() => Rep.PersistSettingValue(personalSettingData.GetValue(defaultValue)));
		}
    }
}
