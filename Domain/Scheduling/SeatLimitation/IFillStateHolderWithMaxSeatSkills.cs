using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public interface IFillStateHolderWithMaxSeatSkills
	{
		void Execute(int intervalLength);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class FillStateHolderWithMaxSeatSkills : IFillStateHolderWithMaxSeatSkills
	{
		private readonly IInitMaxSeatForStateHolder _initMaxSeatForStateHolder;

		public FillStateHolderWithMaxSeatSkills(IInitMaxSeatForStateHolder initMaxSeatForStateHolder)
		{
			_initMaxSeatForStateHolder = initMaxSeatForStateHolder;
		}

		public void Execute(int intervalLength)
		{
			_initMaxSeatForStateHolder.Execute(15);
		}
	}


	[RemoveMeWithToggle(Toggles.ResourcePlanner_MaxSeatsNew_40939)]
	public class DontFillStateHolderWithMaxSeatSkills : IFillStateHolderWithMaxSeatSkills
	{
		public void Execute(int intervalLength)
		{
		}
	}
}