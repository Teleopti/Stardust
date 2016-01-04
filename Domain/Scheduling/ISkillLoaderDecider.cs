using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ISkillLoaderDecider
	{
		IEnumerable<Guid> PeopleGuidDependencies { get; }
		IEnumerable<Guid> SkillGuidDependencies { get; }
		IEnumerable<Guid> SiteGuidDependencies { get; }
		IPairMatrixService<Guid> MatrixService { get; }
		double PercentageOfPeopleFiltered { get; }
		void Execute(IScenario scenario, DateTimePeriod period, ISkill skill);
		int FilterPeople(ICollection<IPerson> people);
		int FilterSkills(ISkill[] skills, Action<ISkill> removeSkill, Action<ISkill> addSkill);
	}
}