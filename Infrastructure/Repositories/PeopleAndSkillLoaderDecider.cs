using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PeopleAndSkillLoaderDecider : IPeopleAndSkillLoaderDecider
    {
        private readonly IPersonRepository _personRepository;
        private readonly IPairMatrixService<Guid> _matrixService;
		
	    public PeopleAndSkillLoaderDecider(IPersonRepository personRepository, IPairMatrixService<Guid> pairMatrixService)
        {
            _personRepository = personRepository;
            _matrixService = pairMatrixService;
        }

		public ILoaderDeciderResult Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> people)
        {
            var matrix = _personRepository.PeopleSkillMatrix(scenario, period);
			var peopleGuids = people.Select(p => p.Id.GetValueOrDefault()).ToArray();

            var result = _matrixService.CreateDependencies(matrix, peopleGuids);
            var peopleGuidDependencies = result.FirstDependencies.ToArray();
			var skillGuidDependencies = result.SecondDependencies.ToArray();
			var siteGuidDependencies = _personRepository.PeopleSiteMatrix(period).ToArray();
			return new LoaderDeciderResult(period, peopleGuidDependencies, skillGuidDependencies, siteGuidDependencies);
        }
    }
}
