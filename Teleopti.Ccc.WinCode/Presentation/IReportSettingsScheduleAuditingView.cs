﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{
    public interface IReportSettingsScheduleAuditingView
    {
        void InitializeSettings();
        IList<IPerson> ModifiedBy { get; }
        DateOnlyPeriod ChangePeriod { get; }
        DateOnlyPeriod SchedulePeriod { get; }
        DateOnlyPeriod ChangePeriodDisplay { get; }
        DateOnlyPeriod SchedulePeriodDisplay { get; }
        ICollection<IPerson> Agents { get; }
        ReportSettingsScheduleAuditingModel ScheduleAuditingSettingsModel { get; }
        void InitUserSelector();
        void InitPersonSelector();
        void SetDateControlTexts();
    }
}
