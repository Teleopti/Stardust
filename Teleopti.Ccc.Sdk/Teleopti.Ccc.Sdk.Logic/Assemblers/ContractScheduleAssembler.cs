using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ContractScheduleAssembler : Assembler<IContractSchedule,ContractScheduleDto>
    {
        private readonly IContractScheduleRepository _contractScheduleRepository;

        public ContractScheduleAssembler(IContractScheduleRepository contractScheduleRepository)
        {
            _contractScheduleRepository = contractScheduleRepository;
        }

        public override ContractScheduleDto DomainEntityToDto(IContractSchedule entity)
        {
	        var daysOfWeek = Enum.GetValues(typeof (DayOfWeek)).OfType<DayOfWeek>();
            return new ContractScheduleDto
                       {
                           Description = entity.Description.Name,
                           Id = entity.Id,
                           IsDeleted = ((IDeleteTag) entity).IsDeleted,
						   Weeks = entity.ContractScheduleWeeks.Select(s => new ContractScheduleWeekDto {WeekNumber = s.WeekOrder+1,WorkingDays = daysOfWeek.Where(s.IsWorkday).ToArray()}).ToArray()
                       };
        }

        public override IContractSchedule DtoToDomainEntity(ContractScheduleDto dto)
        {
            return _contractScheduleRepository.Get(dto.Id.GetValueOrDefault(Guid.Empty));
        }
    }
}