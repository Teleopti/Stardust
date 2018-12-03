using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public abstract class OvertimeRequestEndPeriodCellValidator : OvertimeRequestPeriodCellValidatorBase
	{
		protected OvertimeRequestEndPeriodCellValidator(IToggleManager toggleManager) : base(toggleManager)
		{
		}

		public override bool ValidateCell(OvertimeRequestPeriodModel dataItem)
		{
			if (!canValidate(dataItem)) return false;
			var result = false;
			var requestPeriod = dataItem.PeriodType.Item.GetPeriod(DateOnly.Today);
			var availablePeriod = getAvailablePeriod();

			if (availablePeriod.StartDate <= requestPeriod.EndDate
				&& requestPeriod.EndDate <= availablePeriod.EndDate)
				result = true;

			Canceled = !result;

			if (Canceled)
			{
				Message = getMessage(availablePeriod);
			}

			return result;
		}
	}

	public class OvertimeRequestDatePeriodEndCellValidator : OvertimeRequestEndPeriodCellValidator
	{
		public OvertimeRequestDatePeriodEndCellValidator(IToggleManager toggleManager) : base(toggleManager) { }

		protected override string getMessage(DateOnlyPeriod availablePeriod)
		{
			return string.Format(Resources.OvertimeRequestDatePeriodError, availablePeriod.StartDate.ToShortDateString(),
				availablePeriod.EndDate.ToShortDateString());
		}

		protected override bool canValidate(OvertimeRequestPeriodModel dataItem)
		{
			return dataItem.PeriodType.Item is OvertimeRequestOpenDatePeriod;
		}
	}

	public class OvertimeRequestRollingPeriodEndCellValidator : OvertimeRequestEndPeriodCellValidator
	{
		public OvertimeRequestRollingPeriodEndCellValidator(IToggleManager toggleManager) : base(toggleManager) { }

		protected override string getMessage(DateOnlyPeriod availablePeriod)
		{
			return string.Format(Resources.OvertimeRequestRollingPeriodError, availablePeriod.DayCount() - 1);
		}

		protected override bool canValidate(OvertimeRequestPeriodModel dataItem)
		{
			return dataItem.PeriodType.Item is OvertimeRequestOpenRollingPeriod;
		}
	}
}