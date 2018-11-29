using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// A wrapper for the custom shrinkage values
    ///</summary>
    public interface ICustomShrinkageWrapper
    {
        ///<summary>
        /// Sets the shrinkage value for the given defined custom shrinkage.
        ///</summary>
        ///<param name="customShrinkageId"></param>
        ///<param name="percent"></param>
        void SetShrinkage(Guid customShrinkageId, Percent percent);

        ///<summary>
        /// Gets the shrinkage value for the given defined custom shrinkage.
        ///</summary>
        ///<param name="customShrinkageId"></param>
        ///<returns></returns>
        Percent GetShrinkage(Guid customShrinkageId);

        /// <summary>
        /// Gets the shrinkage change factor.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 10/18/2010
        /// </remarks>
        Percent GetTotal();
    }
}