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
			var payrollTimeExportDataDto = createDtoWithPersonData(person, dateOnly);

			var dayOff = scheduleDay.PersonDayOffCollection().FirstOrDefault(d => d.Period.Contains(dateOnly));
			if (dayOff != null)
			{
				payrollTimeExportDataDto.DayOffPayrollCode = dayOff.DayOff.PayrollCode;
			}

			else
			{
				var personAssignment = scheduleDay.PersonAssignment();
				payrollTimeExportDataDto.ShiftCategoryName = personAssignment != null
					                                             ? scheduleDay.PersonAssignment().ShiftCategory.Description.Name
					                                             : string.Empty;

				payrollTimeExportDataDto.StartDate = projection.Period().GetValueOrDefault().LocalStartDateTime;
				payrollTimeExportDataDto.EndDate = projection.Period().GetValueOrDefault().LocalEndDateTime;
				payrollTimeExportDataDto.ContractTime = schedulePartDto.ContractTime.TimeOfDay;
				payrollTimeExportDataDto.WorkTime = projection.WorkTime();
				payrollTimeExportDataDto.PaidTime = projection.PaidTime();
			}

			var absence = scheduleDay.PersonAbsenceCollection().FirstOrDefault(d => d.Period.Contains(dateOnly));
			if (absence != null)
				payrollTimeExportDataDto.AbsencePayrollCode = absence.Layer.Payload.PayrollCode;

			yield return payrollTimeExportDataDto;
		}

		public IList<PayrollBaseExportDto> BuildDetailedExport(IPerson person, DateOnly dateOnly, IScheduleDay scheduleDay)
		{
			var exportDtos = new List<PayrollBaseExportDto>();
			var overtimeAndShiftAllowances = createOvertimeAndShiftAllowanceDtos(person, dateOnly, scheduleDay).ToList();
			var absences = createAbsenceDtos(person, dateOnly, scheduleDay, _absenceDictionary).ToList();

			if (overtimeAndShiftAllowances.Any())
				exportDtos.AddRange(overtimeAndShiftAllowances);

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
				payrollDetailedExportDto.PartTimePercentageNumber =
					person.Period(dateOnly).PersonContract.PartTimePercentage.Percentage.Value;
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
				payrollDetailedExportDto.PartTimePercentageNumber =
					person.Period(dateOnly).PersonContract.PartTimePercentage.Percentage.Value;
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
				var projection = new ProjectionProvider().Projection(scheduleDay);

				if (layer.IsAbsence)
				{
					payrollActivitiesExportDto.PayrollCode = absenceDictionary[layer.PayloadId].PayrollCode;
					payrollActivitiesExportDto.Time = layer.ContractTime;
				}
				else
				{
					payrollActivitiesExportDto.PayrollCode = activityDictionary[layer.PayloadId].PayrollCode;
					payrollActivitiesExportDto.Time = scheduleDay.Period.ElapsedTime();
				}

				payrollActivitiesExportDto.PartTimePercentageNumber =
					person.Period(dateOnly).PersonContract.PartTimePercentage.Percentage.Value;
				payrollActivitiesExportDto.StartDate = layer.Period.LocalStartDateTime;
				payrollActivitiesExportDto.EndDate = layer.Period.LocalEndDateTime;
				payrollActivitiesExportDto.ContractTime = projection.ContractTime();
				payrollActivitiesExportDto.WorkTime = projection.WorkTime();
				payrollActivitiesExportDto.PaidTime = projection.PaidTime();

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
					Date = DateTime.Now.Date.ToShortDateString()
				};
			return payrollBaseExportDto;
		}
	}
}