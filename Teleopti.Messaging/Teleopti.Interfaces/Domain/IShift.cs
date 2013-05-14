using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for shifts
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-03
    /// </remarks>
    public interface IShift : ILayerCollectionOwner<IActivity>, 
                                IProjectionSource,
                                IVisualLayerFactoryFactory,
                                ICloneableEntity<IShift>
    {
    }
}