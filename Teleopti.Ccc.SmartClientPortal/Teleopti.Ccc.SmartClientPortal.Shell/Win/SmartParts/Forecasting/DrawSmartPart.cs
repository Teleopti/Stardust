using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting
{
    public class DrawSmartPart : IDrawSmartPart, IDisposable
    {
        private readonly Font _font = new Font("Arial", 8, FontStyle.Regular);
        private readonly Font _fontBold = new Font("Arial", 8, FontStyle.Bold);
        private readonly Color _defaultColor = Color.Green;
        private const string DateFormat = "g";

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
                var details = workloadNames[index];
                string name = details.Name;

                if (details.CharactersToShow.HasValue && name.Length > details.CharactersToShow)
                {
                    name = name.Substring(0, (int)details.CharactersToShow) + UserTexts.Resources.ThreeDots;
                }

                else
                {
                    Size textSize = System.Windows.Forms.TextRenderer.MeasureText(name, DefaultFont);
                    while (textSize.Width > 90)
                    {
                        name = name.Substring(0, name.Length - 4) + UserTexts.Resources.ThreeDots;
                        textSize = System.Windows.Forms.TextRenderer.MeasureText(name, DefaultFont);
                    }
                    details.CharactersToShow = name.Length;
                }
                
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
                var details = workloadNames[(rowIndex - 1) % rowsOfOneScenario];
                string workloadName = details.Name;

                // check to truncate strings
                if (details.CharactersToShow.HasValue && workloadName.Length > details.CharactersToShow)
                {
                    workloadName = workloadName.Substring(0, (int)details.CharactersToShow) + UserTexts.Resources.ThreeDots;
                }
                // reduce the pixelwidth of string to fit UI
                else
                {
                    Size textSize = System.Windows.Forms.TextRenderer.MeasureText(workloadName, DefaultFont);
                    while (textSize.Width > 90)
                    { 
                        workloadName = workloadName.Substring(0, workloadName.Length - 4) + UserTexts.Resources.ThreeDots;
                        textSize = System.Windows.Forms.TextRenderer.MeasureText(workloadName, DefaultFont);
                    }
                    details.CharactersToShow = workloadName.Length;
                    workloadName = workloadName.Substring(0, (int)details.CharactersToShow);

                }

                drawProperties.Graphics.DrawString(workloadName, DefaultFont, Brushes.Black, drawProperties.NameColumnStartPosition, drawProperties.Bounds.Y);
            }
        }

        public void DrawForecasts(IDrawProperties drawProperties, ICollection<DateOnlyPeriod> periods, DateOnlyPeriod period, int colorIndex)
        {
            if (drawProperties == null) throw new ArgumentNullException("drawProperties");
            if (periods == null) throw new ArgumentNullException("periods");

            if (periods.Count > 0)
            {
                var startDate = period.StartDate;
                Calendar calendar = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture.Calendar;
                int totalDays = calendar.GetDaysInYear(calendar.GetYear(startDate.Date));
                double unitOfDraw = drawProperties.DrawingWidth / (totalDays * 1.0);

                Color color = DefaultColor;

                foreach (var timePeriod in periods)
                {
                    var startTime = timePeriod.StartDate;
                    int pointX = Convert.ToInt32(drawProperties.ProgressStartPosition + (startTime.Subtract(startDate)).Days * unitOfDraw);

                    int width =
                        Convert.ToInt32(
                            (timePeriod.EndDate.Subtract(startTime)).
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
                var endDate = period.EndDate;
                var startDate = period.StartDate;
                int totalDays = endDate.Subtract(startDate).Days + 1;
                double unitOfDraw = drawPositionAndWidth.DrawingWidth / (totalDays * 1.0);

                foreach (var timePeriod in periods)
                {
                    var startTime = timePeriod.StartDate;
                    var endTime = timePeriod.EndDate;
                    double start = drawPositionAndWidth.ProgressStartPosition + (startTime.Subtract(startDate)).Days * unitOfDraw;
                    double end = (timePeriod.EndDate.Subtract(startTime)).Days * unitOfDraw + start;

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
            if (entityUpdateInformation.LastUpdate.HasValue)
                lastUpdated = entityUpdateInformation.LastUpdate.Value.ToString(DateFormat, TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture);
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