using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
{
    public class GetPeopleForShiftTradeByGroupPageGroupQueryHandler : IHandleQuery<GetPeopleForShiftTradeByGroupPageGroupQueryDto, ICollection<PersonDto>>
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetPeopleForShiftTradeByGroupPageGroupQueryHandler(
            IAssembler<IPerson, PersonDto> personAssembler,
            IGroupingReadOnlyRepository groupingReadOnlyRepository,
            IPersonRepository personRepository,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _personAssembler = personAssembler;
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<PersonDto> Handle(GetPeopleForShiftTradeByGroupPageGroupQueryDto query)
        {
            var queryDate = new DateOnly(query.QueryDate.DateTime);
            var details = _groupingReadOnlyRepository.DetailsForGroup(query.GroupPageGroupId, queryDate);

            var availableDetails = details.Where(
                p =>
                TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.ViewSchedules, queryDate, p));
            var peopleForShiftTrade = new List<IPerson>();
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var personFrom = _personRepository.Get(query.PersonId);
                var people = _personRepository.FindPeople(availableDetails.Select(d =>
                                                                                      {
                                                                                          IPerson p = new Person();
                                                                                          p.SetId(d.PersonId);
                                                                                          return p;
                                                                                      }));
                people.ForEach(p => { if (isAvailableForShiftTrade(personFrom, p, queryDate)) peopleForShiftTrade.Add(p); });
                return _personAssembler.DomainEntitiesToDtos(peopleForShiftTrade).ToList();
            }
        }

        private static bool isAvailableForShiftTrade(IPerson personFrom, IPerson personTo, DateOnly dateOnly)
        {
            if (personFrom.WorkflowControlSet == null || personTo.WorkflowControlSet == null)
                return false;

            var controlSetFrom = personFrom.WorkflowControlSet;
            var controlSetTo = personTo.WorkflowControlSet;
            var currentDate = DateOnly.Today;
            var openPeriodFrom =
                new DateOnlyPeriod(
                    currentDate.AddDays(personFrom.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum),
                    currentDate.AddDays(personFrom.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum));
            var openPeriodTo =
                new DateOnlyPeriod(
                    currentDate.AddDays(personTo.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum),
                    currentDate.AddDays(personTo.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum));

            if (!openPeriodFrom.Contains(dateOnly) || !openPeriodTo.Contains(dateOnly))
                return false;

            var mustMatchingSkills = getListOfSkills(controlSetFrom.MustMatchSkills,
                                                                              controlSetTo.MustMatchSkills);
            if (mustMatchingSkills.Count == 0)
                return true;
            var periodFrom = personFrom.Period(dateOnly);
            var periodTo = personTo.Period(dateOnly);
            if (periodFrom == null || periodTo == null)
                return false;
            ICollection<ISkill> skills = new HashSet<ISkill>();
            foreach (var personSkill in periodFrom.PersonSkillCollection.Where(personSkill => mustMatchingSkills.Contains(personSkill.Skill)))
            {
                skills.Add(personSkill.Skill);
            }

            foreach (var personSkill in periodTo.PersonSkillCollection.Where(personSkill => mustMatchingSkills.Contains(personSkill.Skill)))
            {
                if (skills.Contains(personSkill.Skill))
                {
                    skills.Remove(personSkill.Skill);
                }
                else
                {
                    skills.Add(personSkill.Skill);
                }
            }
            if (skills.Count > 0) return false;
            return true;
        }

        private static IList<ISkill> getListOfSkills(IEnumerable<ISkill> listOne, IEnumerable<ISkill> listTwo)
        {
            IList<ISkill> listOfSkills = new List<ISkill>();
            foreach (var skill in listOne.Where(skill => !listOfSkills.Contains(skill)))
            {
                listOfSkills.Add(skill);
            }
            foreach (var skill in listTwo.Where(skill => !listOfSkills.Contains(skill)))
            {
                listOfSkills.Add(skill);
            }
            return listOfSkills;
        }
    }
}
