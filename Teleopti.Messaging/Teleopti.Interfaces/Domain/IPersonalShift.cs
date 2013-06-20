namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Personal shift
    /// </summary>
	public interface IPersonalShift : IAggregateEntity, ILayerCollectionOwner<IActivity>, ICloneableEntity<IPersonalShift>
    {
        /// <summary>
        /// Get position of this personalshift in its assignment
        /// </summary>
        int OrderIndex { get; }
    }
}