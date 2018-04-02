using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class OvertimeRequestPeriodCellValidatorTest
	{
		private WorkflowControlSetModel _workflowControlSetModel;
		private IOvertimeRequestOpenPeriod _overtimeRequestOpenPeriod;
		private IWorkflowControlSet _workflowControlSet;
		private OvertimeRequestDatePeriodStartCellValidator _datePeriodStartCellValidator;
		private OvertimeRequestDatePeriodEndCellValidator _datePeriodEndCellValidator;
		private OvertimeRequestRollingPeriodStartCellValidator _rollingPeriodStartCellValidator;
		private OvertimeRequestRollingPeriodEndCellValidator _rollingPeriodEndCellValidator;
		private FakeToggleManager _fakeToggleManager;

		[DatapointSource]
		public Toggles[] ToggleList =
		{
			Toggles.Wfm_Staffing_StaffingReadModel49DaysStep2_45109,
			Toggles.Wfm_Staffing_StaffingReadModel28DaysStep1_45109,
		};

		[SetUp]
		public void Setup()
		{
			_workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			_workflowControlSet.SetId(Guid.NewGuid());
			_workflowControlSetModel = new WorkflowControlSetModel(_workflowControlSet);
			_fakeToggleManager = new FakeToggleManager();
			_datePeriodStartCellValidator = new OvertimeRequestDatePeriodStartCellValidator(_fakeToggleManager);
			_datePeriodEndCellValidator = new OvertimeRequestDatePeriodEndCellValidator(_fakeToggleManager);
			_rollingPeriodStartCellValidator = new OvertimeRequestRollingPeriodStartCellValidator(_fakeToggleManager);
			_rollingPeriodEndCellValidator = new OvertimeRequestRollingPeriodEndCellValidator(_fakeToggleManager);
		}

		[Theory]
		public void ShouldReturnTrueWhenDatePeriodWithinAvailableDays(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenDatePeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				PeriodStartDate = DateOnly.Today,
				PeriodEndDate = DateOnly.Today.AddDays(getStaffingAvailableDays())
			};

			Assert.AreEqual(true, _datePeriodStartCellValidator.ValidateCell(model), _datePeriodStartCellValidator.Message);
			Assert.AreEqual(true, _datePeriodEndCellValidator.ValidateCell(model), _datePeriodEndCellValidator.Message);
		}

		[Theory]
		public void ShouldReturnFalseWhenDatePeriodTotallyExceedsAvailableDays(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			var staffingAvailableDays = getStaffingAvailableDays();
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenDatePeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				PeriodStartDate = DateOnly.Today.AddDays(staffingAvailableDays + 1),
				PeriodEndDate = DateOnly.Today.AddDays(staffingAvailableDays + 13)
			};

			Assert.AreEqual(false, _datePeriodStartCellValidator.ValidateCell(model));
			Assert.AreEqual(false, _datePeriodEndCellValidator.ValidateCell(model));
		}

		[Theory]
		public void ShouldReturnFalseWhenStartDateIsEarlierThanToday(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenDatePeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				PeriodStartDate = DateOnly.Today.AddDays(-1),
				PeriodEndDate = DateOnly.Today.AddDays(getStaffingAvailableDays())
			};

			Assert.AreEqual(false, _datePeriodStartCellValidator.ValidateCell(model));
		}

		[Theory]
		public void ShouldReturnFalseWhenEndDateIsLaterThanTheEndDateOfAvailableDaysRange(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenDatePeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				PeriodStartDate = DateOnly.Today,
				PeriodEndDate = DateOnly.Today.AddDays(getStaffingAvailableDays() + 1)
			};

			Assert.AreEqual(false, _datePeriodEndCellValidator.ValidateCell(model));
		}

		[Theory]
		public void ShouldReturnTrueWhenRollingPeriodWithinAvailableDays(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenRollingPeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				RollingStart = 0,
				RollingEnd = getStaffingAvailableDays()
			};

			Assert.AreEqual(true, _rollingPeriodStartCellValidator.ValidateCell(model), _rollingPeriodStartCellValidator.Message);
			Assert.AreEqual(true, _rollingPeriodEndCellValidator.ValidateCell(model), _rollingPeriodEndCellValidator.Message);
		}

		[Theory]
		public void ShouldReturnFalseWhenRollingPeriodExceedsAvailableDays(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenRollingPeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				RollingStart = 0,
				RollingEnd = getStaffingAvailableDays() + 1
			};

			Assert.AreEqual(false, _rollingPeriodEndCellValidator.ValidateCell(model));
		}

		[Theory]
		public void ShouldReturnFalseWhenRollingPeriodIsTotallyOutsideAvailableDaysRange(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			var staffingAvailableDays = getStaffingAvailableDays();
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenRollingPeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				RollingStart = staffingAvailableDays + 1,
				RollingEnd = staffingAvailableDays + 13
			};

			Assert.AreEqual(false, _rollingPeriodStartCellValidator.ValidateCell(model));
			Assert.AreEqual(false, _rollingPeriodEndCellValidator.ValidateCell(model));
		}

		[Theory]
		[SetCulture("en-US")]
		public void ShouldReturnErrorMessageWhenDateOpenPeriodIsOutOfRange(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			var staffingAvailableDays = getStaffingAvailableDays();
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenDatePeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				PeriodStartDate = DateOnly.Today.AddDays(staffingAvailableDays + 1),
				PeriodEndDate = DateOnly.Today.AddDays(staffingAvailableDays + 13)
			};

			var startDate = DateOnly.Today.ToShortDateString();
			var endDate = DateOnly.Today.AddDays(staffingAvailableDays).ToShortDateString();

			_datePeriodStartCellValidator.ValidateCell(model);
			_datePeriodEndCellValidator.ValidateCell(model);

			Assert.AreEqual($"valid range of date is {startDate} - {endDate}", _datePeriodStartCellValidator.Message);
			Assert.AreEqual($"valid range of date is {startDate} - {endDate}", _datePeriodEndCellValidator.Message);
		}

		[Theory]
		[SetCulture("en-US")]
		public void ShouldReturnErrorMessageWhenRollingOpenPeriodIsOutOfRange(Toggles toggles)
		{
			_fakeToggleManager.Enable(toggles);
			var staffingAvailableDays = getStaffingAvailableDays();
			_overtimeRequestOpenPeriod = new OvertimeRequestOpenRollingPeriod();
			var model = new OvertimeRequestPeriodModel(_overtimeRequestOpenPeriod, _workflowControlSetModel)
			{
				RollingStart = staffingAvailableDays + 1,
				RollingEnd = staffingAvailableDays + 13
			};


			_rollingPeriodStartCellValidator.ValidateCell(model);
			_rollingPeriodEndCellValidator.ValidateCell(model);

			Assert.AreEqual($"valid range is 0 - {staffingAvailableDays}", _rollingPeriodStartCellValidator.Message);
			Assert.AreEqual($"valid range is 0 - {staffingAvailableDays}", _rollingPeriodEndCellValidator.Message);
		}


		private int getStaffingAvailableDays()
		{
			return StaffingInfoAvailableDaysProvider.GetDays(_fakeToggleManager);
		}
	}
}