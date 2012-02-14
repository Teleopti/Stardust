using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a LicenseVerificationResultDto object.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class LicenseVerificationResultDto
    {
        [DataMember(Name = "IsValidLicenseFound")]
        private bool _isValidLicenseFound;

        [DataMember(Name = "WarningCollection")]
        private readonly IList<FaultDto> _warningList = new List<FaultDto>();

        [DataMember(Name = "ExceptionCollection")]
        private readonly IList<FaultDto> _exceptionList = new List<FaultDto>();

        /// <summary>
        /// Gets a value indicating whether this instance is valid license found.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is valid license found; otherwise, <c>false</c>.
        /// </value>
        public bool IsValidLicenseFound
        {
            get
            {
                return _isValidLicenseFound;
            }
        }

        /// <summary>
        /// Gets or sets the name of the license holder.
        /// </summary>
        /// <value>The name of the license holder.</value>
        [DataMember]
        public string LicenseHolderName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is exception found.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is exception found; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsExceptionFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is warning found.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is warning found; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsWarningFound { get; set; }

        /// <summary>
        /// Gets the exception collection.
        /// </summary>
        /// <value>The exception collection.</value>
        public ReadOnlyCollection<FaultDto> ExceptionCollection
        {
            get { return new ReadOnlyCollection<FaultDto>(_exceptionList); }
        }

        /// <summary>
        /// Gets the warning collection.
        /// </summary>
        /// <value>The warning collection.</value>
        public ReadOnlyCollection<FaultDto> WarningCollection
        {
            get { return new ReadOnlyCollection<FaultDto>(_warningList); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseVerificationResultDto"/> class.
        /// </summary>
        /// <param name="isValidLicenseFound">if set to <c>true</c> [is valid license found].</param>
        public LicenseVerificationResultDto(bool isValidLicenseFound)
        {
            _isValidLicenseFound = isValidLicenseFound;
            LicenseHolderName = string.Empty;
        }

        /// <summary>
        /// Sets the valid license found true.
        /// </summary>
        public void SetValidLicenseFoundTrue()
        {
            _isValidLicenseFound = true;
        }

        /// <summary>
        /// Adds the exception to collection.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void AddExceptionToCollection(FaultDto exception)
        {
            _exceptionList.Add(exception);
        }

        /// <summary>
        /// Adds the warning to collection.
        /// </summary>
        /// <param name="warning">The warning.</param>
        public void AddWarningToCollection(FaultDto warning)
        {
            _warningList.Add(warning);
        }
    }
}