using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class SkillDataAssembler:Assembler<ISkillStaffPeriod, SkillDataDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

        public SkillDataAssembler(IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

        public override SkillDataDto DomainEntityToDto(ISkillStaffPeriod entity)
        {
            SkillDataDto skillDataDto = new SkillDataDto
                                        {
                                            ForecastedAgents = entity.FStaff,
                                            ScheduledAgents = entity.CalculatedResource,
                                            ScheduledHeads = entity.CalculatedLoggedOn,
                                            IntervalStandardDeviation = entity.IntraIntervalDeviation,
                                            EstimatedServiceLevel = entity.EstimatedServiceLevel.Value,
                                            Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period)
                                        };
            return skillDataDto;
        }

        public override ISkillStaffPeriod DtoToDomainEntity(SkillDataDto dto)
        {
            throw new NotSupportedException("Conversion this way of skill data is not supported because values are calculated only.");
        }
    }
}