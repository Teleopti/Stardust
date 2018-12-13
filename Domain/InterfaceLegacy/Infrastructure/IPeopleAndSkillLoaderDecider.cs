using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{

    /// <summary>
    /// Helper to tell what skills and people really necessary to load
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-12-08
    /// </remarks>
    public interface IPeopleAndSkillLoaderDecider
    {
        /// <summary>
        /// Executes and verifies the specified period.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <param name="period">The period.</param>
        /// <param name="people">The people.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-12-08
        /// </remarks>
        ILoaderDeciderResult Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> people);
    }

	public interface ILoaderDeciderResult
	{
		Guid[] PeopleGuidDependencies { get; }
		Guid[] SkillGuidDependencies { get; }
		Guid[] SiteGuidDependencies { get; }

		/// <summary>
		/// Filters the people. 
		/// Removing entries with Ids not in PeopleGuidDependencies
		/// </summary>
		/// <param name="people">The people.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-01-21
		/// </remarks>
		int FilterPeople(ICollection<IPerson> people);

		int FilterSkills(IEnumerable<ISkill> skills, Action<ISkill> removeSkill, Action<ISkill> addSkill);
	}
}
