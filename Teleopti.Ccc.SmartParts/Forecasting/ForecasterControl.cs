#region Imports

using System.Collections.Generic;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Interfaces.Domain;
#endregion

namespace Teleopti.Ccc.SmartParts.Forecasting
{
    /// <summary>
    /// Forecasts skills
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 9/24/2008
    /// </remarks>
    public partial class ForecasterControl : UserControl
    {
        #region Fields - Instance Member

        private readonly IDrawingBehavior drawing;

        #endregion

        #region Properties - Instance Member - ForecasterControl Members

        /// <summary>
        /// Gets the forecaster grid.
        /// </summary>
        /// <value>The forecaster grid.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 9/24/2008
        /// </remarks>
        public GridControl ForecasterGrid
        {
            get { return gridControlForecaster; }
        }

        #endregion

        #region Member - ForecasterControl Members - (constructors)

        /// <summary>
        /// Initializes a new instance of the <see cref="ForecasterControl"/> class.
        /// </summary>
        /// <param name="drawingBehavior">The drawing behavior.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 9/24/2008
        /// </remarks>
        public ForecasterControl(IDrawingBehavior drawingBehavior)
        {
            drawing = drawingBehavior;
            InitializeComponent();
        }

        #endregion

        #region Member - ForecasterControl Members - (event handlers)

        /// <summary>
        /// Handles the CellDrawn event of the gridControlForecaster control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 9/24/2008
        /// </remarks>
        private void gridControlForecaster_CellDrawn(object sender, Syncfusion.Windows.Forms.Grid.GridDrawCellEventArgs e)
        {
            if (e.ColIndex > 1 && e.RowIndex > 0)
            {
                drawing.DrawGraphs(e.Graphics, e.RowIndex - 1, e.Bounds);
            }

            if (e.ColIndex == 1)
            {
                drawing.DrawNames(e.Graphics, e.RowIndex - 1, e.Bounds);
            }

        }


        #endregion
    }
}