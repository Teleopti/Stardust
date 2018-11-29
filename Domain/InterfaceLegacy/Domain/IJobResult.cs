using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// The job result information.
    ///</summary>
    public interface IJobResult : IAggregateRoot, IVersioned
	{
        ///<summary>
        /// The job category (defined in <see cref="JobCategory"/>).
        ///</summary>
        string JobCategory { get; }

        ///<summary>
        /// The period.
        ///</summary>
        DateOnlyPeriod Period { get; set; }

        /// <summary>
        /// The start time for this job.
        /// </summary>
        DateTime Timestamp { get; }

        ///<summary>
        /// The owner of the job.
        ///</summary>
        IPerson Owner { get; }

        ///<summary>
        /// The detailed information about the job.
        ///</summary>
        IEnumerable<IJobResultDetail> Details { get; }

        ///<summary>
        /// Indicates if the job is finished.
        ///</summary>
        bool FinishedOk { get; set; }
        
        ///<summary>
        /// Adds detailed information about the job processing.
        ///</summary>
        ///<param name="jobResultDetail">The detailed information.</param>
        void AddDetail(IJobResultDetail jobResultDetail);

        ///<summary>
        /// Indicates if the job encountered any errors while processing.
        ///</summary>
        ///<returns></returns>
        bool HasError();

        ///<summary>
        /// Indicates if the job is currently running.
        ///</summary>
        ///<returns></returns>
        bool IsWorking();

	    IList<JobResultArtifact> Artifacts { get; }

		void AddArtifact(JobResultArtifact artifact);

    }

	
}