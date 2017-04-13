using System.Globalization;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public class Culture
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Culture"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="displayName">The display name.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-14
        /// </remarks>
        public Culture(int id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-14
        /// </remarks>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-14
        /// </remarks>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the display name of the language info by.
        /// </summary>
        /// <param name="displayValue">The display value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-16
        /// </remarks>
        public static Culture GetLanguageInfoByDisplayName(string displayValue)
        {
            Culture RetVal = new Culture(0, "");

            CultureInfo[] cInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            for (int i = 0; i < cInfo.Length - 1; i++)
            {
                if (cInfo[i].DisplayName == displayValue || cInfo[i].Name == displayValue)
                {
                    RetVal = new Culture(cInfo[i].LCID, displayValue);
                    break;
                }
            }
            return RetVal;
        }
    }
}
