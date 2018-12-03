using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Factory class for Person Skill
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-06-15
    /// </remarks>
    public static class PersonSkillFactory
    {
        /// <summary>
        /// Creates the person skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="percent">The percent.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-06-15
        /// </remarks>
        public static IPersonSkill CreatePersonSkill(string skill, double percent)
        {
            ISkill skill1 = SkillFactory.CreateSkill(skill);
            return CreatePersonSkill(skill1, percent);
        }

        /// <summary>
        /// Creates the person skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <param name="percent">The percent.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-01-16
        /// </remarks>
		public static IPersonSkill CreatePersonSkill(ISkill skill, double percent)
        {
        	var percent1 = new Percent(percent);
			return new PersonSkill(skill, percent1);
        }

    	/// <summary>
        /// Creates the person skill with same percent.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-06-15
        /// </remarks>
        public static IPersonSkill CreatePersonSkillWithSamePercent(ISkill skill)
        {
            Percent percent1 = new Percent(1);

			return new PersonSkill(skill, percent1);
        }
    }
}
