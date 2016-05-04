﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetPeopleForShiftTradeByGroupPageGroupQueryHandler : IHandleQuery<GetPeopleForShiftTradeByGroupPageGroupQueryDto, ICollection<PersonDto>>
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IShiftTradeLightValidator _shiftTradeLightValidator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public GetPeopleForShiftTradeByGroupPageGroupQueryHandler(
            IAssembler<IPerson, PersonDto> personAssembler,
            IGroupingReadOnlyRepository groupingReadOnlyRepository,
            IPersonRepository personRepository,
            ICurrentUnitOfWorkFactory unitOfWorkFactory,
			IShiftTradeLightValidator shiftTradeLightValidator)
        {
            _personAssembler = personAssembler;
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
            _personRepository = personRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
			_shiftTradeLightValidator = shiftTradeLightValidator;
        }

		public ICollection<PersonDto> Handle(GetPeopleForShiftTradeByGroupPageGroupQueryDto query)
		{
			var queryDate = query.QueryDate.ToDateOnly();
            var details = _groupingReadOnlyRepository.DetailsForGroup(query.GroupPageGroupId, queryDate);

            var availableDetails = details.Where(
                p =>
                PrincipalAuthorization.Current().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.ViewSchedules, queryDate, p));
            var peopleForShiftTrade = new List<IPerson>();
            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var personFrom = _personRepository.Get(query.PersonId);
                var people = _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));
                people.ForEach(p => { if (isAvailableForShiftTrade(new ShiftTradeAvailableCheckItem(queryDate, personFrom, p))) peopleForShiftTrade.Add(p); });
                return _personAssembler.DomainEntitiesToDtos(peopleForShiftTrade).ToList();
            }
        }

        private bool isAvailableForShiftTrade(ShiftTradeAvailableCheckItem checkItem)
        {
        	return _shiftTradeLightValidator.Validate(checkItem).Value;
        }
    }
}
