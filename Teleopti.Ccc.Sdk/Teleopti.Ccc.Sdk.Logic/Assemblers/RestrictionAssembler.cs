using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class RestrictionAssembler<TDomain,TDto, TActivityRestriction> : Assembler<TDomain,TDto> 
        where TDto : IPreferenceContainerDto
        where TDomain : IPreferenceRestriction
        where TActivityRestriction : IActivityRestriction
    {
        private readonly IDomainAndDtoConstructor<TDomain, TDto> _domainAndDtoConstructor;
        private readonly IAssembler<IShiftCategory, ShiftCategoryDto> _shiftCategoryAssembler;
        private readonly IAssembler<IDayOffTemplate, DayOffInfoDto> _dayOffAssembler;
        private readonly IAssembler<IAbsence, AbsenceDto> _absenceAssembler;
        private readonly IAssembler<TActivityRestriction, ActivityRestrictionDto> _activityRestrictionAssembler;

        public RestrictionAssembler(IDomainAndDtoConstructor<TDomain,TDto> domainAndDtoConstructor, IAssembler<IShiftCategory, ShiftCategoryDto> shiftCategoryAssembler, IAssembler<IDayOffTemplate, DayOffInfoDto> dayOffAssembler, IAssembler<TActivityRestriction, ActivityRestrictionDto> activityRestrictionAssembler, IAssembler<IAbsence, AbsenceDto> absenceAssembler)
        {
            _domainAndDtoConstructor = domainAndDtoConstructor;
            _shiftCategoryAssembler = shiftCategoryAssembler;
            _dayOffAssembler = dayOffAssembler;
            _absenceAssembler = absenceAssembler; 
            _activityRestrictionAssembler = activityRestrictionAssembler;
        }

        public override TDto DomainEntityToDto(TDomain entity)
        {
            TDto dto = _domainAndDtoConstructor.CreateNewDto();
            if (entity != null)
            {
                if (entity.DayOffTemplate != null)
                    dto.DayOff = _dayOffAssembler.DomainEntityToDto(entity.DayOffTemplate);
                if (entity.ShiftCategory != null)
                    dto.ShiftCategory = _shiftCategoryAssembler.DomainEntityToDto(entity.ShiftCategory);

                if (entity.Absence != null)
                    dto.Absence = _absenceAssembler.DomainEntityToDto(entity.Absence);

                dto.StartTimeLimitation = new TimeLimitationDto
                                              {
                                                  MinTime =
                                                      entity.StartTimeLimitation.StartTime,
                                                  MaxTime =
                                                      entity.StartTimeLimitation.EndTime
                                              };
                dto.EndTimeLimitation = new TimeLimitationDto
                                            {
                                                MinTime =
                                                    entity.EndTimeLimitation.StartTime,
                                                MaxTime =
                                                    entity.EndTimeLimitation.EndTime
                                            };
                dto.WorkTimeLimitation = new TimeLimitationDto
                                             {
                                                 MinTime =
                                                     entity.WorkTimeLimitation.StartTime,
                                                 MaxTime =
                                                     entity.WorkTimeLimitation.EndTime
                                             };
                dto.LimitationEndTimeString = entity.EndTimeLimitation.EndTimeString;
                dto.LimitationStartTimeString = entity.StartTimeLimitation.StartTimeString;
                foreach (TActivityRestriction activityRestriction in entity.ActivityRestrictionCollection)
                {
                    dto.ActivityRestrictionCollection.Add(
                        _activityRestrictionAssembler.DomainEntityToDto(activityRestriction));
                }
                dto.Id = entity.Id;
            }
            return dto;
        }

        public override TDomain DtoToDomainEntity(TDto dto)
        {
            TDomain preferenceRestriction = _domainAndDtoConstructor.CreateNewDomainObject();
            preferenceRestriction.SetId(dto.Id);
            if (dto.ShiftCategory != null)
                preferenceRestriction.ShiftCategory = _shiftCategoryAssembler.DtoToDomainEntity(dto.ShiftCategory);

            if (dto.DayOff != null)
                preferenceRestriction.DayOffTemplate = _dayOffAssembler.DtoToDomainEntity(dto.DayOff);

            if (dto.Absence != null)
                preferenceRestriction.Absence = _absenceAssembler.DtoToDomainEntity(dto.Absence);

            preferenceRestriction.StartTimeLimitation = new StartTimeLimitation(dto.StartTimeLimitation.MinTime, dto.StartTimeLimitation.MaxTime);
            preferenceRestriction.EndTimeLimitation = new EndTimeLimitation(dto.EndTimeLimitation.MinTime, dto.EndTimeLimitation.MaxTime);
            preferenceRestriction.WorkTimeLimitation = new WorkTimeLimitation(dto.WorkTimeLimitation.MinTime, dto.WorkTimeLimitation.MaxTime);

            if (dto.ActivityRestrictionCollection != null)
            {
                foreach (var activityRestrictionDto in dto.ActivityRestrictionCollection)
                {
                    preferenceRestriction.AddActivityRestriction(
                        _activityRestrictionAssembler.DtoToDomainEntity(activityRestrictionDto));
                }
            }
            return preferenceRestriction;
        }
    }
}