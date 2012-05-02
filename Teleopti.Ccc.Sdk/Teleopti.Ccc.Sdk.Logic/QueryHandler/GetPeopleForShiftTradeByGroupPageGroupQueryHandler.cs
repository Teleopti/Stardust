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

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetPeopleForShiftTradeByGroupPageGroupQueryHandler : IHandleQuery<GetPeopleForShiftTradeByGroupPageGroupQueryDto, ICollection<PersonDto>>
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IEnumerable<ISpecification<IShiftTradeAvailableCheckItem>> _availableForShiftTradeSpecifications;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public GetPeopleForShiftTradeByGroupPageGroupQueryHandler(
            IAssembler<IPerson, PersonDto> personAssembler,
            IGroupingReadOnlyRepository groupingReadOnlyRepository,
            IPersonRepository personRepository,
            IUnitOfWorkFactory unitOfWorkFactory,
			IEnumerable<ISpecification<IShiftTradeAvailableCheckItem>> availableForShiftTradeSpecifications)
        {
            _personAssembler = personAssembler;
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        	_availableForShiftTradeSpecifications = availableForShiftTradeSpecifications;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
                var people = _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));
                people.ForEach(p => { if (isAvailableForShiftTrade(new ShiftTradeAvailableCheckItem{DateOnly = queryDate,PersonFrom = personFrom,PersonTo = p})) peopleForShiftTrade.Add(p); });
                return _personAssembler.DomainEntitiesToDtos(peopleForShiftTrade).ToList();
            }
        }

        private bool isAvailableForShiftTrade(IShiftTradeAvailableCheckItem checkItem)
        {
            return _availableForShiftTradeSpecifications.All(s => s.IsSatisfiedBy(checkItem));
        }
    }
}
