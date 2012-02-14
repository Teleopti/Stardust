using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for classes that are acting as value in ISettingData
    /// </summary>
    public interface ISettingValue
    {
        /// <summary>
        /// Gets the ISettingData this ISettingValue belongs to.
        /// </summary>
        ISettingData BelongsTo { get; }

        /// <summary>
        /// Sets the owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <remarks>
        /// Isn't supposed to be explicitly. 
        /// Implemented explicitly in base clase SettingValue
        /// 
        /// Created by: rogerkr
        /// Created date: 2009-08-27
        /// </remarks>
        void SetOwner(ISettingData owner);
    }
}