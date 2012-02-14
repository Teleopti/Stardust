using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction
{
    [Serializable]
    public class RestrictionSummaryViewCellModel  : GridStaticCellModel, IRestrictionSummaryViewCellModel
    {
        private RestrictionSummaryPresenter _restrictionSummaryPresenter;
        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictionSummaryViewCellModel"/> class.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        protected RestrictionSummaryViewCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictionSummaryViewCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        public RestrictionSummaryViewCellModel(GridModel grid)
            : base(grid)
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
        public RestrictionSummaryPresenter RestrictionSummaryPresenter
        {
            get { return _restrictionSummaryPresenter; }
            set
            {
                _restrictionSummaryPresenter = value;
            }
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
            return new RestrictionSummaryViewCellRenderer(control, this);

        }
    }

    /// <summary>
    /// Renders the restriction cell
    /// </summary>
    public class RestrictionSummaryViewCellRenderer : GridStaticCellRenderer
    {
        private readonly IList<IPreferenceCellPainter> _cellPainters;
        private IRestrictionSummaryViewCellModel _model;

        public RestrictionSummaryViewCellRenderer(GridControlBase grid, GridStaticCellModel cellModel)
            : base(grid, cellModel)
        {
            _cellPainters = new List<IPreferenceCellPainter>
                                {
                                    new ScheduledPainter(),
                                    new AbsencePainter(grid),
                                    new AbsenceOnContractDayOffPainter(grid),
                                    new ScheduledDayOffPainter(grid),
                                    new ScheduledShiftPainter(grid),
                                    new PreferredDayOffPainter(grid),
                                    new PreferredShiftCategoryPainter(grid),
                                    new PreferredAbsencePainter(grid),
                                    new AbsenceOnContractDayOffPainter(grid),
                                    new PreferredExtendedPainter(grid),
                                    new PreferredAbsenceOnContractDayOffPainter(grid),
                                    new EffectiveRestrictionPainter(grid),
                                    new EffectiveRestrictionRtlPainter(grid),
                                    new NotValidPainter(),
                                    new MustHavePainter(),
                                    new ActivityPreferencePainter(),
                                    new DisabledPainter()
                                    //This one must be at the end of collection to get the right effect
                                };
            _model = (IRestrictionSummaryViewCellModel)cellModel;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "rowIndex-1")]
        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            int currentCell = ((rowIndex - 1)*7) + colIndex;
            IPreferenceCellData cellData;
            _model.RestrictionSummaryPresenter.CellDataCollection.TryGetValue(currentCell - 1, out cellData);
            g.SmoothingMode = SmoothingMode.HighQuality;

            base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);
            PreferenceRestriction preference = new PreferenceRestriction();
            var effectiveRestriction = cellData.EffectiveRestriction;

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;

            CultureInfo culture =
                TeleoptiPrincipal.Current.Regional.Culture;

            g.DrawString(cellData.TheDate.ToShortDateString(culture), Grid.Font, Brushes.Black, clientRectangle);

            foreach (IPreferenceCellPainter cellPainter in _cellPainters)
            {
                if (cellPainter.CanPaint(cellData))
                {
                    cellPainter.Paint(g, clientRectangle, cellData, preference, effectiveRestriction, format);
                }
            }
            format.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            _model = null;
            _cellPainters.Clear();
            base.Dispose(disposing);
        }
    }
}