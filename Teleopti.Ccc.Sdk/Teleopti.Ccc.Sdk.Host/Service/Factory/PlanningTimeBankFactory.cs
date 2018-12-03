using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.Restrictions;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public class PlanningTimeBankFactory
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;

        public PlanningTimeBankFactory(IAssembler<IPerson,PersonDto> personAssembler)
        {
            _personAssembler = personAssembler;
        }

        public PlanningTimeBankDto GetPlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto)
        {
	        using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
	        {
		        var person = _personAssembler.DtoToDomainEntity(personDto);
		        var timeBankExtractor = new PlanningTimeBankExtractor(person);
		        return timeBankExtractor.GetPlanningTimeBank(new DateOnly(dateOnlyDto.DateTime));    
	        }
        }

	    public void SavePlanningTimeBank(PersonDto personDto, DateOnlyDto dateOnlyDto, int balanceOutMinute)
        {
            var checkDto = GetPlanningTimeBank(personDto, dateOnlyDto);
            if(!checkDto.IsEditable)
                return;
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var person = _personAssembler.DtoToDomainEntity(personDto);
                person.SchedulePeriod(new DateOnly(dateOnlyDto.DateTime)).BalanceOut = TimeSpan.FromMinutes(balanceOutMinute);
                uow.PersistAll();
            }
        }
    }
}