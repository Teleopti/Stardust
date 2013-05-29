using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Transformer;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "studDay"), Test]
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

	
}