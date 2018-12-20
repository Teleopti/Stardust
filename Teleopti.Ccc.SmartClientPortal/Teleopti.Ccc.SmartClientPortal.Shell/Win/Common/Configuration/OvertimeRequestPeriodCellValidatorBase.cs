using System;
using System.Drawing;
using System.Globalization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public abstract class OvertimeRequestPeriodCellValidatorBase : SFGridCellValidatorBase<OvertimeRequestPeriodModel>
	{
		private readonly IToggleManager _toggleManager;

		public override Color ErrorBackColor => Color.Yellow;

		protected OvertimeRequestPeriodCellValidatorBase(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		protected override CultureInfo CurrentCulture => TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.UICulture;

		protected abstract bool canValidate(OvertimeRequestPeriodModel dataItem);

		protected abstract string getMessage(DateOnlyPeriod availablePeriod);

		protected DateOnlyPeriod getAvailablePeriod()
		{
			int staffingAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager);

			return new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(staffingAvailableDays));
		}

		protected override void setStyleError(GridStyleInfo style)
		{
			if (DateTime.TryParse(style.CellValue.ToString(), out DateTime date))
			{
				style.Error = date.ToShortDateString();
				style.ResetError();
			}
			else
			{
				base.setStyleError(style);
			}
			
		}
	}
}