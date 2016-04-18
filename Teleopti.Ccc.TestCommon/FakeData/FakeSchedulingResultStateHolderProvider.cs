using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

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