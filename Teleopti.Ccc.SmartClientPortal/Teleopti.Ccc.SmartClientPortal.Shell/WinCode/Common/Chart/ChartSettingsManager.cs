using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart
{
    /// <summary>
    /// This class is for getting the chartsettings for each row
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2008-05-09
    /// </remarks>
    public class ChartSettingsManager
    {

        private readonly IList<IChartSeriesSetting> _chartSettingsDefault = new List<IChartSeriesSetting>
                                                                                       {
            new ChartSeriesSetting("Tasks",Color.LightPink, ChartSeriesDisplayType.Bar, false, AxisLocation.Left),
            new ChartSeriesSetting("CampaignTasks",Color.White, ChartSeriesDisplayType.Line, false, AxisLocation.Left),
            new ChartSeriesSetting("AverageTaskTime", Color.LightGreen, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("CampaignTaskTime", Color.Chocolate, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("AverageAfterTaskTime", Color.LightBlue, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("CampaignAfterTaskTime", Color.Indigo, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("TotalTasks", Color.Red, ChartSeriesDisplayType.Bar, true, AxisLocation.Left),
            new ChartSeriesSetting("TotalAverageTaskTime", Color.Green, ChartSeriesDisplayType.Line, true, AxisLocation.Right),
            new ChartSeriesSetting("TotalAverageAfterTaskTime", Color.Blue, ChartSeriesDisplayType.Line, true, AxisLocation.Right),
            new ChartSeriesSetting("TotalStatisticCalculatedTasks", Color.Firebrick, ChartSeriesDisplayType.Bar, false, AxisLocation.Left),
            new ChartSeriesSetting("TotalStatisticAbandonedTasks", Color.Goldenrod, ChartSeriesDisplayType.Bar, false, AxisLocation.Left),
            new ChartSeriesSetting("TotalStatisticAnsweredTasks", Color.CornflowerBlue, ChartSeriesDisplayType.Bar, false, AxisLocation.Left),
            new ChartSeriesSetting("TotalStatisticAverageTaskTime", Color.Khaki, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("TotalStatisticAverageAfterTaskTime", Color.Orchid, ChartSeriesDisplayType.Line, false, AxisLocation.Right),

            new ChartSeriesSetting("Payload.TaskData.Tasks", Color.Red, ChartSeriesDisplayType.Bar, false, AxisLocation.Left),
            new ChartSeriesSetting("Payload.TaskData.AverageTaskTime", Color.Green, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("Payload.TaskData.AverageAfterTaskTime", Color.Blue, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("ServiceLevelPercent", Color.Ivory, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("ServiceLevelSeconds", Color.DeepSkyBlue, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("MinOccupancy", Color.Honeydew, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("MaxOccupancy", Color.NavajoWhite, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("MinimumPersons", Color.SeaShell, ChartSeriesDisplayType.Bar, false, AxisLocation.Right),
            new ChartSeriesSetting("MaximumPersons", Color.SandyBrown, ChartSeriesDisplayType.Bar, false, AxisLocation.Right),
            new ChartSeriesSetting("ManualAgents", Color.HotPink, ChartSeriesDisplayType.Line, false, AxisLocation.Right),
            new ChartSeriesSetting("Payload.ForecastedIncomingDemand", Color.YellowGreen, ChartSeriesDisplayType.Bar, false, AxisLocation.Right),
            new ChartSeriesSetting("Shrinkage", Color.YellowGreen, ChartSeriesDisplayType.Bar, false, AxisLocation.Right),
            new ChartSeriesSetting("Payload.CalculatedTrafficIntensityWithShrinkage", Color.YellowGreen, ChartSeriesDisplayType.Bar, false, AxisLocation.Right),
            new ChartSeriesSetting("Payload.CalculatedOccupancy", Color.YellowGreen, ChartSeriesDisplayType.Bar, false, AxisLocation.Right),
            new ChartSeriesSetting("ServiceLevelTimeSpan", Color.Orchid, ChartSeriesDisplayType.Bar, false, AxisLocation.Right),

            new ChartSeriesSetting("ForecastedDistributedDemandWithShrinkage", Color.DarkKhaki, ChartSeriesDisplayType.Bar, false, AxisLocation.Right),

            new ChartSeriesSetting("ScheduledHours", Color.DarkKhaki, ChartSeriesDisplayType.Line, true, AxisLocation.Left),
            new ChartSeriesSetting("ForecastedHours", Color.DarkSlateBlue, ChartSeriesDisplayType.Bar, true, AxisLocation.Left),
            new ChartSeriesSetting("CalculatedResource", Color.DarkKhaki, ChartSeriesDisplayType.Line, true, AxisLocation.Left),
            new ChartSeriesSetting("FStaff", Color.DarkSlateBlue, ChartSeriesDisplayType.Bar, true, AxisLocation.Left),
            new ChartSeriesSetting("RelativeDifference", Color.Tomato, ChartSeriesDisplayType.Line, true, AxisLocation.Right),

            new ChartSeriesSetting("StatAverageQueueTimeSeconds", Color.MediumAquamarine, ChartSeriesDisplayType.Line, true, AxisLocation.Left),
            new ChartSeriesSetting("ActiveAgents", Color.NavajoWhite, ChartSeriesDisplayType.Line, true, AxisLocation.Left)                                                            
        };

        

        public IList<IChartSeriesSetting> ChartSettingsDefault
        {
            get { return _chartSettingsDefault; }
        }

        
        

       
    }
}