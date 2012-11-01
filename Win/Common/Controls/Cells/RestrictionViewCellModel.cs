using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Ccc.Win.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
    public class RestrictionViewCellModel  : GridStaticCellModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCellModel"/> class.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        protected RestrictionViewCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public RestrictionViewCellModel(GridModel grid)
            : base(grid)
        {
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            return string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Trace.WriteLine("GetObjectData called");
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Creates the renderer.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new RestrictionViewCellRenderer(control, this);

        }
    }

    /// <summary>
    /// Renders the restriction cell
    /// </summary>
    public class RestrictionViewCellRenderer : GridStaticCellRenderer
    {
        private Image _startImage = Resources.StartTime;
        private Image _endImage = Resources.EndTime;
        private Image _workImage = Resources.WorkTime;
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericCellRenderer"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="cellModel">The cell model.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public RestrictionViewCellRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);
            RestrictionCellValue cellValue = (RestrictionCellValue) style.CellValue;
            IScheduleDay schedulePart = cellValue.SchedulePart;

            RestrictionExtractor extractor = new RestrictionExtractor(null);
            extractor.Extract(schedulePart);
            IEffectiveRestriction effective =
                extractor.CombinedRestriction(new SchedulingOptions
                                                  {
                                                      UseRotations = cellValue.UseRotation,
                                                      UsePreferences = cellValue.UsePreference,
                                                      UseAvailability = cellValue.UseAvailability,
                                                      UseStudentAvailability = cellValue.UseStudentAvailability,
                                                      UsePreferencesMustHaveOnly = false
                                                  });

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            if (cellValue.UseSchedule)
            {
                var significantPart = schedulePart.SignificantPart(); 
                if (significantPart == SchedulePartView.MainShift)
                {
                    drawSchedule(g, clientRectangle, schedulePart, format);
                    return;
                }
                if (significantPart == SchedulePartView.FullDayAbsence)
                {
                    IAbsence absence = ScheduleViewBase.SignificantAbsence(schedulePart);
                    drawAbsence(clientRectangle, absence, g, format);
                    return;
                }
                if (significantPart == SchedulePartView.DayOff)
                {
                    DayOff dayOff = schedulePart.PersonDayOffCollection()[0].DayOff;
                    drawDayOff(clientRectangle, dayOff, g, format);
                    return;
                }
                if(significantPart == SchedulePartView.ContractDayOff)
                {
                    IAbsence absence = ScheduleViewBase.SignificantAbsence(schedulePart);
                    drawContractDayOff(clientRectangle, absence, g, format);
                }
            }

            if (effective == null)
            {
                g.DrawString("NULL", Grid.Font, Brushes.Black, clientRectangle);
                return;
            }

            if (effective.DayOffTemplate != null)
            {
                drawDayOff(clientRectangle, new DayOff(schedulePart.Period.StartDateTime.AddHours(12), effective.DayOffTemplate.TargetLength, effective.DayOffTemplate.Flexibility, effective.DayOffTemplate.Description, effective.DayOffTemplate.DisplayColor, effective.DayOffTemplate.PayrollCode), g, format);
                return;
            }

            if (effective.ShiftCategory != null)
            {
                IShiftCategory category = effective.ShiftCategory;
                drawShiftCategory(clientRectangle, category, g, format);
            }  

            if (!string.IsNullOrEmpty(limitationToString(effective.StartTimeLimitation)))
                g.DrawImage(_startImage, clientRectangle.Left + 8, clientRectangle.Top + 18);
                g.DrawString(limitationToString(effective.StartTimeLimitation), Grid.Font, Brushes.Black, clientRectangle.Left + 8 + _startImage.Width + 2, clientRectangle.Top + 16);

            if (!string.IsNullOrEmpty(limitationToString(effective.EndTimeLimitation)))
                g.DrawImage(_workImage, clientRectangle.Left + 8, clientRectangle.Top + 30);
                g.DrawString(limitationToString(effective.EndTimeLimitation), Grid.Font, Brushes.Black, clientRectangle.Left + 8 + _startImage.Width + 2, clientRectangle.Top + 28);

            if (!string.IsNullOrEmpty(limitationToString(effective.WorkTimeLimitation)))
                g.DrawImage(_workImage, clientRectangle.Left + 8, clientRectangle.Top + 42);
                g.DrawString(limitationToString(effective.WorkTimeLimitation), Grid.Font, Brushes.Black, clientRectangle.Left + 8 + _startImage.Width + 2, clientRectangle.Top + 40);
        }

        private void drawSchedule(Graphics g, Rectangle clientRectangle, IScheduleDay schedulePart, StringFormat format)
        {
            IShiftCategory category = schedulePart.AssignmentHighZOrder().MainShift.ShiftCategory;

            drawShiftCategory(clientRectangle, category, g, format);

            IVisualLayerCollection visualLayers = schedulePart.AssignmentHighZOrder().ProjectionService().CreateProjection();
            g.DrawImage(_startImage, clientRectangle.Left + 8, clientRectangle.Top + 18);
            g.DrawString(
                visualLayers.Period().Value.StartDateTime.ToShortTimeString(),
                Grid.Font, Brushes.Black, clientRectangle.Left + 8 + _startImage.Width + 8, clientRectangle.Top + 16);

            g.DrawImage(_endImage, clientRectangle.Left + 8, clientRectangle.Top + 30);
            g.DrawString(
                visualLayers.Period().Value.EndDateTime.ToShortTimeString(),
                Grid.Font, Brushes.Black, clientRectangle.Left + 8 + _startImage.Width + 8, clientRectangle.Top + 28);

            g.DrawImage(_workImage, clientRectangle.Left + 8, clientRectangle.Top + 42);
            g.DrawString(DateTime.MinValue.Add(visualLayers.ContractTime()).ToShortTimeString(),
                         Grid.Font, Brushes.Black, clientRectangle.Left + 8 + _startImage.Width + 8, clientRectangle.Top + 40);
        }

        private void drawShiftCategory(Rectangle clientRectangle, IShiftCategory category, Graphics g, StringFormat format)
        {
            using (LinearGradientBrush lBrush = new LinearGradientBrush(clientRectangle, Color.WhiteSmoke, category.DisplayColor, 90, false))
            {
                GridHelper.FillRoundedRectangle(g, clientRectangle, 7, lBrush, -4);
            }
            g.DrawString(longOrShortText(category.Description, clientRectangle.Width - 8, g), Grid.Font, Brushes.Black, middleX(clientRectangle), clientRectangle.Top + 4, format);
        }

        private void drawAbsence(Rectangle clientRectangle, IAbsence absence, Graphics g, StringFormat format)
        {
            Rectangle rect;
            using (LinearGradientBrush lBrush = new LinearGradientBrush(clientRectangle, Color.WhiteSmoke, absence.DisplayColor, 90, false))
            {
                rect = new Rectangle(clientRectangle.Location, clientRectangle.Size);
                rect.Inflate(-4, -4);
                g.FillRectangle(lBrush, rect);
            }
            g.DrawString(longOrShortText(absence.Description, rect.Width, g), Grid.Font, Brushes.Black, middleX(clientRectangle), clientRectangle.Top + 4, format);
        }

        private void drawContractDayOff(Rectangle clientRectangle, IAbsence absence, Graphics g, StringFormat format)
        {
            Rectangle rect;
            using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, absence.DisplayColor, Color.LightGray))
            {
                rect = new Rectangle(clientRectangle.Location, clientRectangle.Size);
                rect.Inflate(-4, -4);
                g.FillRectangle(brush, rect);
            }
            g.DrawString(longOrShortText(absence.Description, rect.Width, g), Grid.Font, Brushes.Black, middleX(clientRectangle), clientRectangle.Top + 4, format);
        }

        private void drawDayOff(Rectangle clientRectangle, DayOff dayOff, Graphics g, StringFormat format)
        {
            Rectangle rect = new Rectangle(clientRectangle.Location, clientRectangle.Size);
            rect.Inflate(-4, -4);
            using (HatchBrush brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, dayOff.DisplayColor, Color.LightGray))
            {
                g.FillRectangle(brush, rect);
            }
            g.DrawString(longOrShortText(dayOff.Description, rect.Width, g), Grid.Font, Brushes.Black, middleX(clientRectangle), clientRectangle.Top + 4, format);
        }

        private string longOrShortText(Description description, int maxWidth, Graphics g)
        {
            string text = description.Name;
            SizeF size = g.MeasureString(text, Grid.Font);
            if (size.Width > maxWidth)
                text = description.ShortName;
            return text;
        }

        private static float middleX(Rectangle clientRectangle)
        {
            return clientRectangle.Left + (clientRectangle.Width/2);
        }

        private static string limitationToString(ILimitation limitation)
        {
            if (!limitation.StartTime.HasValue && !limitation.EndTime.HasValue)
                return string.Empty;

            string start = "N/A";
            if (limitation.StartTime.HasValue)
                start = limitation.StartTimeString;

            string end = "N/A";
            if (limitation.EndTime.HasValue)
                end = limitation.EndTimeString;

            return string.Concat(start, " -- ", end);
        }
    }
}