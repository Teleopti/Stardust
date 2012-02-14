namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Object that can create a visuallayerfactory
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-02-13
    /// </remarks>
    public interface IVisualLayerFactoryFactory
    {
        /// <summary>
        /// Gets the visual layer factory.
        /// </summary>
        /// <value>The visual layer factory.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-13
        /// </remarks>
        IVisualLayerFactory CreateVisualLayerFactory();
    }
}
