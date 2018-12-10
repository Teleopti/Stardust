using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
	public interface ITeleoptiPayrollExportBuilder
	{
		IList<PayrollBaseExportDto> BuildTimeExport(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay);

		IList<PayrollBaseExportDto> BuildDetailedExport(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay);

		IList<PayrollBaseExportDto> BuildActivitesExport(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay);
	}

	public class TeleoptiPayrollExportBuilder : ITeleoptiPayrollExportBuilder
	{
		private readonly ISchedulePartAssembler _schedulepartAssembler;
		private readonly IDictionary<Guid, AbsenceDto> _absenceDictionary;
		private readonly IDictionary<Guid, ActivityDto> _activityDictionary;
		private readonly IList<Guid> _dayOffCodes;

		public TeleoptiPayrollExportBuilder(ISchedulePartAssembler schedulepartAssembler,
		                                    IDictionary<Guid, AbsenceDto> absenceDictionary,
		                                    IDictionary<Guid, ActivityDto> activityDictionary,
		                                    IList<Guid> dayOffCodes)
		{
			_schedulepartAssembler = schedulepartAssembler;
			_absenceDictionary = absenceDictionary;
			_activityDictionary = activityDictionary;
			_dayOffCodes = dayOffCodes;
		}

		public IList<PayrollBaseExportDto> BuildTimeExport(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay)
		{
			var exportDtos = new List<PayrollBaseExportDto>();
			var schedulePartDto = _schedulepartAssembler.DomainEntityToDto(scheduleDay);
            var timeExportDots = createTimeExportDtos(person, dateOnly, schedulePartDto).ToList();
			if (timeExportDots.Any())
				exportDtos.AddRange(timeExportDots);
			return exportDtos;
		}

		private static IEnumerable<PayrollBaseExportDto> createTimeExportDtos(IPerson person, DateOnly dateOnly, SchedulePartDto schedulePartDto)
		{
			if (isDayEmpty(schedulePartDto))
				yield break;

			var payrollTimeExportDataDto = createDtoWithPersonData(person, dateOnly);

			if (schedulePartDto.PersonDayOff != null)
			{
				payrollTimeExportDataDto.DayOffPayrollCode = schedulePartDto.PersonDayOff.PayrollCode;
				payrollTimeExportDataDto.StartDate = payrollTimeExportDataDto.Date;
				payrollTimeExportDataDto.EndDate = payrollTimeExportDataDto.Date;
			}
			else
			{
				if (schedulePartDto.IsFullDayAbsence)
				{
					var absence = schedulePartDto.PersonAbsenceCollection.FirstOrDefault();
					if (absence != null)
						payrollTimeExportDataDto.AbsencePayrollCode = absence.AbsenceLayer.Absence.PayrollCode;
				}

				var personAssignmentDto = schedulePartDto.PersonAssignmentCollection.FirstOrDefault();
				if (personAssignmentDto?.MainShift != null)
					payrollTimeExportDataDto.ShiftCategoryName =
						personAssignmentDto.MainShift.ShiftCategoryName;
                if (schedulePartDto.ProjectedLayerCollection != null && schedulePartDto.ProjectedLayerCollection.Count > 0)
                {
			        payrollTimeExportDataDto.StartDate = schedulePartDto.ProjectedLayerCollection.FirstOrDefault().Period.LocalStartDateTime;
                    payrollTimeExportDataDto.EndDate = schedulePartDto.ProjectedLayerCollection.LastOrDefault().Period.LocalEndDateTime;
			    }
			    else
			    {
                    payrollTimeExportDataDto.StartDate = schedulePartDto.LocalPeriod.LocalStartDateTime;
                    payrollTimeExportDataDto.EndDate = schedulePartDto.LocalPeriod.LocalEndDateTime;
			    }
				payrollTimeExportDataDto.ContractTime = schedulePartDto.ContractTime.TimeOfDay;
				payrollTimeExportDataDto.WorkTime = schedulePartDto.WorkTime.TimeOfDay;
				payrollTimeExportDataDto.PaidTime = schedulePartDto.PaidTime.TimeOfDay;
			}

			yield return payrollTimeExportDataDto;
		}

		private static bool isDayEmpty(SchedulePartDto part)
		{
			return !part.ProjectedLayerCollection.Any() &&
			       !part.PersonAbsenceCollection.Any() &&
			       part.PersonDayOff == null;
		}
		
		public IList<PayrollBaseExportDto> BuildDetailedExport(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay)
		{
			var exportDtos = new List<PayrollBaseExportDto>();

			var schedulePartDto = _schedulepartAssembler.DomainEntityToDto(scheduleDay);
			var overtimeAndShiftAllowances = createOvertimeAndShiftAllowanceDtos(person, dateOnly, schedulePartDto).ToList();
			if (overtimeAndShiftAllowances.Any())
				exportDtos.AddRange(overtimeAndShiftAllowances);

			var absences = createAbsenceDtos(person, dateOnly, schedulePartDto, _absenceDictionary).ToList();
			if (absences.Any())
				exportDtos.AddRange(absences);

			return exportDtos;
		}

		private static IEnumerable<PayrollBaseExportDto> createOvertimeAndShiftAllowanceDtos(IPerson person,
		                                                                              DateOnly dateOnly,
		                                                                              SchedulePartDto schedulePartDto)
		{
			foreach (var assignmentDto in schedulePartDto.PersonAssignmentCollection)
				foreach (var shiftDto in assignmentDto.OvertimeShiftCollection)
					foreach (var activityLayerDto in shiftDto.LayerCollection)
					{
						var payrollDetailedExportDto = createDtoWithPersonData(person, dateOnly);
						payrollDetailedExportDto.PayrollCode = activityLayerDto.Activity.PayrollCode;
						payrollDetailedExportDto.Time = activityLayerDto.Period.UtcEndTime - activityLayerDto.Period.UtcStartTime;
						yield return payrollDetailedExportDto;
					}
		}

		private static IEnumerable<PayrollBaseExportDto> createAbsenceDtos(IPerson person, DateOnly dateOnly,
		                                                            SchedulePartDto schedulePartDto,
		                                                            IDictionary<Guid, AbsenceDto> absenceDictionary)
		{
			var absencesInProjection = schedulePartDto.ProjectedLayerCollection
			                                          .Where(layer => layer.IsAbsence &&
			                                                          layer.ContractTime != TimeSpan.Zero);
			foreach (var layer in absencesInProjection)
			{
				var payrollDetailedExportDto = createDtoWithPersonData(person, dateOnly);
				payrollDetailedExportDto.PayrollCode = absenceDictionary[layer.PayloadId].PayrollCode;
				payrollDetailedExportDto.Time = layer.ContractTime;
				yield return payrollDetailedExportDto;
			}
		}

		public IList<PayrollBaseExportDto> BuildActivitesExport(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay)
		{
			var exportDtos = new List<PayrollBaseExportDto>();
			var schedulePartDto = _schedulepartAssembler.DomainEntityToDto(scheduleDay);
			var activityDtos =
				createActivityExportDtos(person, dateOnly, schedulePartDto, _absenceDictionary, _activityDictionary, _dayOffCodes)
					.ToList();
			if (activityDtos.Any())
				exportDtos.AddRange(activityDtos);
			return exportDtos;
		}

		private static IEnumerable<PayrollBaseExportDto> createActivityExportDtos(IPerson person, DateOnly dateOnly,
																		   SchedulePartDto schedulePartDto,
		                                                                   IDictionary<Guid, AbsenceDto>
			                                                                   absenceDictionary,
		                                                                   IDictionary<Guid, ActivityDto>
			                                                                   activityDictionary, IList<Guid> dayOffCodes)
		{
			var projectedLayersWithoutDayOffs = schedulePartDto.ProjectedLayerCollection
			                                                   .Where(layer => !dayOffCodes.Contains(layer.PayloadId));

			foreach (var layer in projectedLayersWithoutDayOffs)
			{
				var payrollActivitiesExportDto = createDtoWithPersonData(person, dateOnly);

				if (layer.IsAbsence)
				{
					payrollActivitiesExportDto.PayrollCode = absenceDictionary[layer.PayloadId].PayrollCode;
					payrollActivitiesExportDto.Time = layer.ContractTime;
				}
				else
				{
					payrollActivitiesExportDto.PayrollCode = activityDictionary[layer.PayloadId].PayrollCode;
					payrollActivitiesExportDto.Time = layer.Period.UtcEndTime - layer.Period.UtcStartTime;
				}

				payrollActivitiesExportDto.StartDate = layer.Period.UtcStartTime;
				payrollActivitiesExportDto.EndDate = layer.Period.UtcEndTime;
				payrollActivitiesExportDto.ContractTime = layer.ContractTime;
				payrollActivitiesExportDto.WorkTime = layer.WorkTime;
				payrollActivitiesExportDto.PaidTime = layer.PaidTime;

				yield return payrollActivitiesExportDto;
			}
		}

		private static PayrollBaseExportDto createDtoWithPersonData(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			var payrollBaseExportDto = new PayrollBaseExportDto
				{
					EmploymentNumber = person.EmploymentNumber,
					FirstName = person.Name.FirstName,
					LastName = person.Name.LastName,
					BusinessUnitName = personPeriod.Team.BusinessUnitExplicit.Name,
					SiteName = personPeriod.Team.Site.Description.Name,
					TeamName = personPeriod.Team.Description.Name,
					ContractName = personPeriod.PersonContract.Contract.Description.Name,
					PartTimePercentageName = personPeriod.PersonContract.PartTimePercentage.Description.Name,
					PartTimePercentageNumber =
						fiveDigitPartTimePercentage(personPeriod.PersonContract.PartTimePercentage.Percentage.Value),
					Date = dateOnly.Date
				};
			return payrollBaseExportDto;
		}

		private static int fiveDigitPartTimePercentage(double value)
		{
			return (int)(value*10000);
		}
	}
}