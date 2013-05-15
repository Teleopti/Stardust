using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
	[TestFixture]
	public class HourlyAvailabilityTransformerTest
	{
		private MockRepository _mocks;
		private HourlyAvailabilityTransformer _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new HourlyAvailabilityTransformer();
		}

		[Test]
		public void ShouldTransformAvailability()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var scenario = new Scenario("sen");
			scenario.SetId(Guid.NewGuid());
			var table = new DataTable();
			HourlyAvailabilityInfrastructure.AddColumnsToDataTable(table);
			var schedDay = _mocks.DynamicMock<IScheduleDay>();
			var studRestriction = new StudentAvailabilityRestriction();
			var studDay = new StudentAvailabilityDay(person, new DateOnly(2013, 5, 15), new List<IStudentAvailabilityRestriction> { studRestriction });
			
			var projService = _mocks.DynamicMock<IProjectionService>();
			var layers = _mocks.DynamicMock<IVisualLayerCollection>();

			Expect.Call(schedDay.RestrictionCollection()).Return(new List<IRestrictionBase> { studRestriction });
			Expect.Call(schedDay.Scenario).Return(scenario);
			Expect.Call(schedDay.Person).Return(person);
			Expect.Call(schedDay.IsScheduled()).Return(true);
			Expect.Call(schedDay.ProjectionService()).Return(projService);
			Expect.Call(projService.CreateProjection()).Return(layers);
			Expect.Call(layers.HasLayers).Return(true);
			Expect.Call(layers.WorkTime()).Return(TimeSpan.FromHours(5));
			_mocks.ReplayAll();
			_target.Transform(new List<IScheduleDay> { schedDay }, table);
			Assert.That(table.Rows.Count,Is.EqualTo(1));
			_mocks.VerifyAll();
		}

	}

	public class HourlyAvailabilityTransformer : IEtlTransformer<IScheduleDay>
	{
		public void Transform(IEnumerable<IScheduleDay> rootList, DataTable table)
		{
			foreach (var schedulePart in rootList)
			{
				var restrictionBases = schedulePart.RestrictionCollection();
				foreach (var restrictionBase in restrictionBases)
				{
					var availRestriction = restrictionBase as IStudentAvailabilityRestriction;
					
					var newDataRow = table.NewRow();
					newDataRow = fillDataRow(newDataRow, availRestriction, schedulePart);
					table.Rows.Add(newDataRow);
					
				}
			}
		}

		private DataRow fillDataRow(DataRow dataRow, IStudentAvailabilityRestriction availRestriction, IScheduleDay schedulePart)
		{
			var availDay = (IStudentAvailabilityDay)availRestriction.Parent;
			dataRow["restriction_date"] = availDay.RestrictionDate.Date;
			dataRow["person_code"] = schedulePart.Person.Id;
			dataRow["scenario_code"] = schedulePart.Scenario.Id;
			dataRow["business_unit_code"] = schedulePart.Scenario.BusinessUnit.Id;
			dataRow["available_time_m"] = getMaxAvailable(availRestriction);
			var workTime = scheduledWorkTime(schedulePart);
			dataRow["scheduled_time_m"] = workTime;
			dataRow["scheduled"] = workTime > 0;
			dataRow["datasource_id"] = 1;

			return dataRow;
		}

		private int scheduledWorkTime(IScheduleDay scheduleDay)
		{
			var minutes = 0;
			if (scheduleDay.IsScheduled())
			{
				var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();

				if (visualLayerCollection.HasLayers)
				{
					minutes = (int)visualLayerCollection.WorkTime().TotalMinutes;
				}
			}
			return minutes;
		}

		private int getMaxAvailable(IStudentAvailabilityRestriction availRestriction)
		{
			var start = TimeSpan.FromMinutes(0);
			var end = TimeSpan.FromHours(24);
			if (availRestriction.StartTimeLimitation.StartTime.HasValue)
				start = availRestriction.StartTimeLimitation.StartTime.GetValueOrDefault();

			if (availRestriction.EndTimeLimitation.EndTime.HasValue)
				end = availRestriction.EndTimeLimitation.EndTime.GetValueOrDefault();

			var minutes = (int)end.Add(-start).TotalMinutes;

			if (availRestriction.WorkTimeLimitation.EndTime.HasValue)
			{
				minutes = availRestriction.WorkTimeLimitation.EndTime.Value.Minutes;
			}
			return minutes;
		}
	}
}