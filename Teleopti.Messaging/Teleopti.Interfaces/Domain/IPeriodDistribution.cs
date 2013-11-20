
namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// An interface for containing splitted interval data
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-10-27    
    /// /// </remarks>
    public interface IPeriodDistribution
    {
        /// <summary>
        /// Processes the layers.
        /// </summary>
        /// <param name="layerCollectionFilteredByPeriod">The layer collection.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// /// </remarks>
        void ProcessLayers(IResourceCalculationDataContainerWithSingleOperation layerCollectionFilteredByPeriod);

        /// <summary>
        /// Calculates the standard deviation.
        /// </summary>
        /// <returns></returns>
        double CalculateStandardDeviation();

        /// <summary>
        /// Gets the period detail average.
        /// </summary>
        /// <value>The period detail average.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// /// </remarks>
        double PeriodDetailAverage { get; }

        /// <summary>
        /// Gets the period details sum.
        /// </summary>
        /// <value>The period details sum.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// /// </remarks>
        double PeriodDetailsSum { get; }

        /// <summary>
        /// Gets the splitted period values.
        /// </summary>
        /// <value>The splitted period values.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-10-27    
        /// /// </remarks>
        double[] GetSplitPeriodValues();

        /// <summary>
        /// Gets the split period relative values.
        /// </summary>
        /// <returns></returns>
        double[] CalculateSplitPeriodRelativeValues();

        /// <summary>
        /// Deviations the after new layers.
        /// </summary>
        /// <param name="layerCollection">The layer collection.</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-05-05    
        /// /// </remarks>
        double DeviationAfterNewLayers(IVisualLayerCollection layerCollection);
    }
}
