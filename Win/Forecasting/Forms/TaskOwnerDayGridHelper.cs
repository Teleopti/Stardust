using System;
using System.Globalization;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    internal static class TaskOwnerDayGridHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.TimePeriod.ToShortTimeString")]
        static void TemplateDayTooltipText(GridStyleInfo gridStyleInfo, ITemplateDay templateDay)
        {
            ITaskOwner taskOwner = templateDay as ITaskOwner;
            if (taskOwner == null)
			{
				var multisiteDay = templateDay as MultisiteDay;
				if (multisiteDay == null)
					return;
				gridStyleInfo.CellTipText = templateDay.TemplateReference.TemplateName;
				return;
            }
            if (taskOwner.IsClosed)
            {
                gridStyleInfo.CellTipText = UserTexts.Resources.Closed;
                return;
            }
            StringBuilder openHours;
            WorkloadDay workloadDay = templateDay as WorkloadDay;
            if (workloadDay == null)
            {
                SkillDay skillDay = templateDay as SkillDay;

                if (skillDay == null)
					return;
                gridStyleInfo.CellTipText = templateDay.TemplateReference.TemplateName;
                return;
            }

            openHours = new StringBuilder(workloadDay.OpenHourList[0].ToShortTimeString());
            if (workloadDay.OpenHourList[0].EndTime.Days == 1)
                openHours.Append(" +1");

            gridStyleInfo.CellTipText = templateDay.TemplateReference.TemplateName + '\n' + openHours;
        }

        internal static void SetTemplateCellStyle(GridStyleInfo gridStyle, ITemplateDay realTemplateDay, DayOfWeek dayOfWeek)
        {
            gridStyle.TextColor = ColorHelper.GridControlGridCellStandardTemplate();
            gridStyle.Font.Bold = true;
            gridStyle.Enabled = true;
            gridStyle.ReadOnly = true;

            if (realTemplateDay != null)
            {
                if (!realTemplateDay.TemplateReference.DayOfWeek.HasValue ||
                    dayOfWeek != realTemplateDay.TemplateReference.DayOfWeek)
                {
                    gridStyle.TextColor = ColorHelper.GridControlGridCellSpecialTemplate();
                }
                if (realTemplateDay.TemplateReference.TemplateName == "<" + UserTexts.Resources.None.ToUpper(CultureInfo.CurrentCulture) + ">"
                    || realTemplateDay.TemplateReference.TemplateName == "<NONE>")
                    gridStyle.TextColor = ColorHelper.GridControlGridCellNoTemplate();
                if (realTemplateDay.TemplateReference.DayOfWeek.HasValue && realTemplateDay.TemplateReference.TemplateName != "<" + CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(realTemplateDay.TemplateReference.DayOfWeek.Value)
                .ToUpper(CultureInfo.CurrentUICulture) + ">")
                    gridStyle.TextColor = ColorHelper.GridControlGridCellOldStandardTemplate();
                TemplateDayTooltipText(gridStyle, realTemplateDay);
            }
        }

        internal static void ResetClosedDay(ITaskOwner day)
        {
            day.ResetTaskOwner();
        }
    }
}