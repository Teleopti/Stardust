using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Interface for all forms and user controls that should have translations
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-03
    /// </remarks>
    public interface ILocalized
    {
        /// <summary>
        /// Sets the texts.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        void SetTexts();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets or sets the right to left setting.
        /// </summary>
        /// <value>The right to left setting.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        RightToLeft RightToLeft { get; set; }
    }
}
