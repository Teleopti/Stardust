using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode;
using Teleopti.Ccc.WinCode.Budgeting;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
	[TestFixture]
	public class BudgetGroupMainTest
	{
		private BudgetGroupMainPresenter _target;
		private MockRepository _mock;
		private IBudgetGroupMainView _view;
		private BudgetGroupMainModel _model;
		private IBudgetGroup _budgetGroup;
        private IBudgetSettingsProvider _settingsProvider;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IBudgetGroupMainView>();
		    _settingsProvider = _mock.StrictMock<IBudgetSettingsProvider>();
 
			_budgetGroup = new BudgetGroup();
	        _model = new BudgetGroupMainModel(_settingsProvider)
	                     {
	                         BudgetGroup = _budgetGroup,
	                         Period = new DateOnlyPeriod(2010, 8, 3, 2010, 9, 3),
	                         Scenario = ScenarioFactory.CreateScenarioAggregate()
	                     };
			_target = new BudgetGroupMainPresenter(_view, _model);
		}

		[Test]
		public void CanInitialize()
		{
            var settings = new BudgetSettings();
            settings.SelectedView = ViewType.Day;

			using (_mock.Record())
			{
				Expect.Call(() => _view.SetText(string.Empty)).IgnoreArguments();
                Expect.Call(_settingsProvider.BudgetSettings).Return(settings);
                _view.SelectedView = settings.SelectedView;
			}

			using (_mock.Playback())
			{
				_target.Initialize();
			}
		}

		[Test]
		public void ShouldAddEfficiencyShrinkageRowToBudgetGroup()
		{
			IUnitOfWork unitOfWork = _mock.StrictMock<IUnitOfWork>();
			var customEfficiencyShrinkage = new CustomEfficiencyShrinkage("Sleepy");
			using (_mock.Record())
			{
				Expect.Call(() => unitOfWork.Reassociate(_budgetGroup));
				Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
				Expect.Call(() => _view.OnAddEfficiencyShrinkageRow(customEfficiencyShrinkage)).IgnoreArguments();
			}
			using (_mock.Playback())
			{
				_target.AddEfficiencyShrinkageRow(customEfficiencyShrinkage, unitOfWork);
				Assert.That(_model.BudgetGroup.CustomEfficiencyShrinkages.Count() == 1);
			}
		}

		[Test]
		public void ShouldDeleteShrinkageRowsFromBudgetGroup()
		{
			IUnitOfWork unitOfWork = _mock.StrictMock<IUnitOfWork>();
			var customShrinkage = new CustomShrinkage("SomeSickness");
			_budgetGroup.AddCustomShrinkage(customShrinkage);
			var customShrinkages = new List<ICustomShrinkage> { customShrinkage };
			using (_mock.Record())
			{
				Expect.Call(() => unitOfWork.Reassociate(_budgetGroup));
				Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
				Expect.Call(() => _view.OnDeleteShrinkageRows(customShrinkages));
			}
			using (_mock.Playback())
			{
				Assert.That(_model.BudgetGroup.CustomShrinkages.Count() == 1);
				_target.DeleteShrinkageRows(customShrinkages, unitOfWork);
				Assert.That(_model.BudgetGroup.CustomShrinkages.Count() == 0);
			}
		}

		[Test]
		public void ShouldDeleteEfficiencyShrinkageRowsFromBudgetGroup()
		{
			IUnitOfWork unitOfWork = _mock.StrictMock<IUnitOfWork>();
			var customEfficiencyShrinkage = new CustomEfficiencyShrinkage("Kaffe");
			_budgetGroup.AddCustomEfficiencyShrinkage(customEfficiencyShrinkage);
			var customEfficiencyShrinkages = new List<ICustomEfficiencyShrinkage> { customEfficiencyShrinkage };
			using (_mock.Record())
			{
				Expect.Call(() => unitOfWork.Reassociate(_budgetGroup));
				Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
				Expect.Call(() => _view.OnDeleteEfficiencyShrinkageRows(customEfficiencyShrinkages));
			}
			using (_mock.Playback())
			{
				Assert.That(_model.BudgetGroup.CustomEfficiencyShrinkages.Count() == 1);
				_target.DeleteEfficiencyShrinkageRows(customEfficiencyShrinkages, unitOfWork);
				Assert.That(_model.BudgetGroup.CustomEfficiencyShrinkages.Count() == 0);
			}
		}

        [Test]
		public void ShouldUpdateEfficiency()
		{
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var newName = "NewName";
            var shrinkage = new CustomEfficiencyShrinkage("OldName");

            using (_mock.Record())
            {
                Expect.Call(() => unitOfWork.Reassociate(_budgetGroup));
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
                Expect.Call(() => _view.UpdateEfficiencyShrinkageProperty(shrinkage)).IgnoreArguments();
            }
            using (_mock.Playback())
            {
                shrinkage.ShrinkageName = newName;
                shrinkage.IncludedInAllowance = true;
                _target.SaveUpdateEfficiencyShrinkage(new CustomEfficiencyShrinkageUpdatedEventArgs(), unitOfWork);
                Assert.AreEqual(newName, shrinkage.ShrinkageName);
                Assert.IsTrue(shrinkage.IncludedInAllowance);
            }
		}

		[Test]
		public void ShouldNotifyUserOfOptimisticLockExceptionWhenAddingCustomEfficiencyShrinkage()
		{
			IUnitOfWork unitOfWork = _mock.StrictMock<IUnitOfWork>();

			using (_mock.Record())
			{
				Expect.Call(() => unitOfWork.Reassociate(_budgetGroup));
				Expect.Call(unitOfWork.PersistAll()).Throw(new OptimisticLockException());
				Expect.Call(_view.NotifyCustomShrinkageUpdatedByOthers);
			}
			using (_mock.Playback())
			{
                _target.AddEfficiencyShrinkageRow(new CustomEfficiencyShrinkage("SomeSickness"), unitOfWork);
				Assert.That(_model.BudgetGroup.CustomEfficiencyShrinkages.Count() == 0);
			}
		}

		[Test]
		public void ShouldNotifyUserOfOptimisticLockExceptionWhenDeletingCustomShrinkages()
		{
			IUnitOfWork unitOfWork = _mock.StrictMock<IUnitOfWork>();

			using (_mock.Record())
			{
				Expect.Call(() => unitOfWork.Reassociate(_budgetGroup));
				Expect.Call(unitOfWork.PersistAll()).Throw(new OptimisticLockException());
				Expect.Call(_view.NotifyCustomShrinkageUpdatedByOthers);
			}
			using (_mock.Playback())
			{
				_target.DeleteShrinkageRows(new List<ICustomShrinkage>(), unitOfWork);
				Assert.That(_model.BudgetGroup.CustomShrinkages.Count() == 0);
			}
		}

		[Test]
		public void ShouldNotifyUserOfOptimisticLockExceptionWhenDeletingCustomEfficiencyShrinkages()
		{
			IUnitOfWork unitOfWork = _mock.StrictMock<IUnitOfWork>();

			using (_mock.Record())
			{
				Expect.Call(() => unitOfWork.Reassociate(_budgetGroup));
				Expect.Call(unitOfWork.PersistAll()).Throw(new OptimisticLockException());
				Expect.Call(_view.NotifyCustomShrinkageUpdatedByOthers);
			}
			using (_mock.Playback())
			{
				_target.DeleteEfficiencyShrinkageRows(new List<ICustomEfficiencyShrinkage>(), unitOfWork);
				Assert.That(_model.BudgetGroup.CustomEfficiencyShrinkages.Count() == 0);
			}
		}

        [Test]
        public void CanShowDayView()
        {
            var settings = new BudgetSettings();
            settings.SelectedView = ViewType.Month;

            using (_mock.Record())
            {
                Expect.Call(() => _view.SetText(string.Empty)).IgnoreArguments();
                Expect.Call(_settingsProvider.BudgetSettings).Return(settings).Repeat.Twice();
                _view.SelectedView = settings.SelectedView;
                _view.DayView = false;
                _view.WeekView = false;
                _view.MonthView = false;

                _view.DayView = true;
                _view.ShowDayView();
            }

            using (_mock.Playback())
            {
                _target.Initialize();
                _target.ShowDayView(); 
            }
        }
        
        [Test]
        public void CanShowMonthView()
        {
            var settings = new BudgetSettings();
            settings.SelectedView = ViewType.Day;

            using (_mock.Record())
            {
                Expect.Call(() => _view.SetText(string.Empty)).IgnoreArguments();
                Expect.Call(_settingsProvider.BudgetSettings).Return(settings).Repeat.Twice();
                _view.SelectedView = settings.SelectedView;
                _view.DayView = false;
                _view.WeekView = false;
                _view.MonthView = false;

                _view.MonthView = true;
                _view.ShowMonthView();
            }

            using (_mock.Playback())
            {
                _target.Initialize();
                _target.ShowMonthView();
            }
        }

        [Test]
        public void CanShowWeekView()
        {
            var settings = new BudgetSettings();
            settings.SelectedView = ViewType.Day;

            using (_mock.Record())
            {
                Expect.Call(() => _view.SetText(string.Empty)).IgnoreArguments();
                Expect.Call(_settingsProvider.BudgetSettings).Return(settings).Repeat.Twice();
                _view.SelectedView = settings.SelectedView;
                _view.DayView = false;
                _view.WeekView = false;
                _view.MonthView = false;

                _view.WeekView = true;
                _view.ShowWeekView();
            }

            using (_mock.Playback())
            {
                _target.Initialize();
                _target.ShowWeekView();
            }
        }

	    [Test]
        public void CanSaveSettings()
        {
            var settings = new BudgetSettings();
            settings.SelectedView = ViewType.Day;

            using (_mock.Record())
            {
                Expect.Call(() => _view.SetText(string.Empty)).IgnoreArguments();
                Expect.Call(_settingsProvider.BudgetSettings).Return(settings);
                Expect.Call(() => _settingsProvider.Save());
                _view.SelectedView = settings.SelectedView;
            }

            using (_mock.Playback())
            {
                _target.Initialize();
                _target.SaveSettings();
            }
        }

        [Test]
        public void ShouldUpdateBudgetGroup()
        {
            var bg = new BudgetGroup();
            _target.UpdateBudgetGroup(bg);
            Assert.That(_model.BudgetGroup, Is.EqualTo(bg));
        }
	}
}
