using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockSchedulingOptions
	{
		bool IsBlockScheduling(SchedulingOptions schedulingOptions);
		bool IsTeamScheduling(SchedulingOptions schedulingOptions);
		bool IsTeamBlockScheduling(SchedulingOptions schedulingOptions);
		bool IsBlockSchedulingWithSameShift(SchedulingOptions schedulingOptions);
		bool IsBlockSchedulingWithSameShiftCategory(SchedulingOptions schedulingOptions);
		bool IsBlockSchedulingWithSameStartTime(SchedulingOptions schedulingOptions);
		bool IsBlockSchedulingWithSameEndTime(SchedulingOptions schedulingOptions);
		bool IsTeamSchedulingWithSameStartTime(SchedulingOptions schedulingOptions);
		bool IsTeamSchedulingWithSameEndTime(SchedulingOptions schedulingOptions);
		bool IsTeamSchedulingWithSameShiftCategory(SchedulingOptions schedulingOptions);
	    bool IsSingleAgentTeam(SchedulingOptions schedulingOptions);
		bool IsBlockSameStartTimeInTeamBlock(SchedulingOptions schedulingOptions);
		bool IsTeamSameStartTimeInTeamBlock(SchedulingOptions schedulingOptions);
		bool IsBlockSameShiftCategoryInTeamBlock(SchedulingOptions schedulingOptions);
		bool IsTeamSameEndTimeInTeamBlock(SchedulingOptions schedulingOptions);
		bool IsBlockSameShiftInTeamBlock(SchedulingOptions schedulingOptions);
		bool IsTeamSameShiftCategoryInTeamBlock(SchedulingOptions schedulingOptions);
		bool IsTeamSameActivityInTeamBlock(SchedulingOptions schedulingOptions);
		bool IsTeamSchedulingWithSameActivity(SchedulingOptions schedulingOptions);
		bool IsBlockWithSameShiftCategoryInvolved(SchedulingOptions schedulingOptions);
	}

	public class TeamBlockSchedulingOptions : ITeamBlockSchedulingOptions
	{
		public bool IsBlockScheduling(SchedulingOptions schedulingOptions)
		{
			return schedulingOptions.UseBlock && !schedulingOptions.UseTeam;
		}

		public bool IsTeamScheduling(SchedulingOptions schedulingOptions)
		{
			return !schedulingOptions.UseBlock && schedulingOptions.UseTeam;
		}

		public bool IsTeamBlockScheduling(SchedulingOptions schedulingOptions)
		{
			return schedulingOptions.UseBlock && schedulingOptions.UseTeam;
		}

		public bool IsBlockSchedulingWithSameShift(SchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
					schedulingOptions.BlockSameShift;
		}

		public bool IsBlockSchedulingWithSameShiftCategory(SchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
				   schedulingOptions.BlockSameShiftCategory;
		}

		public bool IsBlockSchedulingWithSameStartTime(SchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
				   schedulingOptions.BlockSameStartTime;
		}

		public bool IsBlockSchedulingWithSameEndTime(SchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
				   schedulingOptions.BlockSameEndTime;
		}

		public bool IsTeamSchedulingWithSameStartTime(SchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   schedulingOptions.TeamSameStartTime;
		}

		public bool IsTeamSchedulingWithSameEndTime(SchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   schedulingOptions.TeamSameEndTime;
		}

		public bool IsTeamSchedulingWithSameShiftCategory(SchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   schedulingOptions.TeamSameShiftCategory;
		}

		public bool IsBlockSameStartTimeInTeamBlock(SchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.BlockSameStartTime;
		}

		public bool IsTeamSameStartTimeInTeamBlock(SchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameStartTime;
		}
	
		public bool IsBlockSameShiftCategoryInTeamBlock(SchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.BlockSameShiftCategory;
		}

		public bool IsTeamSameEndTimeInTeamBlock(SchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameEndTime;
		}
	
		public bool IsBlockSameShiftInTeamBlock(SchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
					 schedulingOptions.BlockSameShift;
		}

		public bool IsTeamSameShiftCategoryInTeamBlock(SchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameShiftCategory;
		}
		
        public bool IsSingleAgentTeam(SchedulingOptions schedulingOptions)
        {
	        return schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type == GroupPageType.SingleAgent;
        }

		public bool IsTeamSameActivityInTeamBlock(SchedulingOptions schedulingOptions)
		{
			return IsTeamBlockScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameActivity;
		}

		public bool IsTeamSchedulingWithSameActivity(SchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
			       schedulingOptions.TeamSameActivity;
		}

		public bool IsBlockWithSameShiftCategoryInvolved(SchedulingOptions schedulingOptions)
		{
			var blockFinder = schedulingOptions.BlockFinderTypeForAdvanceScheduling;
			var isSingleDay = (blockFinder == BlockFinderType.SingleDay);
			return (!isSingleDay && schedulingOptions.BlockSameShiftCategory);
		}
	}
}