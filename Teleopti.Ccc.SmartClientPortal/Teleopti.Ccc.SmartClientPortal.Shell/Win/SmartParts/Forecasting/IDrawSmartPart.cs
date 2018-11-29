using System.Collections.Generic;
using System.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    public interface IDrawSmartPart
    {
        Font DefaultFont { get; }
        Font DefaultFontBold { get; }
        void DrawWorkloadNames(IDrawProperties drawProperties, IList<NamedEntity> workloadNames, int index);
        void DrawScenarioNames(IDrawProperties drawProperties, IList<NamedEntity> scenarioNames, IList<NamedEntity> workloadNames);
        void DrawForecasts(IDrawProperties drawProperties, ICollection<DateOnlyPeriod> periods, DateOnlyPeriod period, int colorIndex);
        ToolTipInfo GetProgressGraphToolTip(IDrawPositionAndWidth drawPositionAndWidth, ICollection<DateOnlyPeriod> periods, DateOnlyPeriod period, int mouseX);
        ToolTipInfo GetScenarioToolTip(IDrawPositionAndWidth drawPositionAndWidth, EntityUpdateInformation entityUpdateInformation, int mouseX);
        ToolTipInfo GetWorkloadToolTip(IDrawPositionAndWidth drawPositionAndWidth, string workloadName, int mouseX);
    }
}