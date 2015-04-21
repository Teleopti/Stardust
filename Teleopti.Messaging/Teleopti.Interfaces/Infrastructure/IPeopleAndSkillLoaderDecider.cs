﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
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

		/// <summary>
		/// Filters the skills.
		/// Removing entries with Ids not in SkillGuidDependencies
		/// </summary>
		/// <param name="skills">The skills.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-01-21
		/// </remarks>
		int FilterSkills(ICollection<ISkill> skills);

		///<summary>
		/// Shows how many percent of all people injected into method FilterPeople that is filtered out. If FilterPeople is not executed then zero wil be returned.
		///</summary>
		double PercentageOfPeopleFiltered { get; }
	}
}
