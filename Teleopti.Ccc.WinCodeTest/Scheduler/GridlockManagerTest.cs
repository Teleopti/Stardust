using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class GridlockManagerTest
    {
        GridlockManager _gridlockManager;
        Person _person;
        IList<IScheduleDay> _schedules;
        Scenario _scenario;
        IScheduleDay _schedulePart1;
        IScheduleDay _schedulePart2;
        private IScheduleDictionary dic;

        [SetUp]
        public void Setup()
        {
            _scenario = new Scenario("default");
			var currentAuthorization = CurrentAuthorization.Make();
			dic = new ScheduleDictionary(_scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 2)),
				new PersistableScheduleDataPermissionChecker(currentAuthorization),
				currentAuthorization);
            _person = new Person();
            _gridlockManager = new GridlockManager();
            _schedules = new List<IScheduleDay>();
           
            _schedulePart1 = ExtractedSchedule.CreateScheduleDay(dic, _person, new DateOnly(2000,1,1), currentAuthorization);
            _schedulePart2 = ExtractedSchedule.CreateScheduleDay(dic, _person, new DateOnly(2000, 1, 3), currentAuthorization);

            _schedules.Add(_schedulePart1);
        }

       
        [Test]
        public void CanCreateGridlockManager()
        {
            Assert.IsNotNull(_gridlockManager);
        }

        [Test]
        public void CanAddLock()
        {
            DateOnly date = new DateOnly(2006, 1, 2);
            _gridlockManager.AddLock(_person, date, LockType.Normal);
            _gridlockManager.AddLock(_schedulePart2, LockType.Normal);
            _gridlockManager.AddLock(_schedules, LockType.Normal);
            Assert.AreEqual(3, _gridlockManager.GridlocksDictionary.Count);
        }

        [Test]
        public void CanRemoveLock()
        {
            DateOnly date = new DateOnly(2006, 1, 2);
            _gridlockManager.AddLock(_person, date, LockType.Normal);
            _gridlockManager.AddLock(_schedulePart2, LockType.Normal);
            _gridlockManager.AddLock(_schedules, LockType.Normal);
            Assert.AreEqual(3, _gridlockManager.GridlocksDictionary.Count);

            _gridlockManager.RemoveLock(_person, date);
            _gridlockManager.RemoveLock(_schedulePart2);
            _gridlockManager.RemoveLock(_schedules);

            Assert.IsFalse(_gridlockManager.HasLocks);
        }

        [Test]
        public void CanGetLock()
        {
            _gridlockManager.AddLock(_schedulePart1, LockType.Normal);
            Assert.IsNotNull(_gridlockManager.Gridlocks(_schedulePart1));
        }

        [Test]
        public void ShouldReturnUnlockedDays()
        {
            var scheduleDays = new List<IScheduleDay>{_schedulePart1, _schedulePart2};
            _gridlockManager.AddLock(_schedulePart1, LockType.Normal);

            var unlockedDays = _gridlockManager.UnlockedDays(scheduleDays);

            Assert.AreEqual(1, unlockedDays.Count);
            Assert.IsTrue(unlockedDays.Contains(_schedulePart2));
        }

        [Test]
        public void VerifyNullWhenNoLockExists()
        {
            Assert.IsNull(_gridlockManager.Gridlocks(_person, new DateOnly(2007, 1, 1)));
        }

        

        [Test]
        public void CanClearAllLocks()
        {
            _gridlockManager.AddLock(_schedulePart1, LockType.Normal);
            Assert.IsTrue(_gridlockManager.HasLocks);
            _gridlockManager.Clear();
            Assert.IsFalse(_gridlockManager.HasLocks);
        }

        [Test]
        public void CanTestLockType()
        {
 
            _gridlockManager.AddLock(_schedulePart1, LockType.Normal);
            IDictionary<string, GridlockDictionary> locks = _gridlockManager.GridlocksDictionary;
            GridlockDictionary dictionary =
                locks[GridlockManager.GetPersonDateKey(_schedulePart1.Person, _schedulePart1.DateOnlyAsPeriod.DateOnly)];
            Assert.IsTrue(dictionary.HasLockType(LockType.Normal));
            Assert.IsFalse(dictionary.HasLockType(LockType.Authorization));
            _gridlockManager.Clear();
            Assert.IsFalse(dictionary.HasLockType(LockType.Normal));
        }

        [Test]
        public void CanClearAllWriteProtectionLocks()
        {
            //_gridlockManager.AddLock(_scheduleRange);
            _gridlockManager.AddLock(_schedulePart2, LockType.OutsidePersonPeriod);
            _gridlockManager.AddLock(_schedulePart2, LockType.Normal);
            _gridlockManager.AddLock(_schedulePart2, LockType.WriteProtected);

            IDictionary<string, GridlockDictionary> locks = _gridlockManager.GridlocksDictionary;
            GridlockDictionary dictionary =
                locks[GridlockManager.GetPersonDateKey(_schedulePart2.Person, _schedulePart2.DateOnlyAsPeriod.DateOnly)];

            Assert.AreEqual(3, dictionary.Count);
            _gridlockManager.ClearWriteProtected();
            Assert.AreEqual(2, dictionary.Count);
        }

    }


}
