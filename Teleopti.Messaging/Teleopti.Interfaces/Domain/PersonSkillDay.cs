namespace Teleopti.Interfaces.Domain
{
	public struct PersonSkillDay
	{
		private readonly ISkill[] _skills;
		private readonly DateTimePeriod _period;
		private readonly ITeam _team;
		private readonly ISkill[] _maxSeatSkills;
		private readonly ISkill[] _nonBlendSkills;

		public PersonSkillDay(DateTimePeriod period, ITeam team, ISkill[] skills, ISkill[] maxSeatSkills, ISkill[] nonBlendSkills)
		{
			_period = period;
			_team = team;
			_skills = skills;
			_maxSeatSkills = maxSeatSkills;
			_nonBlendSkills = nonBlendSkills;
		}

		public DateTimePeriod Period()
		{
			return _period;
		}

		public ITeam Team()
		{
			return _team;
		}

		public ISkill[] Skills()
		{
			return _skills;
		}

		public ISkill[] MaxSeatSkills()
		{
			return _maxSeatSkills;
		}

		public ISkill[] NonBlendSkills()
		{
			return _nonBlendSkills;
		}
	}
}