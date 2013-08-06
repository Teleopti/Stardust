using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
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
			var timeExportDots = createTimeExportDtos(person, dateOnly, scheduleDay).ToList();
			if (timeExportDots.Any())
				exportDtos.AddRange(timeExportDots);
			return exportDtos;
		}

		private IEnumerable<PayrollBaseExportDto> createTimeExportDtos(IPerson person, DateOnly dateOnly,
		                                                               IScheduleDay scheduleDay)
		{
			var schedulePartDto = _schedulepartAssembler.DomainEntityToDto(scheduleDay);
			var projection = new ProjectionProvider().Projection(scheduleDay);
			
			if (isDayEmpty(scheduleDay, projection))
				yield break;

			var payrollTimeExportDataDto = createDtoWithPersonData(person, dateOnly);

			var dayOff = scheduleDay.PersonDayOffCollection().FirstOrDefault();
			if (dayOff != null)
			{
				payrollTimeExportDataDto.DayOffPayrollCode = dayOff.DayOff.PayrollCode;
			}
			
			else
			{
				if (schedulePartDto.IsFullDayAbsence)
				{
					var absence = schedulePartDto.PersonAbsenceCollection.FirstOrDefault();
					if (absence != null)
						payrollTimeExportDataDto.AbsencePayrollCode = absence.AbsenceLayer.Absence.PayrollCode;
				}

				var personAssignment = scheduleDay.PersonAssignment();
				if (havePersonAssignmentAndShiftCategory(personAssignment))
					payrollTimeExportDataDto.ShiftCategoryName = personAssignment.ShiftCategory.Description.Name;
				
				payrollTimeExportDataDto.StartDate = projection.Period().GetValueOrDefault().LocalStartDateTime;
				payrollTimeExportDataDto.EndDate = projection.Period().GetValueOrDefault().LocalEndDateTime;
				payrollTimeExportDataDto.ContractTime = schedulePartDto.ContractTime.TimeOfDay;
				payrollTimeExportDataDto.WorkTime = projection.WorkTime();
				payrollTimeExportDataDto.PaidTime = projection.PaidTime();
			}

			yield return payrollTimeExportDataDto;
		}

		private static bool havePersonAssignmentAndShiftCategory(IPersonAssignment personAssignment)
		{
			return personAssignment != null && personAssignment.ShiftCategory != null;
		}

		private static bool isDayEmpty(IScheduleDay scheduleDay, IVisualLayerCollection projection)
		{
			return !projection.HasLayers &&
			       !scheduleDay.PersonDayOffCollection().Any() &&
			       !scheduleDay.PersonAbsenceCollection(false).Any();
		}
		
		public IList<PayrollBaseExportDto> BuildDetailedExport(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay)
		{
			var exportDtos = new List<PayrollBaseExportDto>();

			var overtimeAndShiftAllowances = createOvertimeAndShiftAllowanceDtos(person, dateOnly, scheduleDay).ToList();
			if (overtimeAndShiftAllowances.Any())
				exportDtos.AddRange(overtimeAndShiftAllowances);

			var absences = createAbsenceDtos(person, dateOnly, scheduleDay, _absenceDictionary).ToList();
			if (absences.Any())
				exportDtos.AddRange(absences);

			return exportDtos;
		}

		private static IEnumerable<PayrollBaseExportDto> createOvertimeAndShiftAllowanceDtos(IPerson person,
		                                                                                     DateOnly dateOnly,
		                                                                                     IScheduleDay scheduleDay)
		{
			var multiplicatorProjectionService = new MultiplicatorProjectionService(scheduleDay, dateOnly);
			var projection = multiplicatorProjectionService.CreateProjection();

			foreach (var layer in projection)
			{
				var payrollDetailedExportDto = createDtoWithPersonData(person, dateOnly);
				payrollDetailedExportDto.PayrollCode = layer.Payload.ExportCode;
				payrollDetailedExportDto.Time = layer.Period.ElapsedTime();
				yield return payrollDetailedExportDto;
			}
		}

		private IEnumerable<PayrollBaseExportDto> createAbsenceDtos(IPerson person, DateOnly dateOnly,
		                                                            IScheduleDay scheduleDay,
		                                                            IDictionary<Guid, AbsenceDto> absenceDictionary)
		{
			var schedulePartDto = _schedulepartAssembler.DomainEntityToDto(scheduleDay);
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
			var activityDtos =
				createActivityExportDtos(person, dateOnly, scheduleDay, _absenceDictionary, _activityDictionary, _dayOffCodes)
					.ToList();
			if (activityDtos.Any())
				exportDtos.AddRange(activityDtos);
			return exportDtos;
		}

		private IEnumerable<PayrollBaseExportDto> createActivityExportDtos(IPerson person, DateOnly dateOnly,
		                                                                   IScheduleDay scheduleDay,
		                                                                   IDictionary<Guid, AbsenceDto>
			                                                                   absenceDictionary,
		                                                                   IDictionary<Guid, ActivityDto>
			                                                                   activityDictionary, IList<Guid> dayOffCodes)
		{
			var schedulePartDto = _schedulepartAssembler.DomainEntityToDto(scheduleDay);
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
						fiveDigitPartTimePercentage(person.Period(dateOnly).PersonContract.PartTimePercentage.Percentage.Value),
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