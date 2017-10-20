
namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface IPeriodDistribution
    {
        void ProcessLayers(IResourceCalculationDataContainerWithSingleOperation layerCollectionFilteredByPeriod, ISkill skill);

        double CalculateStandardDeviation();

        double PeriodDetailAverage { get; }

        double PeriodDetailsSum { get; }

        double[] GetSplitPeriodValues();

        double[] CalculateSplitPeriodRelativeValues();

        double DeviationAfterNewLayers(IVisualLayerCollection layerCollection);
    }
}
