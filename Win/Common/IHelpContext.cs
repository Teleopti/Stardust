namespace Teleopti.Ccc.Win.Common
{
    public interface IHelpContext
    {
        /// <summary>
        /// Gets a value indicating whether this instance has help information.
        /// Default is True
        /// </summary>
        /// <value><c>true</c> if this instance has help information; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-01
        /// </remarks>
        bool HasHelp { get; }

        /// <summary>
        /// Gets the help id.
        /// </summary>
        /// <value>The help id.</value>
        string HelpId { get; }
    }
}