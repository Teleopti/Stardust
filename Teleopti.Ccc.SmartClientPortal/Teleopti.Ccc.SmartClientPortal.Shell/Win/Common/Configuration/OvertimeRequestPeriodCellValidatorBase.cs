﻿using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public abstract class OvertimeRequestPeriodCellValidatorBase : SFGridCellValidatorBase<OvertimeRequestPeriodModel>
	{
		private readonly IToggleManager _toggleManager;

		protected OvertimeRequestPeriodCellValidatorBase(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		protected override CultureInfo CurrentCulture => TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture;

		protected abstract bool canValidate(OvertimeRequestPeriodModel dataItem);

		protected abstract string getMessage(DateOnlyPeriod availablePeriod);

		protected DateOnlyPeriod getAvailablePeriod()
		{
			int staffingAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager);

			return new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(staffingAvailableDays));
		}
	}
}