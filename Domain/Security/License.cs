using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security
{
    /// <summary>
    /// Used for persisting the XML license to the database
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-12-03
    /// </remarks>
    public class License : AggregateRoot_Events_ChangeInfo_Versioned, ILicense
    {
        #region ILicense Members
        private string _xmlString;

        /// <summary>
        /// Gets or sets the XML string containing the license
        /// </summary>
        /// <value>The XML string.</value>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-11-18
        /// </remarks>
        public virtual string XmlString
        {
            get { return _xmlString; }
            set { _xmlString = value; }
        }

        #endregion

    }
}
