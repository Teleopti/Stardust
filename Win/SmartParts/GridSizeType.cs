using System;

namespace Teleopti.Ccc.Win.SmartParts
{

    /// <summary>
    /// Represents a Grid Size of the Grid worksapce.
    /// </summary>
    [Serializable]
    public enum GridSizeType
    {
        None=0,

        /// <summary>
        /// One cell Grid layout
        /// </summary>
        OneByOne=1,

        /// <summary>
        /// Two row and once column Grid layout
        /// </summary>
        TwoByOne=2,

        /// <summary>
        /// Three row and one column Grid layout
        /// </summary>
        ThreeByOne=3,

        /// <summary>
        /// Two row and Two column Grid layout
        /// </summary>
        TwoByTwo=4,

        /// <summary>
        /// Three row and Three column Grid layout
        /// </summary>
        ThreeByThree=9
    }

}
