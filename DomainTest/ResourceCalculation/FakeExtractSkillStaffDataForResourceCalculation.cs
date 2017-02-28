using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	public class FakeExtractSkillStaffDataForResourceCalculation : IExtractSkillStaffDataForResourceCalculation
	{
		public IResourceCalculationData FakeResourceCalculationData { get; set; }

		public IResourceCalculationData ExtractResourceCalculationData(DateOnlyPeriod periodDateOnly)
		{
			return FakeResourceCalculationData;
		}

		public void DoCalculation(DateOnlyPeriod period, IResourceCalculationData resCalcData)
		{
			// do nothing
		}
	}
}