using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	[DomainTest]
	public class WorkflowControlSetPresenterOvertimeTest : ISetup
	{
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;
		public FakeUnitOfWorkFactory UnitOfWorkFactory;
		public FakeRepositoryFactory RepositoryFactory;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeToggleManager ToggleManager;
		public FakeActivityRepository ActivityRepository;

		private WorkflowControlSetPresenter _target;
		private WorkflowControlSetView _view;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();
		}

		[TearDown]
		public void Clean()
		{
			_view?.Dispose();
		}

		[DatapointSource]
		public Toggles[] ToggleList =
		{
			Toggles.Wfm_Staffing_StaffingReadModel49DaysStep2_45109,
			Toggles.Wfm_Staffing_StaffingReadModel28DaysStep1_45109,
			Toggles.OvertimeRequestPeriodSetting_46417
		};

		[Theory]
		public void VerifyAddNewDateOpenPeriod(Toggles toggles)
		{
			ToggleManager.Enable(toggles);

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			_target.AddOvertimeRequestOpenDatePeriod();

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			Assert.AreEqual(DateOnly.Today, _target.SelectedModel.OvertimeRequestPeriodModels[0].PeriodStartDate);
			Assert.AreEqual(DateOnly.Today.AddDays(staffingInfoAvailableDays), _target.SelectedModel.OvertimeRequestPeriodModels[0].PeriodEndDate);
		}

		[Theory]
		public void VerifyAddNewRollingOpenPeriod(Toggles toggles)
		{
			ToggleManager.Enable(toggles);

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();
			_view.RefreshOvertimeOpenPeriodsGrid();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			_target.AddOvertimeRequestOpenRollingPeriod();

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels[0].RollingStart);
			Assert.AreEqual(staffingInfoAvailableDays, _target.SelectedModel.OvertimeRequestPeriodModels[0].RollingEnd);
		}

		[Test]
		public void VerifyDeleteOvertimeRequestPeriodCanDelete()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			workflowControlSet.AddOpenOvertimeRequestPeriod(WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item);
			_target = new WorkflowControlSetPresenter(null, UnitOfWorkFactory, RepositoryFactory, ToggleManager);
			_view = new FixConfirmResultWorkflowControlSetView(ToggleManager, _target, true);
			_target.Initialize();
			_view.SetOvertimeOpenPeriodsGridRowCount(0);
			_view.RefreshOvertimeOpenPeriodsGrid();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			_target.DeleteOvertimeRequestPeriod(
				new ReadOnlyCollection<OvertimeRequestPeriodModel>(_target.SelectedModel.OvertimeRequestPeriodModels));
			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
		}

		[Test]
		public void VerifyCanCancelDeleteOvertimeRequestPeriod()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			workflowControlSet.AddOpenOvertimeRequestPeriod(WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item);
			_target = new WorkflowControlSetPresenter(null, UnitOfWorkFactory, RepositoryFactory, ToggleManager);
			_view = new FixConfirmResultWorkflowControlSetView(ToggleManager, _target, false);
			_target.Initialize();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			_target.DeleteOvertimeRequestPeriod(
				new ReadOnlyCollection<OvertimeRequestPeriodModel>(_target.SelectedModel.OvertimeRequestPeriodModels));
			Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);

		}

		[Test]
		public void VerifyCanGetDefaultPeriod()
		{
			DateTime startDate = DateTime.Today;
			startDate = DateHelper.GetFirstDateInMonth(startDate, CultureInfo.CurrentCulture);
			DateTime endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, 3).AddDays(-1);

			initialize();

			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)), _target.ProjectionPeriod);

			_target.NextProjectionPeriod();

			startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, 1);
			endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(endDate, 1);
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
				_target.ProjectionPeriod);

			_target.PreviousProjectionPeriod();

			startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, -1);
			endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(endDate, -1);
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
				_target.ProjectionPeriod);
		}

		[Test]
		public void VerifyDeleteOvertimeRequestPeriodNothingToDelete()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			initialize();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			_target.DeleteOvertimeRequestPeriod(_target.SelectedModel.OvertimeRequestPeriodModels);
			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
		}

		[Test]
		public void VerifyCanSetOvertimeRequestMaximumTimeHandleType()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SelectedModel.OvertimeRequestValidationHandleOptionView = new OvertimeRequestValidationHandleOptionView(OvertimeValidationHandleType.Deny,"deny");

			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestMaximumTimeHandleType,_target.SelectedModel.OvertimeRequestValidationHandleOptionView.WorkRuleValidationHandleType);
		}

		[Test]
		public void VerifyCanSetOvertimeRequestMaximumTime()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SelectedModel.OvertimeRequestMaximumTime = TimeSpan.MaxValue;

			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestMaximumTime, _target.SelectedModel.OvertimeRequestMaximumTime);
		}

		[Test]
		public void VerifyMove()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			var openDatePeriod =
				(OvertimeRequestOpenDatePeriod)WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item;
			((IEntity)openDatePeriod).SetId(Guid.NewGuid());
			var openRollingPeriod =
				(OvertimeRequestOpenRollingPeriod)WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[1].Item;
			((IEntity)openRollingPeriod).SetId(Guid.NewGuid());
			workflowControlSet.AddOpenOvertimeRequestPeriod(openDatePeriod);
			workflowControlSet.AddOpenOvertimeRequestPeriod(openRollingPeriod);

			initialize();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(openDatePeriod, _target.SelectedModel.OvertimeRequestPeriodModels[0].DomainEntity);
			Assert.AreEqual(openRollingPeriod, _target.SelectedModel.OvertimeRequestPeriodModels[1].DomainEntity);
			_target.MoveUp(_target.SelectedModel.OvertimeRequestPeriodModels[1]);
			Assert.AreEqual(openDatePeriod, _target.SelectedModel.OvertimeRequestPeriodModels[1].DomainEntity);
			Assert.AreEqual(openRollingPeriod, _target.SelectedModel.OvertimeRequestPeriodModels[0].DomainEntity);
			_target.MoveDown(_target.SelectedModel.OvertimeRequestPeriodModels[0]);
			Assert.AreEqual(openDatePeriod, _target.SelectedModel.OvertimeRequestPeriodModels[0].DomainEntity);
			Assert.AreEqual(openRollingPeriod, _target.SelectedModel.OvertimeRequestPeriodModels[1].DomainEntity);
		}

		private void initialize()
		{
			_target = new WorkflowControlSetPresenter(null, UnitOfWorkFactory, RepositoryFactory, ToggleManager);
			_view = new WorkflowControlSetView(ToggleManager, _target);
			_target.Initialize();
		}
	}
}