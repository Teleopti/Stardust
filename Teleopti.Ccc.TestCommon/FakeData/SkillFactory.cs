using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// SkillFactory
    /// </summary>
    public static class SkillFactory
    {
        /// <summary>
        /// Creates a Skill
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="skillType">Type of the skill.</param>
        /// <param name="defaultResolution">The default solution.</param>
        /// <returns></returns>
        public static ISkill CreateSkill(string name, ISkillType skillType, int defaultResolution)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            string description = "Description of the Skill";
            Color displayColor = Color.FromArgb(123);

			Skill skill =
				new Skill(name, description, displayColor, defaultResolution, skillType)
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = ActivityFactory.CreateActivity("activity")
				};

			return skill;
        }


        public static ISkill CreateSkill(string skillName, ISkillType skillType, int defaultResolution, TimeZoneInfo TimeZoneInfo, TimeSpan timeSpan)
        {
            ISkill newSkill = CreateSkill(skillName, skillType, defaultResolution);
            newSkill.TimeZone = TimeZoneInfo;
            newSkill.MidnightBreakOffset = timeSpan;
            return newSkill;
        }

        /// <summary>
        /// Creates the multisite skill.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="skillType">Type of the skill.</param>
        /// <param name="defaultSolution">The default solution.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public static IMultisiteSkill CreateMultisiteSkill(string name, ISkillType skillType, int defaultSolution)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            string description = "Description of the Skill";
            Color displayColor = Color.FromArgb(123);

			MultisiteSkill skill =
				new MultisiteSkill(name, description, displayColor, defaultSolution, skillType)
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = ActivityFactory.CreateActivity("activity")
				};

			return skill;
        }

		public static ISkill CreateSkillWithId(string name, int defaultSolution)
		{
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			return CreateSkill(name, skillType, defaultSolution).WithId();
		}

		/// <summary>
		/// Creates the skill.
		/// </summary>
		/// <param name="skillName">Name of the skill.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: sumeda herath
		/// Created date: 2008-01-15
		/// </remarks>
		public static ISkill CreateSkill(string skillName)
        {
            SkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
            ISkill skill = CreateSkill(skillName, skillType, 15);

            return skill;
        }

	    public static ISkill CreateSkill(string skillName, TimeZoneInfo TimeZoneInfo)
	    {
			ISkill newSkill = CreateSkill(skillName);
			newSkill.TimeZone = TimeZoneInfo;			
			return newSkill;
	    }


		public static ISkill CreateSkillWithId(string skillName)
		{
			var skill = CreateSkill(skillName).WithId();
			return skill;
		}

        /// <summary>
        /// Creates the multisite skill.
        /// </summary>
        /// <param name="skillName">Name of the skill.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public static IMultisiteSkill CreateMultisiteSkill(string skillName)
        {
            SkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
            IMultisiteSkill skill = CreateMultisiteSkill(skillName, skillType, 15);

            return skill;
        }

        /// <summary>
        /// Creates the child skill.
        /// </summary>
        /// <param name="skillName">Name of the skill.</param>
        /// <param name="parentSkill">The parent skill.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        public static IChildSkill CreateChildSkill(string skillName,IMultisiteSkill parentSkill)
        {
            return new ChildSkill(skillName,skillName,parentSkill.DisplayColor, parentSkill);
        }

		public static ISkill CreateSiteSkill(string skillName)
		{
			SkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
			skillType.ForecastSource = ForecastSource.MaxSeatSkill;
			ISkill skill = CreateSkill(skillName, skillType, 15);
		    skill.TimeZone = TimeZoneInfo.Utc;

			return skill;
		}

        public static ISkill CreateNonBlendSkill(string skillName)
        {
            SkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
            skillType.ForecastSource = ForecastSource.NonBlendSkill;
            ISkill skill = CreateSkill(skillName, skillType, 15);

            return skill;
        }

        /// <summary>
        /// Creates the skill with workload and sources.
        /// </summary>
        /// <returns></returns>
        public static ISkill CreateSkillWithWorkloadAndSources()
        {
            //1. CreateProjection a skill, with skilltype
            //2. CreateProjection a workload and add it to the skill
            //3. Add some ctiQueues to the workload
            //4. Load historical data in Stat structure
            SkillType skillType = SkillTypeFactory.CreateSkillTypePhone();
            ISkill skill = CreateSkill("TestSkill", skillType,15);
            IWorkload workload = WorkloadFactory.CreateWorkload(skill);
			workload.SetId(Guid.NewGuid());

            QueueSource qsInrikes = QueueSourceFactory.CreateQueueSourceInrikes();
            QueueSource qsHelpDesk = QueueSourceFactory.CreateQueueSourceHelpdesk();

            workload.AddQueueSource(qsInrikes);
            workload.AddQueueSource(qsHelpDesk);

            return skill;
        }
    }
}