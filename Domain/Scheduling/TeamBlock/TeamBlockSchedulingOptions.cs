using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSchedulingOptions
	{
		bool IsBlockScheduling(ISchedulingOptions schedulingOptions);
		bool IsTeamScheduling(ISchedulingOptions schedulingOptions);
		bool IsTeamBlockScheduling(ISchedulingOptions schedulingOptions);
		bool IsBlockSchedulingWithSameShift(ISchedulingOptions schedulingOptions);
		bool IsBlockSchedulingWithSameShiftCategory(ISchedulingOptions schedulingOptions);
		bool IsBlockSchedulingWithSameStartTime(ISchedulingOptions schedulingOptions);
		bool IsTeamSchedulingWithSameStartTime(ISchedulingOptions schedulingOptions);
		bool IsTeamSchedulingWithSameEndTime(ISchedulingOptions schedulingOptions);
		bool IsTeamSchedulingWithSameShiftCategory(ISchedulingOptions schedulingOptions);
	    bool IsSingleAgentTeam(ISchedulingOptions schedulingOptions);
		bool IsBlockSameStartTimeInTeamBlock(ISchedulingOptions schedulingOptions);
		bool IsTeamSameStartTimeInTeamBlock(ISchedulingOptions schedulingOptions);
		bool IsBlockSameShiftCategoryInTeamBlock(ISchedulingOptions schedulingOptions);
		bool IsTeamSameEndTimeInTeamBlock(ISchedulingOptions schedulingOptions);
		bool IsBlockSameShiftInTeamBlock(ISchedulingOptions schedulingOptions);
		bool IsTeamSameShiftCategoryInTeamBlock(ISchedulingOptions schedulingOptions);
		bool IsTeamSameActivityInTeamBlock(ISchedulingOptions schedulingOptions);
		bool IsTeamSchedulingWithSameActivity(ISchedulingOptions schedulingOptions);
		bool IsBlockWithSameShiftCategoryInvolved(ISchedulingOptions schedulingOptions);
	}

	public class TeamBlockSchedulingOptions : ITeamBlockSchedulingOptions
	{
		public bool IsBlockScheduling(ISchedulingOptions schedulingOptions)
		{
			return schedulingOptions.UseBlock && !schedulingOptions.UseGroupScheduling;
		}

		public bool IsTeamScheduling(ISchedulingOptions schedulingOptions)
		{
			return !schedulingOptions.UseBlock && schedulingOptions.UseGroupScheduling;
		}

		public bool IsTeamBlockScheduling(ISchedulingOptions schedulingOptions)
		{
			return schedulingOptions.UseBlock && schedulingOptions.UseGroupScheduling;
		}

		public bool IsBlockSchedulingWithSameShift(ISchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
					schedulingOptions.BlockSameShift;
		}

		public bool IsBlockSchedulingWithSameShiftCategory(ISchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
				   schedulingOptions.BlockSameShiftCategory;
		}

		public bool IsBlockSchedulingWithSameStartTime(ISchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
				   schedulingOptions.BlockSameStartTime;
		}

		public bool IsTeamSchedulingWithSameStartTime(ISchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   schedulingOptions.TeamSameStartTime;
		}

		public bool IsTeamSchedulingWithSameEndTime(ISchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   schedulingOptions.TeamSameEndTime;
		}

		public bool IsTeamSchedulingWithSameShiftCategory(ISchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   schedulingOptions.TeamSameShiftCategory;
		}

		public bool IsBlockSameStartTimeInTeamBlock(ISchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.BlockSameStartTime;
		}

		public bool IsTeamSameStartTimeInTeamBlock(ISchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameStartTime;
		}
	
		public bool IsBlockSameShiftCategoryInTeamBlock(ISchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.BlockSameShiftCategory;
		}

		public bool IsTeamSameEndTimeInTeamBlock(ISchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameEndTime;
		}
	
		public bool IsBlockSameShiftInTeamBlock(ISchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
					 schedulingOptions.BlockSameShift;
		}

		public bool IsTeamSameShiftCategoryInTeamBlock(ISchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameShiftCategory;
		}
		
        public bool IsSingleAgentTeam(ISchedulingOptions schedulingOptions)
        {
            return schedulingOptions.GroupOnGroupPageForTeamBlockPer != null &&
                   schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";
        }

		public bool IsTeamSameActivityInTeamBlock(ISchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameActivity;
		}

		public bool IsTeamSchedulingWithSameActivity(ISchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameActivity;
		}

		public bool IsBlockWithSameShiftCategoryInvolved(ISchedulingOptions schedulingOptions)
		{
			var blockFinder = schedulingOptions.BlockFinderTypeForAdvanceScheduling;
			var isNoneOrSingleDay = (blockFinder == BlockFinderType.None || blockFinder == BlockFinderType.SingleDay);
			return (!isNoneOrSingleDay && schedulingOptions.BlockSameShiftCategory);
		}
	}
}