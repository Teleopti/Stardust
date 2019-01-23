using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SingleAgentRestriction.PreferenceCellPainters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	[Serializable]
	public class AgentRestrictionsDetailViewCellModel : GridStaticCellModel, IAgentRestrictionsDetailViewCellModel
	{
		private IAgentRestrictionsDetailModel _agentRestrictionsDetailModel;
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentRestrictionsDetailViewCellModel"/> class.
        /// </summary>
        /// <param name="info">An object that holds all the data needed to serialize or deserialize this instance.</param>
        /// <param name="context">Describes the source and destination of the serialized stream specified by info.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
        protected AgentRestrictionsDetailViewCellModel(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentRestrictionsDetailViewCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-01-05
        /// </remarks>
		public AgentRestrictionsDetailViewCellModel(GridModel grid): base(grid)
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
		public IAgentRestrictionsDetailModel DetailModel
        {
			get { return _agentRestrictionsDetailModel; }
			set { _agentRestrictionsDetailModel = value; }
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
			return new AgentRestrictionsDetailViewCellRenderer(control, this);

        }
	}

	/// <summary>
	/// Renders the restriction cell
	/// </summary>
	public class AgentRestrictionsDetailViewCellRenderer : GridStaticCellRenderer
	{
		private readonly IList<IPreferenceCellPainter> _cellPainters;
		private IAgentRestrictionsDetailViewCellModel _model;

		public AgentRestrictionsDetailViewCellRenderer(GridControlBase grid, GridStaticCellModel cellModel) : base(grid, cellModel)
		{
			_cellPainters = new List<IPreferenceCellPainter>
                                {
									new AgentRestrictionsSchedulePainter(grid),
									new AgentRestrictionsPreferencePainter(grid),
									new EffectiveRestrictionPainter(grid),
									new EffectiveRestrictionRtlPainter(grid),
									new NotValidPainter(),
									new MustHavePainter(),
									new ActivityPreferencePainter(),
									new DisabledPainter()
                                    //This one must be at the end of collection to get the right effect
                                };
			_model = (IAgentRestrictionsDetailViewCellModel)cellModel;

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "rowIndex-1")]
		protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
		{
			var currentCell = ((rowIndex - 1) * 7) + colIndex;
			IPreferenceCellData cellData;
			_model.DetailModel.DetailData().TryGetValue(currentCell - 1, out cellData);
			
			g.SmoothingMode = SmoothingMode.HighQuality;

			base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);

			if (cellData == null) return;

			var preference = new PreferenceRestriction();
			var effectiveRestriction = cellData.EffectiveRestriction;

			using (var format = new StringFormat())
			{
				format.Alignment = StringAlignment.Center;

				CultureInfo culture =
					TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;

				g.DrawString(cellData.TheDate.ToShortDateString(culture), Grid.Font, Brushes.Black, clientRectangle);

				foreach (IPreferenceCellPainter cellPainter in _cellPainters)
				{
					if (cellPainter.CanPaint(cellData))
					{
						cellPainter.Paint(g, clientRectangle, cellData, preference, effectiveRestriction, format);
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			_model = null;
			_cellPainters.Clear();
			base.Dispose(disposing);
		}
	}
}
