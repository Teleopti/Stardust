using System;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Progress information for a payroll export / background job.
    ///</summary>
    public class JobResultProgress : IJobResultProgress
    {
        ///<summary>
        /// The completed percentage.
        ///</summary>
        public int Percentage { get; set; }

        ///<summary>
        /// A message with the current action.
        ///</summary>
        public string Message { get; set; }

        ///<summary>
        /// The id of the payroll or job result this is progress for.
        ///</summary>
        public Guid JobResultId { get; set; }
    }
}