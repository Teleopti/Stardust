using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSchedulingResultStateHolderProvider : SchedulingResultStateHolderProvider
	{
		private readonly ISchedulingResultStateHolder _holder;
		public FakeSchedulingResultStateHolderProvider(ISchedulingResultStateHolder holder)
		{
			_holder = holder;
		}
		public override ISchedulingResultStateHolder GiveMeANew()
		{
			return _holder;
		}
	}
}