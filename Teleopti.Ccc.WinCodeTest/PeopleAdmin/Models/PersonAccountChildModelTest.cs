using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    [TestFixture]
    public class PersonAccountChildModelTest
    {
        private IAccount _account1;
        private IAccount _account2;
        private IAccount _account3;
        private PersonAccountChildModel _targetDay;
        private PersonAccountChildModel _targetTime;
        private IAbsence _absenceDay = new Absence(), _absenceTime = new Absence();
        private DateOnly _from;
        private MockRepository _mocker;
        private ITraceableRefreshService _traceableRefreshService;
	    private IPersonAccountUpdater _personAccountUpdater;
        private PersonAccountCollection _acc;
	    private IPerson _person;


        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _traceableRefreshService = _mocker.StrictMock<ITraceableRefreshService>();
	        _personAccountUpdater = _mocker.StrictMock<IPersonAccountUpdater>();
            _person = PersonFactory.CreatePerson("Mama Hawa");
            _from = new DateOnly(2008, 1, 3);
            var from2 = new DateOnly(2009, 1, 3);
            var from3 = new DateOnly(2010, 1, 3);
            _absenceDay.Tracker = Tracker.CreateDayTracker();
            _absenceTime.Tracker = Tracker.CreateTimeTracker();

            _account1 = new AccountDay(_from);
            _account1.BalanceIn = TimeSpan.FromDays(5);
            _account1.Accrued = TimeSpan.FromDays(20);
            _account1.Extra = TimeSpan.FromDays(5);
            _account1.BalanceOut = TimeSpan.FromDays(2);
            _account2 = new AccountTime(from2);
            _account2.BalanceIn = new TimeSpan(3);
            _account2.BalanceOut = TimeSpan.FromDays(1);
            _account3 = new AccountDay(from3);
            _account3.BalanceIn = TimeSpan.FromDays(3);
            _account3.Accrued = TimeSpan.FromDays(20);
            _account3.Extra = TimeSpan.FromDays(3);
            _account3.BalanceOut = TimeSpan.FromDays(1);

            _acc = new PersonAccountCollection(_person);
			var accountDay = new PersonAbsenceAccount(_person, _absenceDay);
			var accountTime = new PersonAbsenceAccount(_person, _absenceTime);

            _acc.Add(accountDay);
            _acc.Add(accountTime);

            accountDay.Add(_account1);
            accountDay.Add(_account3);
            accountTime.Add(_account2);

            _targetDay = new PersonAccountChildModel(_traceableRefreshService, _acc, _account1,null, null);
           
            _targetTime = new PersonAccountChildModel(_traceableRefreshService, _acc, _account2, null, null);
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
            //henrika 080824: removed, return something from mock if you want to check this property 
            //Assert.IsNotNull(_target.Used);
            Assert.IsNotNull(_targetDay.CurrentAccount);
            Assert.IsFalse(_targetDay.CanGray);
            Assert.IsFalse(_targetDay.CanBold);
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
            ICurrentScenario scenario = _mocker.StrictMock<ICurrentScenario>();
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account1);
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account2);
            LastCall.Repeat.AtLeastOnce();

            var personAbsenceAccountRepository = _mocker.DynamicMock<IPersonAbsenceAccountRepository>();
            var refreshService = _mocker.DynamicMock<ITraceableRefreshService>();
			_targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, _personAccountUpdater);
            ((PersonAccountChildModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);


			_targetTime = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account2, null, _personAccountUpdater);
            ((PersonAccountChildModelForTest)_targetTime).SetUnitOfWorkFactory(unitOfWorkFactory);
            _mocker.ReplayAll();
            
            _targetDay.Accrued = 2;
            Assert.AreEqual(2, _targetDay.Accrued);


            _targetTime.Accrued = new TimeSpan(2);
            Assert.AreEqual(new TimeSpan(2), _targetTime.Accrued);
            
            SetTargetDayWithoutAccount(unitOfWork, scenario);
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
        public void VerifyBalanceIn()
        {
            

            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            ICurrentScenario scenario = _mocker.DynamicMock<ICurrentScenario>();


            var personAbsenceAccountRepository = _mocker.DynamicMock<IPersonAbsenceAccountRepository>();
            var refreshService = _mocker.DynamicMock<ITraceableRefreshService>();
			PersonAccountChildModelForTest targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, _personAccountUpdater);
			PersonAccountChildModelForTest targetTime = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account2, null, _personAccountUpdater);


            targetDay.SetUnitOfWorkFactory(unitOfWorkFactory);
            targetTime.SetUnitOfWorkFactory(unitOfWorkFactory);

            IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account1);
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account2);
            LastCall.Repeat.AtLeastOnce();

            _mocker.ReplayAll();

            targetDay.BalanceIn = 3;
            targetTime.BalanceIn = new TimeSpan(2, 0, 0, 0);

            Assert.AreEqual(3, targetDay.BalanceIn);
            Assert.AreEqual(new TimeSpan(2, 0, 0, 0), targetTime.BalanceIn);

            SetTargetDayWithoutAccount(unitOfWork, scenario);
            Assert.IsNull(_targetDay.BalanceIn);

            _mocker.VerifyAll();
        }

        [Test]
        public void VerifyBalanceOut()
        {
            IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            ICurrentScenario scenario = _mocker.DynamicMock<ICurrentScenario>();        
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            var personAbsenceAccountRepository = _mocker.DynamicMock<IPersonAbsenceAccountRepository>();
            var refreshService = _mocker.DynamicMock<ITraceableRefreshService>();
			PersonAccountChildModelForTest targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, _personAccountUpdater);
			PersonAccountChildModelForTest targetTime = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account2, null, _personAccountUpdater);

            targetDay.SetUnitOfWorkFactory(unitOfWorkFactory);
            targetTime.SetUnitOfWorkFactory(unitOfWorkFactory);

            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account1);
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account2);
            LastCall.Repeat.AtLeastOnce();

            _mocker.ReplayAll();

            targetDay.BalanceOut = 3;
            targetTime.BalanceOut = new TimeSpan(2, 0, 0, 0);

            Assert.AreEqual(3, targetDay.BalanceOut);
            Assert.AreEqual(new TimeSpan(2, 0, 0, 0), targetTime.BalanceOut);

            SetTargetDayWithoutAccount(unitOfWork, scenario);
            Assert.IsNull(_targetDay.BalanceOut);

            _mocker.VerifyAll();
        }

        [Test]
        public void VerifyRemaining()
        {
            Assert.AreEqual(28, _targetDay.Remaining);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.Remaining);
        }

        //[Test]
        //public void VerifyCanSetTrackingAbsence()
        //{
        //    //IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
        //    //IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
        //    //Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
        //    //unitOfWork.Dispose();
        //    //_traceableRefreshService.Refresh(_targetDay.CurrentAccount, unitOfWork);
        //    _mocker.ReplayAll();
        //    IAbsence absence = AbsenceFactory.CreateAbsence("Ada Enna Behe Wage");
        //    ICurrentScenario scenario = _mocker.DynamicMock<ICurrentScenario>();
        //    var personAbsenceAccountRepository = _mocker.DynamicMock<IPersonAbsenceAccountRepository>();
        //    var refreshService = _mocker.DynamicMock<ITraceableRefreshService>();
        //    _targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, new PersonAccountUpdater(personAbsenceAccountRepository, refreshService));
        //    //((PersonAccountChildModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
        //    Assert.AreNotSame(absence, _targetDay.TrackingAbsence);

        //    Assert.Throws<ArgumentException>(()=> _targetDay.TrackingAbsence = absence);

        //    //Assert.AreSame(absence, _targetDay.TrackingAbsence);
        //    _mocker.VerifyAll();
        //}

        [Test]
        public void VerifyDate()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            IPersonAccountUpdater personAccountUpdater = _mocker.StrictMock<IPersonAccountUpdater>();

            Expect.Call(() => personAccountUpdater.Update(_person));

            _mocker.ReplayAll();

            _targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null,
                                                            personAccountUpdater);
            ((PersonAccountChildModelForTest) _targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
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
            ICurrentScenario scenario = _mocker.DynamicMock<ICurrentScenario>();
            var personAbsenceAccountRepository = _mocker.DynamicMock<IPersonAbsenceAccountRepository>();
            var refreshService = _mocker.DynamicMock<ITraceableRefreshService>();
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account1);
            LastCall.Repeat.AtLeastOnce();
            _traceableRefreshService.Refresh(_account2);
            LastCall.Repeat.AtLeastOnce();

            _mocker.ReplayAll();
			_targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, _personAccountUpdater);
            ((PersonAccountChildModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetDay.Extra = 10;
            Assert.AreEqual(10, _targetDay.Extra);

			_targetTime = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account2, null, _personAccountUpdater);
            ((PersonAccountChildModelForTest)_targetTime).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetTime.Extra = new TimeSpan(10);
            Assert.AreEqual(new TimeSpan(10), _targetTime.Extra);

            SetTargetDayWithoutAccount(unitOfWork, scenario);
            Assert.IsNull(_targetDay.BalanceOut);
            _mocker.VerifyAll();
        }

        [Test] //Fix on monday
        public void VerifyAccountCount()
        {
            PersonAccountModel personAccountAdapter = new PersonAccountModel(_traceableRefreshService, new DateOnly(2008, 5, 2), _targetDay.Parent);

            Assert.AreEqual(3, personAccountAdapter.PersonAccountCount);
        }

        [Test]
        public void VerifyAccountType()
        {
            Assert.AreEqual(_targetDay.AccountType, Tracker.CreateDayTracker().Description.Name);
            Assert.AreEqual(_targetTime.AccountType, Tracker.CreateTimeTracker().Description.Name);

            SetTargetDayWithoutAccount();
            Assert.AreEqual(_targetDay.AccountType, string.Empty);
        }

        [Test]
        public void VerifyTrackerDescription()
        {
            Assert.AreEqual(_targetDay.TrackingAbsence, _absenceDay);
            Assert.AreEqual(_targetTime.TrackingAbsence, _absenceTime);
            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.TrackingAbsence);
        }

        [Test]
        public void VerifyContainedEntity()
        {
            Assert.AreEqual(_targetDay.ContainedEntity, _account1);
            Assert.AreEqual(_targetTime.ContainedEntity, _account2);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.ContainedEntity);
        }

        private void SetTargetDayWithoutAccount()
        {
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            ICurrentScenario scenario = _mocker.DynamicMock<ICurrentScenario>();
            var account = _acc.Find(new DateOnly(2005, 5, 2)).FirstOrDefault();
            _targetDay =
                new PersonAccountChildModel(
                    new TraceableRefreshService(scenario, new ScheduleRepository(unitOfWorkFactory.CreateAndOpenUnitOfWork())), _acc, account, null, null);
        }

        private void SetTargetDayWithoutAccount(IUnitOfWork unitOfWork, ICurrentScenario scenario)
        {
            //IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            //ICurrentScenario scenario = _mocker.DynamicMock<ICurrentScenario>();
			var account = _acc.Find(new DateOnly(2005, 5, 2)).FirstOrDefault();
	        _targetDay =
		        new PersonAccountChildModel(
			        new TraceableRefreshService(scenario,new ScheduleRepository(unitOfWork)), _acc, account, null, null);
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

        private class PersonAccountChildModelForTest : PersonAccountChildModel
        {
            public PersonAccountChildModelForTest(ITraceableRefreshService refreshService, IPersonAccountCollection personAccounts, IAccount account, CommonNameDescriptionSetting commonNameDescription, IPersonAccountUpdater personAccountUpdater)
                : base(refreshService, personAccounts, account, commonNameDescription, personAccountUpdater)
            {}

            public void SetUnitOfWorkFactory(IUnitOfWorkFactory unitOfWorkFactory)
            {
                UnitOfWorkFactory = unitOfWorkFactory;
            }
        }
    }
}
