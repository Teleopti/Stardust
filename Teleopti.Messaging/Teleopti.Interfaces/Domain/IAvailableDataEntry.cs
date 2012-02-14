namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds one entry for the available data.
    /// </summary>
    public interface IAvailableDataEntry : IAuthorizationEntity
    {

        /// <summary>
        /// Gets or sets the key of the available data holder.
        /// </summary>
        /// <value>The name of the available data holder.</value>
        string AvailableDataHolderKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the available data holder.
        /// </summary>
        /// <value>The name of the available data holder.</value>
        string AvailableDataHolderName { get; set; }

        /// <summary>
        /// Gets or sets the available data holder description.
        /// </summary>
        /// <value>The available data holder description.</value>
        string AvailableDataHolderDescription { get; set; }

        /// <summary>
        /// Gets or sets the available data holder value.
        /// </summary>
        /// <value>The available data holder value.</value>
        string AvailableDataHolderValue { get; set; }
    }
}
