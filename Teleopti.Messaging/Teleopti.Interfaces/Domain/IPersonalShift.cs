namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Personal shift
    /// </summary>
	public interface IPersonalShift : IShift, IAggregateEntity, ILayerCollectionOwner<IActivity>
    {
        /// <summary>
        /// Get position of this personalshift in its assignment
        /// </summary>
        int OrderIndex { get; }
    }
}