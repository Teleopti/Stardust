namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    /// <summary>
    /// Represents the search criteria.
    /// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 08-07-2008
	/// </remarks>
    public class SearchCriteria
    {
		#region Fields - Instance Member

        /// <summary>
        /// Specify the text to be searched.
        /// </summary>
        private string searchText;

		/// <summary>
        /// Specify whether the search needs to match the case.
        /// </summary>
        private bool isCaseSensitive;

		#endregion

		#region Properties - Instance Members

        /// <summary>
        /// Gets/Sets the search text of the search criteria
        /// </summary>
        public string SearchText
        {
            get { return this.searchText; }
            set { this.searchText = value; }
        }
 
        /// <summary>
        /// Get/Sets the isCaseSensitive property.
        /// </summary>
		public bool IsCaseSensitive
		{
			get { return this.isCaseSensitive; }
			set { this.isCaseSensitive = value; }
		}

		#endregion
	}
}
