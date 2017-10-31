using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class WorkflowControlSetModelOvertimeTest
	{
		private WorkflowControlSetModel _target;
		private IWorkflowControlSet _domainEntity;

		[SetUp]
		public void Setup()
		{
			_domainEntity = new WorkflowControlSet("description");
			_target = new WorkflowControlSetModel(_domainEntity);
		}

		[Test]
		public void VerifyCanGetDefaultInstancesForType()
		{
			Assert.AreEqual(2, WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters.Count);
			Assert.IsTrue(WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item is OvertimeRequestOpenDatePeriod);
			Assert.AreEqual(Resources.FromTo, WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].DisplayText);
			Assert.IsTrue(WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[1].Item is OvertimeRequestOpenRollingPeriod);
			Assert.AreEqual(Resources.Rolling, WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[1].DisplayText);
		}

		[Test]
		public void VerifyDefaultValuesForOvertimeRequestPeriods()
		{
			var datePeriod =
				(OvertimeRequestOpenDatePeriod)WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item;
			var rollingDatePeriod =
				(OvertimeRequestOpenRollingPeriod)WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[1].Item;

			var todayDateOnly = DateOnly.Today;
			var inNextMonthDateOnly = new DateOnly(todayDateOnly.Date.AddMonths(1));

			var startDateOnly = new DateOnly(inNextMonthDateOnly.Year, inNextMonthDateOnly.Month, 1);
			var endDateOnly = new DateOnly(startDateOnly.Date.AddMonths(1).AddDays(-1));
			var requestPeriod = new DateOnlyPeriod(startDateOnly, endDateOnly);
			Assert.AreEqual(requestPeriod, datePeriod.Period);
			Assert.AreEqual(new MinMax<int>(2, 15), rollingDatePeriod.BetweenDays);
		}

		[Test]
		public void VerifyCanGetOvertimeRequestPeriodList()
		{
			_target.DomainEntity.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod());
			_target.DomainEntity.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod());

			var targetPeriodModels = _target.OvertimeRequestPeriodModels;
			var periodAdapters = WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters;

			Assert.AreEqual(2, _target.OvertimeRequestPeriodModels.Count);
			Assert.IsTrue(targetPeriodModels[0].PeriodType.Equals(periodAdapters[0]));
			Assert.IsTrue(targetPeriodModels[1].PeriodType.Equals(periodAdapters[1]));
			Assert.IsFalse(targetPeriodModels[0].PeriodType.Equals(periodAdapters[1]));
			Assert.IsFalse(targetPeriodModels[1].PeriodType.Equals(periodAdapters[0]));
			Assert.AreEqual(Resources.FromTo, _target.OvertimeRequestPeriodModels[0].PeriodType.DisplayText);
		}

		[Test]
		public void VerifyCanGetAndSetPeriodFromModel()
		{
			_target.DomainEntity.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(2010, 6, 1, 2010, 8, 31)
			});
			_target.DomainEntity.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(2, 14),
			});

			var OvertimeRequestPeriodList = _target.OvertimeRequestPeriodModels;
			var datePeriod = OvertimeRequestPeriodList[0];
			var rollingPeriod = OvertimeRequestPeriodList[1];

			Assert.IsFalse(datePeriod.RollingStart.HasValue);
			Assert.IsFalse(datePeriod.RollingEnd.HasValue);
			Assert.AreEqual(new DateOnly(2010, 6, 1), datePeriod.PeriodStartDate);
			Assert.AreEqual(new DateOnly(2010, 8, 31), datePeriod.PeriodEndDate);

			Assert.IsFalse(rollingPeriod.PeriodStartDate.HasValue);
			Assert.IsFalse(rollingPeriod.PeriodEndDate.HasValue);
			Assert.AreEqual(2, rollingPeriod.RollingStart);
			Assert.AreEqual(14, rollingPeriod.RollingEnd);

			datePeriod.PeriodStartDate = datePeriod.PeriodStartDate.Value.AddDays(365);
			Assert.AreEqual(new DateOnly(2011, 6, 1), datePeriod.PeriodStartDate);
			Assert.AreEqual(new DateOnly(2011, 6, 1), datePeriod.PeriodEndDate);

			datePeriod.PeriodEndDate = datePeriod.PeriodStartDate.Value.AddDays(-365);
			Assert.AreEqual(new DateOnly(2010, 6, 1), datePeriod.PeriodStartDate);
			Assert.AreEqual(new DateOnly(2010, 6, 1), datePeriod.PeriodEndDate);

			rollingPeriod.RollingStart = 16;
			Assert.AreEqual(16, rollingPeriod.RollingStart);
			Assert.AreEqual(16, rollingPeriod.RollingEnd);

			rollingPeriod.RollingEnd = 3;
			Assert.AreEqual(3, rollingPeriod.RollingStart);
			Assert.AreEqual(3, rollingPeriod.RollingEnd);
		}
	}
}