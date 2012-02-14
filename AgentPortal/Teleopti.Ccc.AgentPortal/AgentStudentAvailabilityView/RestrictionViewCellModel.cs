using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView
{
    [Serializable]
    public class RestrictionViewCellModel : GridCellModelBase
    {
        protected RestrictionViewCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public RestrictionViewCellModel(GridModel model) : base(model)
        { 
        }

        public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
        {
            return string.Empty;
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Trace.WriteLine("GetObjectData called");
            base.GetObjectData(info, context);
        }

        public override GridCellRendererBase CreateRenderer(GridControlBase control)
        {
            return new RestrictionViewCellRenderer(control, this);

        }
    }

    public class RestrictionViewCellRenderer : GridStaticCellRenderer
    {
        private static IList<IStudentAvailabilityCellPainter> _cellPainters;

        public RestrictionViewCellRenderer(GridControlBase grid, RestrictionViewCellModel cellModel) : base(grid, cellModel)
        {
            if(_cellPainters == null)
            {
                _cellPainters = new List<IStudentAvailabilityCellPainter>
                                    {
                                        new AbsencePainter(grid),
                                        new ScheduledDayOffPainter(grid),
                                        new ScheduledShiftPainter(grid),
                                        new PersonalAssignmentPainter(),
                                        new StudentAvailabilityRestrictionsPainter(grid),
                                        new EffectiveRestrictionPainter(grid),
                                        new EffectiveRestrictionRtlPainter(grid),
                                        new NotValidatedPainter(),
                                        new DisabledPainter()
                                    };
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;

            base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);

            var cellValue = (StudentAvailabilityCellData)style.CellValue;
            var effectiveRestriction = cellValue.EffectiveRestriction;

            var format = new StringFormat {Alignment = StringAlignment.Center};
            g.DrawString(cellValue.TheDate.ToShortDateString(), Grid.Font, Brushes.Black, clientRectangle);

            foreach (var cellPainter in _cellPainters)
            {
                if (cellPainter.CanPaint(cellValue))
                {
                    cellPainter.Paint(g, clientRectangle, cellValue, effectiveRestriction, format);
                }
            }
            format.Dispose();
        }
    }
}