namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Contains all the extension methods which belongs to Font.
    /// </summary>
    /// <remarks>
    /// Created By: kosalanp
    /// Created Date: 17-04-2008
    /// </remarks>
    public static class FontExtension
    {
        /// <summary>
        /// Make the font as bold.
        /// </summary>
        /// <param name="font">The font.</param>
        /// <returns>The bold font.</returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 17-04-2008
        /// </remarks>
        public static System.Drawing.Font ChangeToBold(this System.Drawing.Font font)
        {
            font = new System.Drawing.Font(font, System.Drawing.FontStyle.Bold);
            return font;
        }
        /// <summary>
        /// Make the font as regular.
        /// </summary>
        /// <param name="font">The font.</param>
        /// <returns>The regular font.</returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 17-04-2008
        /// </remarks>
        public static System.Drawing.Font ChangeToRegular(this System.Drawing.Font font)
        {
            font = new System.Drawing.Font(font, System.Drawing.FontStyle.Regular);
            return font;
        }
    }
}
