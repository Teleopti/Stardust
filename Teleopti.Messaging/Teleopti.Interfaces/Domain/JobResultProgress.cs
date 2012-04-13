using System;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Progress information for a export/import background job.
    ///</summary>
    public class JobResultProgress : IJobResultProgress
    {
        /// <summary>
        /// Represent info of job result progress
        /// </summary>
        /// <param name="totalPercentage">Total percentage with default value of 100</param>
        public JobResultProgress(int totalPercentage = 100)
        {
            TotalPercentage = totalPercentage;
        }

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

        ///<summary>
        /// The total percentage.
        ///</summary>
        public int TotalPercentage { get; set; }
    }
}