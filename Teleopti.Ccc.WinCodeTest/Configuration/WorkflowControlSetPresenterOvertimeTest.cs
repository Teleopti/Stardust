using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[DomainTest]
	public class WorkflowControlSetPresenterOvertimeTest : IIsolateSystem
	{
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;
		public FakeUnitOfWorkFactory UnitOfWorkFactory;
		public FakeRepositoryFactory RepositoryFactory;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeToggleManager ToggleManager;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillTypeRepository SkillTypeRepository;

		private WorkflowControlSetPresenter _target;
		private WorkflowControlSetView _view;
		private static IList<ISkillType> _skillTypes;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();

			if (_skillTypes == null)
				_skillTypes = new List<ISkillType>
				{
					new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId(),
					new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId(),
					new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId()
				};
		}

		[TearDown]
		public void Clean()
		{
			_view?.Dispose();
		}
		
		[Theory]
		public void VerifyAddNewDateOpenPeriod()
		{
			initializeWithDefaultWorkflowControlSet();
			SkillTypeRepository.AddRange(_skillTypes);

			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			_target.AddOvertimeRequestOpenDatePeriod();

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);

			var overtimeRequestPeriod = _target.SelectedModel.OvertimeRequestPeriodModels[0];

			Assert.AreEqual(DateOnly.Today, overtimeRequestPeriod.PeriodStartDate);
			Assert.AreEqual(DateOnly.Today.AddDays(staffingInfoAvailableDays), overtimeRequestPeriod.PeriodEndDate);
		}

		[Theory]
		public void VerifyAddNewRollingOpenPeriod()
		{
			initializeWithDefaultWorkflowControlSet();
			_view.RefreshOvertimeOpenPeriodsGrid();

			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			_target.AddOvertimeRequestOpenRollingPeriod();

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels[0].RollingStart);
			Assert.AreEqual(staffingInfoAvailableDays, _target.SelectedModel.OvertimeRequestPeriodModels[0].RollingEnd);
		}

		[Test]
		public void VerifyDefaultValueToPhoneWhenNoSkillTypeOfModel()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod());

			SkillTypeRepository.AddRange(_skillTypes);

			_target = new WorkflowControlSetPresenter(null, UnitOfWorkFactory, RepositoryFactory, ToggleManager);
			_view = new FixConfirmResultWorkflowControlSetView(ToggleManager, _target, true);
			_target.Initialize();
			_view.SetOvertimeOpenPeriodsGridRowCount(0);
			_view.RefreshOvertimeOpenPeriodsGrid();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(1, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			Assert.AreEqual(Resources.SkillTypeInboundTelephony, _target.SelectedModel.OvertimeRequestPeriodModels.First().SkillTypes);
		}

		[Test]
		public void VerifyDeleteOvertimeRequestPeriodCanDelete()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod());
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
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod());
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
			initializeWithDefaultWorkflowControlSet();

			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
			_target.DeleteOvertimeRequestPeriod(_target.SelectedModel.OvertimeRequestPeriodModels);
			Assert.AreEqual(0, _target.SelectedModel.OvertimeRequestPeriodModels.Count);
		}

		[Test]
		public void VerifyCanSetOvertimeRequestMaximumTimeHandleType()
		{
			initializeWithDefaultWorkflowControlSet();
			_target.SetOvertimeRequestMaximumTimeHandleType(new OvertimeRequestValidationHandleOptionView(OvertimeValidationHandleType.Deny,"deny"));

			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestMaximumTimeHandleType,_target.SelectedModel.OvertimeRequestMaximumOvertimeValidationHandleOptionView.WorkRuleValidationHandleType);
		}

		[Test]
		public void VerifyCanSetOvertimeRequestMaximumTime()
		{
			initializeWithDefaultWorkflowControlSet();
			_target.SetOvertimeRequestMaximumTime(TimeSpan.MaxValue);

			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestMaximumTime, _target.SelectedModel.OvertimeRequestMaximumTime);
		}

		[Test]
		public void VerifyCanSetOvertimeRequestMaximumContinuousWorkTimeEnabled()
		{
			initializeWithDefaultWorkflowControlSet();
			_target.SetOvertimeRequestMaximumContinuousWorkTimeEnabled(true);

			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestMaximumContinuousWorkTimeEnabled, _target.SelectedModel.OvertimeRequestMaximumContinuousWorkTimeEnabled);
		}

		[Test]
		public void VerifyCanSetOvertimeRequestMaximumContinuousWorkTimeAndMinimumRestTimeThreshold()
		{
			initializeWithDefaultWorkflowControlSet();
			_target.SetOvertimeRequestMaximumContinuousWorkTime(TimeSpan.FromHours(4));
			_target.SetOvertimeRequestMinimumRestTimeThreshold(TimeSpan.FromMinutes(20));

			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestMaximumContinuousWorkTime, _target.SelectedModel.OvertimeRequestMaximumContinuousWorkTime);
			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestMinimumRestTimeThreshold, _target.SelectedModel.OvertimeRequestMinimumRestTimeThreshold);
		}

		[Test]
		public void VerifyCanSetOvertimeRequestMaximumContinuousWorkTimeHandleType()
		{
			initializeWithDefaultWorkflowControlSet();
			_target.SetOvertimeRequestMaximumContinuousWorkTimeHandleType(new OvertimeRequestValidationHandleOptionView(OvertimeValidationHandleType.Deny, "deny"));

			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestMaximumContinuousWorkTimeHandleType
				, _target.SelectedModel.OvertimeRequestMaximumContinuousWorkTimeValidationHandleOptionView.WorkRuleValidationHandleType);
		}

		[Test]
		public void VerifyMove()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);
			var openDatePeriod =new OvertimeRequestOpenDatePeriod().WithId();
			var openRollingPeriod = new OvertimeRequestOpenRollingPeriod().WithId();
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

		[Test]
		public void VerifyCanSetOvertimeRequestStaffingCheckMethod()
		{
			initializeWithDefaultWorkflowControlSet();
			_target.SetOvertimeRequestIntradayStaffingCheckMethod(
				new OvertimeRequestStaffingCheckMethodOptionView(OvertimeRequestStaffingCheckMethod.Intraday, "Intraday"));

			Assert.AreEqual(_target.SelectedModel.DomainEntity.OvertimeRequestStaffingCheckMethod,
				_target.SelectedModel.OvertimeRequestStaffingCheckMethodOptionView.OvertimeRequestStaffingCheckMethod);
		}

		[Test]
		public void VerifyCanSetOvertimeRequestUsePrimarySkill()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var workflowControlSetModel = _target.WorkflowControlSetModelCollection.ElementAt(0);
			_target.SetSelectedWorkflowControlSetModel(workflowControlSetModel);
			_target.SetOvertimeRequestUsePrimarySkill(true);

			Assert.AreEqual(true, workflowControlSetModel.OvertimeRequestUsePrimarySkill);
		}

		private void initialize()
		{
			_target = new WorkflowControlSetPresenter(null, UnitOfWorkFactory, RepositoryFactory, ToggleManager);
			_view = new WorkflowControlSetView(ToggleManager, _target);
			_target.Initialize();
		}

		private void initializeWithDefaultWorkflowControlSet()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
		}
	}
}