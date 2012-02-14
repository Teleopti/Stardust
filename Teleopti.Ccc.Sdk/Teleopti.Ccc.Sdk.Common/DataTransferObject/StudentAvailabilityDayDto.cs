using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Class for details about student availability for one person and day.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class StudentAvailabilityDayDto : Dto
    {
        private IList<StudentAvailabilityRestrictionDto> _studentAvailabilityRestrictions = new List<StudentAvailabilityRestrictionDto>();

        /// <summary>
        /// Gets the student availability restrictions.
        /// </summary>
        /// <remarks>Currently only one restriction is supported.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The setter is used in serialization."), DataMember]
        public IList<StudentAvailabilityRestrictionDto> StudentAvailabilityRestrictions
        {
            get { return _studentAvailabilityRestrictions; }
            private set
            {
                if (value!=null)
                {
                    _studentAvailabilityRestrictions = new List<StudentAvailabilityRestrictionDto>(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        [DataMember]
        public DateOnlyDto RestrictionDate { get; set; }

        /// <summary>
        /// Gets or sets an indication wheter or not the person is available during the time given in the restriction.
        /// </summary>
        /// <remarks>This is currently not used and all student availability restrictions are considered available.</remarks>
        [DataMember]
        public bool NotAvailable { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        [DataMember]
        public PersonDto Person { get; set; }
    }
}
