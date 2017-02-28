using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Details about progress inside the payroll export.
    ///</summary>
    public interface IJobResultProgress
    {
        ///<summary>
        /// The completed percentage.
        ///</summary>
        int Percentage { get; set; }

        ///<summary>
        /// A message with the current action.
        ///</summary>
        string Message { get; set; }

        ///<summary>
        /// The id of the payroll result this is progress for.
        ///</summary>
        Guid JobResultId { get; set; }
        
        ///<summary>
        /// The total percentage.
        ///</summary>
        int TotalPercentage { get; set; }
    }
}