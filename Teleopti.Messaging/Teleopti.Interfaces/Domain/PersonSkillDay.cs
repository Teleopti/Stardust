namespace Teleopti.Interfaces.Domain
{
	public struct PersonSkillDay
	{
		private readonly ISkill[] _skills;
		private readonly DateTimePeriod _period;
		private readonly ITeam _team;
		private readonly ISkill _maxSeatSkill;
		private readonly ISkill[] _nonBlendSkills;

		public PersonSkillDay(DateTimePeriod period, ITeam team, ISkill[] skills, ISkill maxSeatSkill, ISkill[] nonBlendSkills)
		{
			_period = period;
			_team = team;
			_skills = skills;
			_maxSeatSkill = maxSeatSkill;
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

		public ISkill MaxSeatSkill()
		{
			return _maxSeatSkill;
		}

		public ISkill[] NonBlendSkills()
		{
			return _nonBlendSkills;
		}
	}
}