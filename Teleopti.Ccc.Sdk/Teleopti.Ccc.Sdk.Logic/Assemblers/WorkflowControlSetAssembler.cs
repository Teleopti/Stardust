using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class WorkflowControlSetAssembler : Assembler<IWorkflowControlSet,WorkflowControlSetDto>
    {
        private readonly IAssembler<IShiftCategory, ShiftCategoryDto> _shiftCategoryAssembler;
        private readonly IAssembler<IDayOffTemplate, DayOffInfoDto> _dayOffAssembler;
        private readonly IAssembler<IActivity, ActivityDto> _activityAssembler;
        private readonly IAssembler<IAbsence, AbsenceDto> _absenceAssembler;

        public WorkflowControlSetAssembler(IAssembler<IShiftCategory,ShiftCategoryDto> shiftCategoryAssembler, IAssembler<IDayOffTemplate, DayOffInfoDto> dayOffAssembler, IAssembler<IActivity,ActivityDto> activityAssembler, IAssembler<IAbsence, AbsenceDto> absenceAssembler)
        {
            _shiftCategoryAssembler = shiftCategoryAssembler;
            _dayOffAssembler = dayOffAssembler;
            _activityAssembler = activityAssembler;
            _absenceAssembler = absenceAssembler;
        }

        public override WorkflowControlSetDto DomainEntityToDto(IWorkflowControlSet entity)
        {
            var workflowControlSetDto = new WorkflowControlSetDto();
            workflowControlSetDto.Id = entity.Id;
            if (entity.AllowedPreferenceActivity != null)
            {
                workflowControlSetDto.AllowedPreferenceActivity = _activityAssembler.DomainEntityToDto(entity.AllowedPreferenceActivity);
            }
			workflowControlSetDto.PreferencePeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = entity.PreferencePeriod.StartDate.Date },
				EndDate = new DateOnlyDto { DateTime = entity.PreferencePeriod.EndDate.Date }
			};
			workflowControlSetDto.PreferenceInputPeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = entity.PreferenceInputPeriod.StartDate.Date },
				EndDate = new DateOnlyDto { DateTime = entity.PreferenceInputPeriod.EndDate.Date }
			};
			workflowControlSetDto.StudentAvailabilityPeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = entity.StudentAvailabilityPeriod.StartDate.Date },
				EndDate = new DateOnlyDto { DateTime = entity.StudentAvailabilityPeriod.EndDate.Date }
			};
			workflowControlSetDto.StudentAvailabilityInputPeriod = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = entity.StudentAvailabilityInputPeriod.StartDate.Date },
				EndDate = new DateOnlyDto { DateTime = entity.StudentAvailabilityInputPeriod.EndDate.Date }
			};
        	workflowControlSetDto.SchedulesPublishedToDate = entity.SchedulePublishedToDate;

            foreach (IShiftCategory shiftCategory in entity.AllowedPreferenceShiftCategories)
            {
                if(!((IDeleteTag)shiftCategory).IsDeleted)
                    workflowControlSetDto.AllowedPreferenceShiftCategories.Add(_shiftCategoryAssembler.DomainEntityToDto(shiftCategory));
            }

            foreach (IDayOffTemplate dayOff in entity.AllowedPreferenceDayOffs)
            {
                if(!((IDeleteTag)dayOff).IsDeleted)
                {
                    DayOffInfoDto dayOffDto = _dayOffAssembler.DomainEntityToDto(dayOff);
                    workflowControlSetDto.AllowedPreferenceDayOffs.Add(dayOffDto);
                }
                
            }

            foreach (IAbsence absence in entity.AllowedPreferenceAbsences)
            {
                if(!((IDeleteTag)absence).IsDeleted)
                {
                    workflowControlSetDto.AllowedPreferenceAbsences.Add(_absenceAssembler.DomainEntityToDto(absence));
                }
            }

            return workflowControlSetDto;
        }

        public override IWorkflowControlSet DtoToDomainEntity(WorkflowControlSetDto dto)
        {
            throw new NotImplementedException();
        }
    }
}