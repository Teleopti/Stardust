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
	}

	public class TeamBlockSchedulingOptions : ITeamBlockSchedulingOptions
	{
		public bool IsBlockScheduling(ISchedulingOptions schedulingOptions)
		{
			return schedulingOptions.UseTeamBlockPerOption && !schedulingOptions.UseGroupScheduling;
		}

		public bool IsTeamScheduling(ISchedulingOptions schedulingOptions)
		{
			return !schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseGroupScheduling;
		}

		public bool IsTeamBlockScheduling(ISchedulingOptions schedulingOptions)
		{
			return schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseGroupScheduling;
		}

		public bool IsBlockSchedulingWithSameShift(ISchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
				   schedulingOptions.UseTeamBlockSameShift &&
				   !schedulingOptions.UseTeamBlockSameStartTime &&
				   !schedulingOptions.UseTeamBlockSameShiftCategory &&
				   !schedulingOptions.UseTeamBlockSameEndTime;
		}

		public bool IsBlockSchedulingWithSameShiftCategory(ISchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
				   !schedulingOptions.UseTeamBlockSameShift &&
				   !schedulingOptions.UseTeamBlockSameStartTime &&
				   schedulingOptions.UseTeamBlockSameShiftCategory &&
				   !schedulingOptions.UseTeamBlockSameEndTime;
		}

		public bool IsBlockSchedulingWithSameStartTime(ISchedulingOptions schedulingOptions)
		{
			return IsBlockScheduling(schedulingOptions) &&
				   !schedulingOptions.UseTeamBlockSameShift &&
				   schedulingOptions.UseTeamBlockSameStartTime &&
				   !schedulingOptions.UseTeamBlockSameShiftCategory &&
				   !schedulingOptions.UseTeamBlockSameEndTime;
		}

		public bool IsTeamSchedulingWithSameStartTime(ISchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   !schedulingOptions.UseGroupSchedulingCommonCategory &&
				   !schedulingOptions.UseGroupSchedulingCommonEnd &&
				   schedulingOptions.UseGroupSchedulingCommonStart;
		}

		public bool IsTeamSchedulingWithSameEndTime(ISchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   !schedulingOptions.UseGroupSchedulingCommonCategory &&
				   schedulingOptions.UseGroupSchedulingCommonEnd &&
				   !schedulingOptions.UseGroupSchedulingCommonStart;
		}

		public bool IsTeamSchedulingWithSameShiftCategory(ISchedulingOptions schedulingOptions)
		{
			return IsTeamScheduling(schedulingOptions) &&
				   schedulingOptions.UseGroupSchedulingCommonCategory &&
				   !schedulingOptions.UseGroupSchedulingCommonEnd &&
				   !schedulingOptions.UseGroupSchedulingCommonStart;
		}

        public bool IsSingleAgentTeam(ISchedulingOptions schedulingOptions)
        {
            return schedulingOptions.GroupOnGroupPageForTeamBlockPer != null &&
                   schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";
        }
	}
}