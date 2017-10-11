using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// This class contains validation result for the absence request.
    /// </summary>
    public class ValidatedRequest : IValidatedRequest
    {
		public ValidatedRequest()
		{
			AffectedTimePerAccount = new ConcurrentDictionary<IAccount, TimeSpan>();
		}

		public static readonly ValidatedRequest Valid = new ValidatedRequest {IsValid = true};
		/// <summary>
		/// Gets and set the validation errors if any.
		/// </summary>
		/// <value>Validation Errors</value>
		public string ValidationErrors { get; set; }

        /// <summary>
        /// Gets and set if the absence request is valid or not
        /// </summary>
        /// <value>Is Valid Absence Request</value>
        public bool IsValid { get; set; }

		public PersonRequestDenyOption? DenyOption { get; set; }

		public ConcurrentDictionary<IAccount,TimeSpan> AffectedTimePerAccount { get; set; }
	}
}
