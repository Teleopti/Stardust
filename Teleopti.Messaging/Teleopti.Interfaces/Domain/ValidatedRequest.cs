namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// This class contains validation result for the absence request.
    /// </summary>
    public class ValidatedRequest:IValidatedRequest
    {
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
    }
}
