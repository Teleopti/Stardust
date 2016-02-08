using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    /// <summary>
    /// Tests for PersonAccountModel
    /// </summary>
    [TestFixture]
    public class PersonAccountModelTest
    {
        private IAccount _account1;
        private IAccount _account2;
        private IAccount _account3;
        private PersonAccountModel _targetDay;
        private PersonAccountModel _targetDay2;
        private PersonAccountModel _targetTime;
        private CommonNameDescriptionSetting commonNameDecroption1;
        private IPersonAccountCollection _collection;
        private IPerson _person;
        private MockRepository _mocker;
        private ITraceableRefreshService _traceableRefreshService;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _traceableRefreshService = _mocker.StrictMock<ITraceableRefreshService>();
            _person = PersonFactory.CreatePerson("Mama Hawa");

            var from1 = new DateOnly(2008, 1, 3);
            var from2 = new DateOnly(2009, 1, 3);
            var from3 = new DateOnly(2010, 1, 3);

            _account1 = new AccountDay(from1);
            _account1.BalanceIn = TimeSpan.FromDays(5);
            _account1.Accrued = TimeSpan.FromDays(20);
            _account1.Extra = TimeSpan.FromDays(5);

            _account2 = new AccountTime(from2);
            _account2.BalanceIn = new TimeSpan(3);

            _account3 = new AccountDay(from3);
            _account3.BalanceIn = TimeSpan.FromDays(3);
            _account3.Accrued = TimeSpan.FromDays(20);
            _account3.Extra = TimeSpan.FromDays(3);

            var apa = new PersonAbsenceAccount(_person, new Absence());
            apa.Absence.Tracker = Tracker.CreateDayTracker();
            apa.Add(_account1);


            var apa2 = new PersonAbsenceAccount(_person, new Absence());
            apa2.Absence.Tracker = Tracker.CreateTimeTracker();
            apa2.Add(_account2);
            _collection = new PersonAccountCollection(_person);
            _collection.Add(apa);
            _collection.Add(apa2);

            commonNameDecroption1 = new CommonNameDescriptionSetting();

            _targetDay = new PersonAccountModel(_traceableRefreshService, _collection, _account1, null);
            _targetDay2 = new PersonAccountModel(_traceableRefreshService, _collection, _account1, commonNameDecroption1);
            _targetTime = new PersonAccountModel(_traceableRefreshService, _collection, _account2, null);
        }

        [Test]
        public void VerifyPropertyNotNullOrEmpty()
        {

            Assert.IsNotNull(_targetDay.Parent);
            Assert.IsNotEmpty(_targetDay.FullName);
            Assert.IsNotNull(_targetDay.AccountDate);
            Assert.IsNotNull(_targetDay.Accrued);
            Assert.IsNotNull(_targetDay.BalanceIn);
            Assert.IsNotNull(_targetDay.BalanceOut);
            Assert.IsNotNull(_targetDay.Extra);
            Assert.IsNotNull(_targetDay.PersonAccountCount);
            Assert.IsFalse(_targetDay.CanGray);
            Assert.IsFalse(_targetDay.CanBold);
        }

        [Test]
        public void VerifyGridControlCanSet()
        {
            using (GridControl grid = new GridControl())
            {
                _targetDay.GridControl = grid;
                Assert.IsNotNull(_targetDay.GridControl);
            }
        }

        [Test]
        public void VerifyCanGetPersonCommonName()
        {
            Assert.AreEqual(_person.Name.ToString(), _targetDay2.FullName);
        }

        [Test]
        public void VerifyCurrentPersonAccountNullChecks()
        {
            SetTargetDayWithoutAccount();
            IAccount account = _targetDay.GetCurrentPersonAccountByDate(new DateOnly(2005, 5, 2));

            Assert.IsNull(account);
            Assert.IsNull(_targetDay.AccountDate);
            Assert.IsNull(_targetDay.Extra);
            Assert.IsNull(_targetDay.Accrued);
            Assert.IsNull(_targetDay.BalanceIn);
            Assert.IsNull(_targetDay.BalanceOut);
            Assert.IsTrue(_targetDay.CanGray);
            Assert.IsNull(_targetDay.Used);
            Assert.IsFalse(_targetDay.CanBold);
        }

        [Test]
        public void VerifyAccrued()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account1);
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account2);
            LastCall.Repeat.AtLeastOnce();

            _mocker.ReplayAll();
            _targetDay = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account1);
            ((PersonAccountModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetDay.Accrued = 2;
            Assert.AreEqual(2, _targetDay.Accrued);

            _targetTime = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account2);
            ((PersonAccountModelForTest)_targetTime).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetTime.Accrued = new TimeSpan(2);
            Assert.AreEqual(new TimeSpan(2), _targetTime.Accrued);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.Accrued);
            _mocker.VerifyAll();
        }

        [Test]
        public void VerifyUsed()
        {
            Assert.AreEqual(0, _targetDay.Used);
            Assert.AreEqual(TimeSpan.Zero, _targetTime.Used);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.Used);
        }

        [Test]
        public void VerifyCurrentAccount()
        {
            Assert.AreEqual(_account1, _targetDay.CurrentAccount);
            Assert.AreEqual(_account2, _targetTime.CurrentAccount);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.CurrentAccount);
        }

        [Test]
        public void VerifyRemaining()
        {
            Assert.AreEqual(30, _targetDay.Remaining);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.Remaining);
        }

        [Test]
        public void VerifyBalanceIn()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account1);
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account2);
            LastCall.Repeat.AtLeastOnce();

            _mocker.ReplayAll();
            _targetDay = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account1);
            ((PersonAccountModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetDay.BalanceIn = 5;
            Assert.AreEqual(5, _targetDay.BalanceIn);

            _targetTime = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account2);
            ((PersonAccountModelForTest)_targetTime).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetTime.BalanceIn = new TimeSpan(2);
            Assert.AreEqual(new TimeSpan(2), _targetTime.BalanceIn);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.BalanceIn);
            _mocker.VerifyAll();
        }

        [Test]
        public void VerifyBalanceOut()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account1);
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account2);
            LastCall.Repeat.AtLeastOnce();

            _mocker.ReplayAll();
            _targetDay = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account1);
            ((PersonAccountModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetDay.BalanceOut = 5;
            Assert.AreEqual(5, _targetDay.BalanceOut);

            _targetTime = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account2);
            ((PersonAccountModelForTest)_targetTime).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetTime.BalanceOut = new TimeSpan(2);
            Assert.AreEqual(new TimeSpan(2), _targetTime.BalanceOut);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.BalanceOut);
            _mocker.VerifyAll();
        }

        [Test]
        public void VerifyDate()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            unitOfWork.Dispose();
            _traceableRefreshService.Refresh(_targetDay.CurrentAccount);
            _mocker.ReplayAll();

            _targetDay = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account1);
            ((PersonAccountModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            Assert.AreEqual(new DateOnly(2008, 1, 3), _targetDay.AccountDate);
            Assert.AreEqual(new DateOnly(2009, 1, 3), _targetTime.AccountDate);

            _targetDay.AccountDate = new DateOnly(DateTime.MinValue);
            Assert.AreEqual(DateTime.MinValue, _targetDay.AccountDate.Value.Date);
            _mocker.VerifyAll();
        }

        [Test]
        public void VerifyExtra()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account1);
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account2);
            LastCall.Repeat.AtLeastOnce();

            _mocker.ReplayAll();
            _targetDay = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account1);
            ((PersonAccountModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetDay.Extra = 10;
            Assert.AreEqual(10, _targetDay.Extra);

            _targetTime = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account2);
            ((PersonAccountModelForTest)_targetTime).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetTime.Extra = new TimeSpan(10);
            Assert.AreEqual(new TimeSpan(10), _targetTime.Extra);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.BalanceOut);
            _mocker.VerifyAll();
        }

        [Test]
        public void VerifyExpandStateCanSet()
        {
            Assert.AreEqual(false, _targetDay.ExpandState);
            _targetDay.ExpandState = true;
            Assert.AreEqual(true, _targetDay.ExpandState);

        }

        [Test]
        public void VerifyAccountCount()
        {
            PersonAccountModel personAccountAdapter = new PersonAccountModel(_traceableRefreshService,new DateOnly(2008, 5, 2), _collection);
            Assert.AreEqual(2, personAccountAdapter.PersonAccountCount);
            
            _collection = new PersonAccountCollection(_person); // only 1
            personAccountAdapter = new PersonAccountModel(_traceableRefreshService, new DateOnly(2008, 5, 2), _collection);
            Assert.AreEqual(0, personAccountAdapter.PersonAccountCount);
        }

        [Test]
        public void AccountType()
        {
            Assert.AreEqual(_targetDay.AccountType, Tracker.CreateDayTracker().Description.Name);
            Assert.AreEqual(_targetTime.AccountType, Tracker.CreateTimeTracker().Description.Name);

            SetTargetDayWithoutAccount();
            Assert.AreEqual(_targetDay.AccountType, string.Empty);
        }

        [Test]
        public void TrackerDescription()
        {
            Assert.AreEqual(_targetDay.TrackingAbsence.Tracker, Tracker.CreateDayTracker());
            Assert.AreEqual(_targetTime.TrackingAbsence.Tracker, Tracker.CreateTimeTracker());

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.TrackingAbsence);
        }

        private void SetTargetDayWithoutAccount()
        {
            _targetDay = new PersonAccountModel(_traceableRefreshService, new DateOnly(2005, 5, 2), _collection);
        }

        [Test]
        public void VerifyCanSetCanBold()
        {
            Assert.IsFalse(_targetDay.CanBold);
            _targetDay.CanBold = true;
            Assert.IsTrue(_targetDay.CanBold);

            Assert.IsFalse(_targetTime.CanBold);
            _targetTime.CanBold = true;
            Assert.IsTrue(_targetTime.CanBold);
        }

        [Test]
        public void VerifyCanSetTrackingAbsence()
        {
            //IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            //IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            //Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            //unitOfWork.Dispose();
            //_traceableRefreshService.Refresh(_targetDay.CurrentAccount, unitOfWork);
            _mocker.ReplayAll();
            IAbsence absence = AbsenceFactory.CreateAbsence("Ada Enna Behe Wage");

            _targetDay = new PersonAccountModelForTest(_traceableRefreshService, _collection, _account1);
            //((PersonAccountModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            Assert.AreNotSame(absence, _targetDay.TrackingAbsence);

            Assert.Throws<ArgumentException>(() => _targetDay.TrackingAbsence = absence);

            //Assert.AreSame(absence, _targetDay.TrackingAbsence);
            _mocker.VerifyAll();
        }

        [Test]
        public void VerifyResetCanBoldPropertyOfChildAdapters()
        {
            using (GridControl grid = new GridControl())
            {
                IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
                ICurrentScenario currentScenario = _mocker.DynamicMock<ICurrentScenario>();
                
                IPersonAccountChildModel adapter1 = new PersonAccountChildModel
                    (new TraceableRefreshService(currentScenario, new ScheduleStorage(unitOfWorkFactory.CreateAndOpenUnitOfWork())), _collection, _account1,
                     null, null);

                IPersonAccountChildModel adapter2 = new PersonAccountChildModel
                    (new TraceableRefreshService(currentScenario, new ScheduleStorage(unitOfWorkFactory.CreateAndOpenUnitOfWork())), _collection, _account3,
                     null, null);

                adapter1.CanBold = true;
                adapter2.CanBold = true;

                IList<IPersonAccountChildModel> adapterCollection = new
                    List<IPersonAccountChildModel>();
                adapterCollection.Add(adapter1);
                adapterCollection.Add(adapter2);

                grid.Tag = adapterCollection;

                _targetDay.GridControl = grid;

                _targetDay.ResetCanBoldPropertyOfChildAdapters();

                IList<IPersonAccountChildModel> childAdapters = _targetDay.GridControl.Tag as
                                                                IList<IPersonAccountChildModel>;

                Assert.IsNotNull(childAdapters);
                Assert.AreEqual(2, childAdapters.Count);
                Assert.IsFalse(childAdapters[0].CanBold);
                Assert.IsFalse(childAdapters[1].CanBold);
            }
        }

        private class PersonAccountModelForTest : PersonAccountModel
        {
            public PersonAccountModelForTest(ITraceableRefreshService refreshService, IPersonAccountCollection personAccounts, IAccount account)
                : base(refreshService, personAccounts, account, null)
            { }

            public void SetUnitOfWorkFactory(IUnitOfWorkFactory unitOfWorkFactory)
            {
                UnitOfWorkFactory = unitOfWorkFactory;
            }
        }
    }
}
