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
using Teleopti.Ccc.Sdk.Logic.Restrictions;
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
                people.ForEach(p => { if (isAvailableForShiftTrade(new ShiftTradeAvailableCheckItem{DateOnly = queryDate,PersonFrom = personFrom,PersonTo = p})) peopleForShiftTrade.Add(p); });
                return _personAssembler.DomainEntitiesToDtos(peopleForShiftTrade).ToList();
            }
        }

        private static bool isAvailableForShiftTrade(IShiftTradeAvailableCheckItem checkItem)
        {
            var specifications = new List<ISpecification<IShiftTradeAvailableCheckItem>>
                                     {
                                         new OpenShiftTradePeriodSpecification(),
                                         new IsWorkflowControlSetNullSpecification(),
                                         new ShiftTradeSkillSpecification()
                                     };
            return specifications.All(s => s.IsSatisfiedBy(checkItem));
        }
    }
}
