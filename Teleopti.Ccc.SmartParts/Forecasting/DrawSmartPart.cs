using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartParts.Forecasting
{
    public class DrawSmartPart : IDrawSmartPart, IDisposable
    {
        private readonly Font _font = new Font("Arial", 8, FontStyle.Regular);
        private readonly Font _fontBold = new Font("Arial", 8, FontStyle.Bold);
        private readonly Color _defaultColor = Color.Green;
        private const string DateFormat = "g";
        private const int OptimalWorkloadNameLength = 15;  // maximum length of the workload which can be displayed as Workload Name on forecast window.

        public Font DefaultFont
        {
            get { return _font; }
        }

        public Font DefaultFontBold
        {
            get { return _fontBold; }
        }

        public Color DefaultColor
        {
            get { return _defaultColor; }
        }

        public void DrawWorkloadNames(IDrawProperties drawProperties, IList<NamedEntity> workloadNames, int index)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            if (workloadNames == null) throw new ArgumentNullException("workloadNames");

            if (index < workloadNames.Count)
            {
                string name = workloadNames[index].Name;
                
                // if workload name length is greater than optimal workload name then we truncate the name till optimal workload length
                if (name.Length > OptimalWorkloadNameLength)
                    name = name.Substring(0, OptimalWorkloadNameLength) + "...";
                
                drawProperties.Graphics.DrawString(name, DefaultFont, Brushes.Black, 0, drawProperties.Bounds.Y);
            }
        }

        public void DrawScenarioNames(IDrawProperties drawProperties, IList<NamedEntity> scenarioNames, IList<NamedEntity> workloadNames)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            if (scenarioNames == null) throw new ArgumentNullException("scenarioNames");
            if (workloadNames == null) throw new ArgumentNullException("workloadNames");

            int totalScenarios = scenarioNames.Count;
            int totalWorkloads = workloadNames.Count;
            int rowsOfOneScenario = totalWorkloads + 1;
            int rowIndex = drawProperties.RowIndex - 1;
            int colIndex = drawProperties.ColIndex - 1;
            if (rowIndex >= totalScenarios * rowsOfOneScenario) return;
            if (colIndex > 0) return;
            if (rowIndex % rowsOfOneScenario == 0)
            {
                string name = scenarioNames[rowIndex / rowsOfOneScenario].Name;
                SizeF size = drawProperties.Graphics.MeasureString(name, DefaultFontBold);
                float startPointX = (drawProperties.Bounds.Width - size.Width) * 0.5f;
                drawProperties.Graphics.DrawString(name, DefaultFontBold, Brushes.Black, startPointX, drawProperties.Bounds.Y);
            }
            else
            {
                string workloadName = workloadNames[(rowIndex - 1) % rowsOfOneScenario].Name;
                
                // if workload name length is greater than optimal workload name then we truncate the name till optimal workload length
                if (workloadName.Length > OptimalWorkloadNameLength)
                    workloadName = workloadName.Substring(0, OptimalWorkloadNameLength) + "...";
                
                drawProperties.Graphics.DrawString(workloadName, DefaultFont, Brushes.Black, drawProperties.NameColumnStartPosition, drawProperties.Bounds.Y);
            }
        }

        public void DrawForecasts(IDrawProperties drawProperties, ICollection<DateOnlyPeriod> periods, DateOnlyPeriod period, int colorIndex)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            if (periods == null) throw new ArgumentNullException("periods");

            if (periods.Count > 0 && StateHolderReader.IsInitialized && StateHolderReader.Instance.StateReader.IsLoggedIn)
            {
                var startDate = period.StartDate;
                Calendar calendar =
                    TeleoptiPrincipal.Current.Regional.Culture.Calendar;
                int totalDays = calendar.GetDaysInYear(calendar.GetYear(startDate));
                double unitOfDraw = drawProperties.DrawingWidth / (totalDays * 1.0);

                Color color = DefaultColor;

                foreach (var timePeriod in periods)
                {
                    DateTime startTime = timePeriod.StartDate;
                    int pointX = Convert.ToInt32(drawProperties.ProgressStartPosition + (startTime - startDate).Days * unitOfDraw);

                    int width =
                        Convert.ToInt32(
                            (timePeriod.EndDate - startTime).
                                Days * unitOfDraw);

                    if (width > 0)
                    {
                        Rectangle rectangle = new Rectangle(pointX, drawProperties.Bounds.Y + 2, width, drawProperties.Bounds.Height - 4);
                        using (LinearGradientBrush brush =
                            new LinearGradientBrush(rectangle, Color.WhiteSmoke, color, 90, false))
                        {
                            drawProperties.Graphics.FillRectangle(brush, rectangle);
                        }
                    }
                }
            }
        }

        public ToolTipInfo GetProgressGraphToolTip(IDrawPositionAndWidth drawPositionAndWidth, ICollection<DateOnlyPeriod> periods, DateOnlyPeriod period, int mouseX)
        {
            if (drawPositionAndWidth == null) throw new ArgumentNullException("drawPositionAndWidth");
            if (periods == null) throw new ArgumentNullException("periods");

            ToolTipInfo toolTipInfo = new ToolTipInfo();
            if (periods.Count > 0)
            {
                DateTime endDate = period.EndDate;
                DateTime startDate = period.StartDate;
                int totalDays = (endDate - startDate).Days + 1;
                double unitOfDraw = drawPositionAndWidth.DrawingWidth / (totalDays * 1.0);

                foreach (var timePeriod in periods)
                {
                    DateTime startTime = timePeriod.StartDate;
                    DateTime endTime = timePeriod.EndDate;
                    double start = drawPositionAndWidth.ProgressStartPosition + (startTime - startDate).Days * unitOfDraw;
                    double end = (timePeriod.EndDate - startTime).Days * unitOfDraw + start;

                    if (mouseX >= start && mouseX <= end)
                    {
                        toolTipInfo.Body.Text = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", startTime.ToShortDateString(), endTime.ToShortDateString());
                        break;
                    }
                }
            }
            return toolTipInfo;
        }

        public ToolTipInfo GetScenarioToolTip(IDrawPositionAndWidth drawPositionAndWidth, EntityUpdateInformation entityUpdateInformation, int mouseX)
        {
            if (drawPositionAndWidth == null) throw new ArgumentNullException("drawPositionAndWidth");
            if (entityUpdateInformation == null) throw new ArgumentNullException("entityUpdateInformation");

            ToolTipInfo toolTipInfo = new ToolTipInfo();
            if (mouseX >= drawPositionAndWidth.ProgressStartPosition) return toolTipInfo;

            string lastUpdated = string.Empty;
            if (entityUpdateInformation.LastUpdate.HasValue && StateHolderReader.IsInitialized && StateHolderReader.Instance.StateReader.IsLoggedIn)
                lastUpdated = entityUpdateInformation.LastUpdate.Value.ToString(DateFormat, TeleoptiPrincipal.Current.Regional.Culture);
            lastUpdated = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", UserTexts.Resources.LastUpdated, lastUpdated);
            string lastChangedByName = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", UserTexts.Resources.ChangedBy,
                                                     (entityUpdateInformation.Name.HasValue)
                                                         ? entityUpdateInformation.Name.Value.ToString()
                                                         : string.Empty);
            toolTipInfo.Header.Text = string.Format(CultureInfo.CurrentCulture, "{0}\r", entityUpdateInformation.Tag);
            toolTipInfo.Body.Text = string.Format(CultureInfo.CurrentCulture, "{0}\r{1}", lastUpdated, lastChangedByName);
            return toolTipInfo;
        }

        public ToolTipInfo GetWorkloadToolTip(IDrawPositionAndWidth drawPositionAndWidth, string workloadName, int mouseX)
        {
            if (drawPositionAndWidth == null) throw new ArgumentNullException("drawPositionAndWidth");
            if (workloadName == null) throw new ArgumentNullException("workloadName");

            ToolTipInfo toolTipInfo = new ToolTipInfo();
            if (mouseX >= drawPositionAndWidth.ProgressStartPosition) return toolTipInfo;
            toolTipInfo.Body.Text = string.Format(CultureInfo.CurrentCulture, "{0}", workloadName);
            return toolTipInfo;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool b)
        {
            if (b)
            {
                _font.Dispose();
                _fontBold.Dispose();
            }
        }
    }
}