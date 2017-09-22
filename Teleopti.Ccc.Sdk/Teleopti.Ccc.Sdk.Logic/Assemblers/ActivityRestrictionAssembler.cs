using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ActivityRestrictionAssembler<TDo> : Assembler<TDo, ActivityRestrictionDto> where TDo : IActivityRestriction
    {
        private readonly IActivityRestrictionDomainObjectCreator<TDo> _domainObjectCreator;
        private readonly IAssembler<IActivity, ActivityDto> _activityAssembler;

        public ActivityRestrictionAssembler(IActivityRestrictionDomainObjectCreator<TDo> domainObjectCreator, IAssembler<IActivity,ActivityDto> activityAssembler)
        {
            _domainObjectCreator = domainObjectCreator;
            _activityAssembler = activityAssembler;
        }

        public override ActivityRestrictionDto DomainEntityToDto(TDo entity)
        {
            var activityRestrictionDto = new ActivityRestrictionDto();
            activityRestrictionDto.Id = entity.Id;
            activityRestrictionDto.Activity = _activityAssembler.DomainEntityToDto(entity.Activity);
            activityRestrictionDto.EndTimeLimitation = new TimeLimitationDto
                                                           {
                                                               MinTime = entity.EndTimeLimitation.StartTime,
                                                               MaxTime = entity.EndTimeLimitation.EndTime
                                                           };
            activityRestrictionDto.StartTimeLimitation = new TimeLimitationDto
                                                             {
                                                                 MinTime =
                                                                     entity.StartTimeLimitation.StartTime,
                                                                 MaxTime =
                                                                     entity.StartTimeLimitation.EndTime
                                                             };
            activityRestrictionDto.WorkTimeLimitation = new TimeLimitationDto
                                                            {
                                                                MinTime =
                                                                    entity.WorkTimeLimitation.StartTime,
                                                                MaxTime = entity.WorkTimeLimitation.EndTime
                                                            };

            return activityRestrictionDto;
        }

        public override TDo DtoToDomainEntity(ActivityRestrictionDto dto)
        {
            var activity = _activityAssembler.DtoToDomainEntity(dto.Activity);
            TDo activtyRestriction = _domainObjectCreator.CreateNewDomainObject(activity);
            activtyRestriction.SetId(dto.Id);
            activtyRestriction.StartTimeLimitation = new StartTimeLimitation(dto.StartTimeLimitation.MinTime, dto.StartTimeLimitation.MaxTime);
            activtyRestriction.EndTimeLimitation = new EndTimeLimitation(dto.EndTimeLimitation.MinTime, dto.EndTimeLimitation.MaxTime);
            activtyRestriction.WorkTimeLimitation = new WorkTimeLimitation(dto.WorkTimeLimitation.MinTime, dto.WorkTimeLimitation.MaxTime);

            return activtyRestriction;
        }

    }

    public interface IActivityRestrictionDomainObjectCreator<TDo> where TDo : IActivityRestriction
    {
        TDo CreateNewDomainObject(IActivity activity);
    }
}