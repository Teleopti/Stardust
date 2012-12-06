using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IScheduleDayReadModelCreator
	{
		ScheduleDayReadModel TurnScheduleToModel(IScheduleDay scheduleDay);
	}

	public class ScheduleDayReadModelCreator : IScheduleDayReadModelCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ScheduleDayReadModel TurnScheduleToModel(IScheduleDay scheduleDay)
		{
			var ret = new ScheduleDayReadModel();
			var person = scheduleDay.Person;
			var tz = person.PermissionInformation.DefaultTimeZone();
			var proj = scheduleDay.ProjectionService().CreateProjection();

			ret.ContractTimeTicks = proj.ContractTime().Ticks;
			ret.WorkTimeTicks = proj.WorkTime().Ticks;
			ret.PersonId = person.Id.GetValueOrDefault();
			ret.Date = scheduleDay.DateOnlyAsPeriod.DateOnly;

			var period = proj.Period();
			if (period != null)
			{
				ret.StartDateTime = period.Value.StartDateTimeLocal(tz);
				ret.EndDateTime = period.Value.EndDateTimeLocal(tz);
			}

			var significantPart = scheduleDay.SignificantPart();
			if (significantPart.Equals(SchedulePartView.MainShift))
			{
				var cat = scheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory;
				ret.Label = cat.Description.ShortName;
				ret.ColorCode = cat.DisplayColor.ToArgb();
				ret.Workday = true;
			}
			if (significantPart.Equals(SchedulePartView.Overtime))
			{
				ret.Workday = true;
			}
			if (significantPart.Equals(SchedulePartView.FullDayAbsence))
				ret.Label = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.Description.ShortName;
			if (significantPart.Equals(SchedulePartView.DayOff))
				ret.Label = scheduleDay.PersonDayOffCollection()[0].DayOff.Description.ShortName;

			return ret;
		}
	}

	public interface IPersonScheduleDayReadModelCreator
	{
		PersonScheduleDayReadModel TurnScheduleToModel(IScheduleDay scheduleDay);
	}

	public class PersonScheduleDayReadModelCreator : IPersonScheduleDayReadModelCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public PersonScheduleDayReadModel TurnScheduleToModel(IScheduleDay scheduleDay)
		{
			var ret = new PersonScheduleDayReadModel();
			var person = scheduleDay.Person;
			var tz = person.PermissionInformation.DefaultTimeZone();
			var proj = scheduleDay.ProjectionService().CreateProjection();
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personPeriod = person.Period(date);

			ret.PersonId = person.Id.GetValueOrDefault();
			ret.TeamId = personPeriod.Team.Id.GetValueOrDefault();
			ret.SiteId = personPeriod.Team.Site.Id.GetValueOrDefault();
			ret.BusinessUnitId = personPeriod.Team.BusinessUnitExplicit.Id.GetValueOrDefault();
			ret.Date = date;

			var period = proj.Period();
			if (period != null)
			{
				ret.ShiftStart = period.Value.StartDateTimeLocal(tz);
				ret.ShiftEnd = period.Value.EndDateTimeLocal(tz);
			}

			var shift = new Shift
			            	{
			            		Date = date,
			            		FirstName = person.Name.FirstName,
			            		LastName = person.Name.LastName,
			            		EmploymentNumber = person.EmploymentNumber,
			            		Id = person.Id.GetValueOrDefault().ToString(),
			            		ContractTimeMinutes = (int) proj.ContractTime().TotalMinutes,
			            		WorkTimeMinutes = (int) proj.WorkTime().TotalMinutes,
								Projection = new List<SimpleLayer>()
			            	};
			var significantPart = scheduleDay.SignificantPartForDisplay();
			if (significantPart.Equals(SchedulePartView.DayOff))
			{
				var dayOff = scheduleDay.PersonDayOffCollection()[0];
				shift.Projection.Add(new SimpleLayer
				                     	{
				                     		Color = ColorTranslator.ToHtml(dayOff.DayOff.DisplayColor),
				                     		Title = dayOff.DayOff.Description.Name,
				                     		Start = dayOff.Period.StartDateTime,
				                     		End = dayOff.Period.EndDateTime,
				                     		Minutes = (int) dayOff.Period.ElapsedTime().TotalMinutes
				                     	});
			}
			else
			{
				foreach (var layer in proj)
				{
					shift.Projection.Add(new SimpleLayer
					{
						Color = ColorTranslator.ToHtml(layer.DisplayColor()),
						Title = layer.DisplayDescription().Name,
						Start = layer.Period.StartDateTime,
						End = layer.Period.EndDateTime,
						Minutes = (int)layer.Period.ElapsedTime().TotalMinutes
					});
				}
			}

			ret.Shift = Newtonsoft.Json.JsonConvert.SerializeObject(shift);

			return ret;
		}
	}

	public class Shift
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public string Id { get; set; }
		public DateTime Date { get; set; }
		public int WorkTimeMinutes { get; set; }
		public int ContractTimeMinutes { get; set; }

		public IList<SimpleLayer> Projection { get; set; }
	}

	public class SimpleLayer
	{
		public string Color { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int Minutes { get; set; }
		public string Title { get; set; }
	}
}