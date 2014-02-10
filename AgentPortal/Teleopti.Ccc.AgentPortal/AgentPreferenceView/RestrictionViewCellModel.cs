using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.AgentPreferenceView.PreferenceCellPainters;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView
{
    [Serializable]
    public class RestrictionViewCellModel  : GridStaticCellModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictionViewCellModel"/> class.
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
        /// Initializes a new instance of the <see cref="RestrictionViewCellModel"/> class.
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
        private static IList<IPreferenceCellPainter> CellPainters;

        public RestrictionViewCellRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
            if (CellPainters==null)
            {
                CellPainters = new List<IPreferenceCellPainter>
                                   {
                                       new AbsencePainter(grid),
                                       new ExtendedTemplatePainter(grid),
                                       new ScheduledDayOffPainter(grid),
									   new ScheduledShiftPainter(grid),
                                       new PreferredDayOffPainter(grid),
                                       new PreferredShiftCategoryPainter(grid),
                                       new PreferredAbsencePainter(grid),
                                       new PreferredExtendedPainter(grid),
                                       new EffectiveRestrictionPainter(grid),
                                       new EffectiveRestrictionRtlPainter(grid),
                                       new NotValidatedPainter(),
                                       new MustHavePainter(),
                                       new PersonalAssignmentPainter(),
                                       new ActivityPreferencePainter(),
                                       new ViolatesNightlyRestPainter(grid),
                                       new DisabledPainter() //This one must be at the end of collection to get the right effect
                                   };
            }
        }

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;

            base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);

            PreferenceCellData cellValue = (PreferenceCellData) style.CellValue;
            Preference preference = cellValue.Preference;
            EffectiveRestriction effectiveRestriction = cellValue.EffectiveRestriction;

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            g.DrawString(cellValue.TheDate.ToShortDateString(),Grid.Font, Brushes.Black,clientRectangle);

            foreach (IPreferenceCellPainter cellPainter in CellPainters)
            {
                if (cellPainter.CanPaint(cellValue))
                {
                    cellPainter.Paint(g, clientRectangle, cellValue, preference, effectiveRestriction, format);
                }
            }
            format.Dispose();
        }
    }
}