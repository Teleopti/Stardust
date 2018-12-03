using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;

using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeRepositories;

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
        private readonly IAbsence _absenceDay = new Absence();
	    private readonly IAbsence _absenceTime = new Absence();
	    private DateOnly _from;
        private ITraceableRefreshService _traceableRefreshService;
	    private IPersonAccountUpdater _personAccountUpdater;
        private PersonAccountCollection _acc;
	    private IPerson _person;
		
        [SetUp]
        public void Setup()
        {
            _traceableRefreshService = MockRepository.GenerateMock<ITraceableRefreshService>();
	        _personAccountUpdater = new PersonAccountUpdaterDummy();
            _person = PersonFactory.CreatePerson("Mama Hawa");
            _from = new DateOnly(2008, 1, 3);
            var from2 = new DateOnly(2009, 1, 3);
            var from3 = new DateOnly(2010, 1, 3);
            _absenceDay.Tracker = Tracker.CreateDayTracker();
            _absenceTime.Tracker = Tracker.CreateTimeTracker();

	        _account1 = new AccountDay(_from)
	        {
		        BalanceIn = TimeSpan.FromDays(5),
		        Accrued = TimeSpan.FromDays(20),
		        Extra = TimeSpan.FromDays(5),
		        BalanceOut = TimeSpan.FromDays(2)
	        };
	        _account2 = new AccountTime(from2)
	        {
		        BalanceIn = new TimeSpan(3),
		        BalanceOut = TimeSpan.FromDays(1)
	        };
	        _account3 = new AccountDay(from3)
	        {
		        BalanceIn = TimeSpan.FromDays(3),
		        Accrued = TimeSpan.FromDays(20),
		        Extra = TimeSpan.FromDays(3),
		        BalanceOut = TimeSpan.FromDays(1)
	        };

	        var accountDay = new PersonAbsenceAccount(_person, _absenceDay);
			var accountTime = new PersonAbsenceAccount(_person, _absenceTime);

	        _acc = new PersonAccountCollection(_person) {accountDay, accountTime};

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
            Assert.IsNotNull(_targetDay.CurrentAccount);
            Assert.IsFalse(_targetDay.CanGray);
            Assert.IsFalse(_targetDay.CanBold);
        }

        [Test]
        public void VerifyAccrued()
        {
			var unitOfWorkFactory = new FakeUnitOfWorkFactory(null, null, null);
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			var scenario = new DefaultScenarioFromRepository(scenarioRepository);

			_targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, _personAccountUpdater);
            ((PersonAccountChildModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
			
			_targetTime = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account2, null, _personAccountUpdater);
            ((PersonAccountChildModelForTest)_targetTime).SetUnitOfWorkFactory(unitOfWorkFactory);
            
            _targetDay.Accrued = 2;
            Assert.AreEqual(2, _targetDay.Accrued);
			
            _targetTime.Accrued = new TimeSpan(2);
            Assert.AreEqual(new TimeSpan(2), _targetTime.Accrued);
            
            SetTargetDayWithoutAccount(scenario);
            Assert.IsNull(_targetDay.Accrued);
			
	        _traceableRefreshService.AssertWasCalled(x => x.Refresh(_account1));
	        _traceableRefreshService.AssertWasCalled(x => x.Refresh(_account2));
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
            var unitOfWorkFactory = new FakeUnitOfWorkFactory(null, null, null);
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			var scenario = new DefaultScenarioFromRepository(scenarioRepository);

			var targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, _personAccountUpdater);
			var targetTime = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account2, null, _personAccountUpdater);
			
            targetDay.SetUnitOfWorkFactory(unitOfWorkFactory);
            targetTime.SetUnitOfWorkFactory(unitOfWorkFactory);
			
            targetDay.BalanceIn = 3;
            targetTime.BalanceIn = new TimeSpan(2, 0, 0, 0);

            Assert.AreEqual(3, targetDay.BalanceIn);
            Assert.AreEqual(new TimeSpan(2, 0, 0, 0), targetTime.BalanceIn);

            SetTargetDayWithoutAccount(scenario);
            Assert.IsNull(_targetDay.BalanceIn);
			
	        _traceableRefreshService.AssertWasCalled(x => x.Refresh(_account1));
	        _traceableRefreshService.AssertWasCalled(x => x.Refresh(_account2));
		}

        [Test]
        public void VerifyBalanceOut()
        {
			var unitOfWorkFactory = new FakeUnitOfWorkFactory(null, null, null);
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			var scenario = new DefaultScenarioFromRepository(scenarioRepository);

			var targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, _personAccountUpdater);
			var targetTime = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account2, null, _personAccountUpdater);

            targetDay.SetUnitOfWorkFactory(unitOfWorkFactory);
            targetTime.SetUnitOfWorkFactory(unitOfWorkFactory);
			
            targetDay.BalanceOut = 3;
            targetTime.BalanceOut = new TimeSpan(2, 0, 0, 0);

            Assert.AreEqual(3, targetDay.BalanceOut);
            Assert.AreEqual(new TimeSpan(2, 0, 0, 0), targetTime.BalanceOut);

            SetTargetDayWithoutAccount(scenario);
            Assert.IsNull(_targetDay.BalanceOut);

	        _traceableRefreshService.AssertWasCalled(x => x.Refresh(_account1));
	        _traceableRefreshService.AssertWasCalled(x => x.Refresh(_account2));
		}

        [Test]
        public void VerifyRemaining()
        {
            Assert.AreEqual(28, _targetDay.Remaining);

            SetTargetDayWithoutAccount();
            Assert.IsNull(_targetDay.Remaining);
        }

        [Test]
        public void VerifyDate()
        {
			var unitOfWorkFactory = new FakeUnitOfWorkFactory(null, null, null);
			var personAccountUpdater = MockRepository.GenerateMock<IPersonAccountUpdater>();
			
            _targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null,
                                                            personAccountUpdater);
            ((PersonAccountChildModelForTest) _targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            Assert.AreEqual(new DateOnly(2008, 1, 3), _targetDay.AccountDate);
            Assert.AreEqual(new DateOnly(2009, 1, 3), _targetTime.AccountDate);

            _targetDay.AccountDate = new DateOnly(DateTime.MinValue);
            Assert.AreEqual(DateTime.MinValue, _targetDay.AccountDate.Value.Date);

	        personAccountUpdater.AssertWasCalled(x => x.Update(_person));
		}
		
        [Test]
        public void VerifyExtra()
        {
			var unitOfWorkFactory = new FakeUnitOfWorkFactory(null, null, null);
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			var scenario = new DefaultScenarioFromRepository(scenarioRepository);

			_targetDay = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account1, null, _personAccountUpdater);
            ((PersonAccountChildModelForTest)_targetDay).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetDay.Extra = 10;
            Assert.AreEqual(10, _targetDay.Extra);

			_targetTime = new PersonAccountChildModelForTest(_traceableRefreshService, _acc, _account2, null, _personAccountUpdater);
            ((PersonAccountChildModelForTest)_targetTime).SetUnitOfWorkFactory(unitOfWorkFactory);
            _targetTime.Extra = new TimeSpan(10);
            Assert.AreEqual(new TimeSpan(10), _targetTime.Extra);

            SetTargetDayWithoutAccount(scenario);
            Assert.IsNull(_targetDay.BalanceOut);

	        _traceableRefreshService.AssertWasCalled(x => x.Refresh(_account1));
	        _traceableRefreshService.AssertWasCalled(x => x.Refresh(_account2));
		}

        [Test]
        public void VerifyAccountCount()
        {
            var personAccountAdapter = new PersonAccountModel(_traceableRefreshService, _targetDay.Parent, _account1, new CommonNameDescriptionSetting());

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
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			var scenario = new DefaultScenarioFromRepository(scenarioRepository);
            var account = _acc.Find(new DateOnly(2005, 5, 2)).FirstOrDefault();
	        
	        _targetDay = new PersonAccountChildModel(new TraceableRefreshService(scenario, new FakeScheduleStorage_DoNotUse()), _acc, account, null, null);
        }

        private void SetTargetDayWithoutAccount(ICurrentScenario scenario)
        {
			var account = _acc.Find(new DateOnly(2005, 5, 2)).FirstOrDefault();
	        _targetDay = new PersonAccountChildModel(new TraceableRefreshService(scenario,new FakeScheduleStorage_DoNotUse()), _acc, account, null, null);
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


	//Don't fake IScheduleStorage - use the real one instead
}
