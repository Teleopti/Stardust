#region Imports

using System;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms.Grid;

#endregion

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{

    /// <summary>
    /// Represents thecell model for the double value type.
    /// </summary>
    /// <remarks>
    /// Created by: SavaniN
    /// Created date: 1/28/2009
    /// </remarks>
    [Serializable]
    public class DoubleCellModel : NumericCellModel
    {

        #region Fields - Instance Member

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - DoubleCellModel Members

        #endregion

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - DoubleCellModel Members

        protected DoubleCellModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleCellModel"/> class.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="numberOfDecimalPoints">The number of decimal points.</param>
        /// <remarks>
        /// Created by: SavaniN
        /// Created date: 1/28/2009
        /// </remarks>
        public DoubleCellModel(GridModel grid, int numberOfDecimalPoints)
            : base(grid)
        {
            NumberOfDecimals = numberOfDecimalPoints;
        }

        #endregion

        #endregion

    }

}
