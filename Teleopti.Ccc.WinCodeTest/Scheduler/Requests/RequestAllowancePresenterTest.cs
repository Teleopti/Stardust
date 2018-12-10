using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
	[TestFixture]
	public class RequestAllowancePresenterTest
	{
		private IRequestAllowanceView _view;
		private IRequestAllowanceModel _model;
		private RequestAllowancePresenter _target;
		private IBudgetGroup _budgetGroup;

		[SetUp]
		public void Setup()
		{
			_budgetGroup = new BudgetGroup();
			_view = MockRepository.GenerateMock<IRequestAllowanceView>();
			_model = MockRepository.GenerateMock<IRequestAllowanceModel>();
			_target = new RequestAllowancePresenter(_view, _model);
		}

		[Test]
		public void ShouldInitialize()
		{
			var bg = new BudgetGroup { Name = "BG1" };
			var shrinkage = new CustomShrinkage("Vacation");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			shrinkage.AddAbsence(absence);
			bg.AddCustomShrinkage(new CustomShrinkage("Vacation"));
			_model.Stub(x => x.SelectedBudgetGroup).Return(bg);
			_model.Stub(x => x.AbsencesInBudgetGroup).Return(new HashSet<IAbsence>(new[] { absence }));

			_target.Initialize(_budgetGroup, new DateOnly());

			Assert.That(_target.Absences.Count(), Is.EqualTo(1));
		}

		[Test]
		public void ShouldLoadBudgetGroups()
		{
			var bg1 = new BudgetGroup { Name = "BG1" };
			var bg2 = new BudgetGroup { Name = "BG2" };
			_model.Stub(x => x.BudgetGroups).Return(new[] { bg1, bg2 });

			Assert.That(_target.BudgetGroups().Count(), Is.EqualTo(2));
		}

		[Test]
		public void ShouldHaveSelectedBudgetGroup()
		{
			var bg1 = new BudgetGroup { Name = "BG1" };
			_model.Stub(x => x.SelectedBudgetGroup).Return(bg1);
			Assert.That(_target.SelectedBudgetGroup(), Is.EqualTo(bg1));
		}

		[Test]
		public void ShouldReloadWhenAllowanceSelectionChanged()
		{
			var visibleWeek = new DateOnlyPeriod();
			_model.Stub(x => x.VisibleWeek).Return(visibleWeek);
			_model.Stub(x => x.ShrinkedAllowanceSelected).Return(false);

			_target.OnRadioButtonShrinkedAllowanceCheckChanged(true);

			_model.AssertWasCalled(x => x.ReloadModel(visibleWeek, true));
		}

		[Test]
		public void ShouldNotReloadWhenAllowanceSelectionChange()
		{
			var visibleWeek = new DateOnlyPeriod();
			_model.Stub(x => x.VisibleWeek).Return(visibleWeek);
			_model.Stub(x => x.ShrinkedAllowanceSelected).Return(true);

			_target.OnRadioButtonShrinkedAllowanceCheckChanged(true);

			_model.AssertWasNotCalled(x => x.ReloadModel(visibleWeek, false));
		}

		[Test]
		public void ShouldReloadWhenFullAllowanceSelectionChanged()
		{
			var visibleWeek = new DateOnlyPeriod();
			_model.Stub(x => x.VisibleWeek).Return(visibleWeek);
			_model.Stub(x => x.FullAllowanceSelected).Return(false);

			_target.OnRadioButtonFullAllowanceCheckChanged(true);

			_model.AssertWasCalled(x => x.ReloadModel(visibleWeek, true));
		}

		[Test]
		public void ShouldNotReloadWhenFullAllowanceSelectionChange()
		{
			var visibleWeek = new DateOnlyPeriod();
			_model.Stub(x => x.VisibleWeek).Return(visibleWeek);
			_model.Stub(x => x.FullAllowanceSelected).Return(true);

			_target.OnRadioButtonFullAllowanceCheckChanged(true);

			_model.AssertWasNotCalled(x => x.ReloadModel(visibleWeek, false));
		}

		[Test]
		public void ShouldReloadWhenBudgetGroupSelectionChanged()
		{
			var bg1 = new BudgetGroup { Name = "BG1" };
			var bg2 = new BudgetGroup { Name = "BG2" };
			_model.Stub(x => x.BudgetGroups).Return(new[] { bg1, bg2 });
			_model.Stub(x => x.SelectedBudgetGroup).Return(bg1);

			_target.OnComboBoxAdvBudgetGroupSelectedIndexChanged(bg2);

			_model.AssertWasCalled(x => x.ReloadModel(new DateOnlyPeriod(), true));
		}

		[Test]
		public void ShouldNotReloadWhenBudgetGroupSelectionNotChange()
		{
			var bg1 = new BudgetGroup { Name = "BG1" };
			var bg2 = new BudgetGroup { Name = "BG2" };
			_model.Stub(x => x.BudgetGroups).Return(new[] { bg1, bg2 });
			_model.Stub(x => x.SelectedBudgetGroup).Return(bg1);

			_target.OnComboBoxAdvBudgetGroupSelectedIndexChanged(bg1);

			_model.AssertWasNotCalled(x => x.ReloadModel(new DateOnlyPeriod(), false));
		}

		[Test]
		public void ShouldReloadWhenMoveToNextWeek()
		{
			_target.OnRequestAllowanceGridControlNextButtonClicked();

			_model.AssertWasCalled(x => x.MoveToNextWeek(), options => options.Repeat.Once());
			_model.AssertWasCalled(x => x.ReloadModel(new DateOnlyPeriod(), true));
		}

		[Test]
		public void ShouldReloadWhenMoveToPreviousWeek()
		{
			_target.OnRequestAllowanceGridControlPreviousButtonClicked();

			_model.AssertWasCalled(x => x.MoveToPreviousWeek(), options => options.Repeat.Once());
			_model.AssertWasCalled(x => x.ReloadModel(new DateOnlyPeriod(), true));
		}

		[Test]
		public void ShouldShowWeekName()
		{
			const string weekName = "2011-11-12 w.46";
			_model.Stub(x => x.WeekName).Return(weekName);

			Assert.That(_target.WeekName, Is.EqualTo(weekName));
		}

		[Test]
		public void ShouldRefreshWhenClickRefreshButton()
		{
			_target.OnRefreshButtonClicked();
			_model.AssertWasCalled(x => x.ReloadModel(new DateOnlyPeriod(), true));
		}

		[Test]
		public void ShouldNotRefreshWhenRefreshButtonNotClicked()
		{
			_model.AssertWasNotCalled(x => x.ReloadModel(new DateOnlyPeriod(), true));
		}

		[Test]
		public void ExceptionIfBudgetGroupComboBoxIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => _target.OnComboBoxAdvBudgetGroupSelectedIndexChanged(null));
		}

		[Test]
		public void ShouldInitializeGridBinding()
		{
			_target.InitializeGridBinding();
			_model.AssertWasCalled(x => x.ReloadModel(new DateOnlyPeriod(), true), options => options.Repeat.Once());
		}
	}
}