using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// A wrapper for the custom efficiency shrinkage values
    ///</summary>
    public interface ICustomEfficiencyShrinkageWrapper
    {
        ///<summary>
        /// Sets the efficiency shrinkage value for the given defined custom efficiency shrinkage.
        ///</summary>
        ///<param name="customEfficiencyShrinkageId"></param>
        ///<param name="percent"></param>
        void SetEfficiencyShrinkage(Guid customEfficiencyShrinkageId, Percent percent);
        
        ///<summary>
        /// Gets the efficiency shrinkage value for the given defined custom efficiency shrinkage.
        ///</summary>
        ///<param name="customEfficiencyShrinkageId"></param>
        ///<returns></returns>
        Percent GetEfficiencyShrinkage(Guid customEfficiencyShrinkageId);

        /// <summary>
        /// Gets the total change factor.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/18/2010
        /// </remarks>
        Percent GetTotal();
    }
}