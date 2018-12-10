namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Interface for MultiplicatorLayer
    ///</summary>
    public interface IMultiplicatorLayer : ILayer<IMultiplicator>
    {
       
        /// <summary>
        /// Gets the layer original period.
        /// </summary>
        /// <value>The layer original period.</value>
        DateTimePeriod LayerOriginalPeriod { get; set; }

        /// <summary>
        /// Gets the multiplicator definition set.
        /// </summary>
        /// <value>The multiplicator definition set.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-12-11
        /// </remarks>
        IMultiplicatorDefinitionSet MultiplicatorDefinitionSet { get; }
    }
}
