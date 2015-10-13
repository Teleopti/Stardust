using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
	public class IntradayAvailabilityTransformerTest
    {
        private IIntradayAvailabilityTransformer _target;
	    private MockRepository _mocks;
	    private IScenario _scenario;
	    private ICommonStateHolder _stateHolder;

	    [SetUp]
        public void Setup()
        {
			_target = new IntradayHourlyAvailabilityTransformer();
	        _mocks = new MockRepository();
		    _scenario = new Scenario("name");
			_scenario.SetId(Guid.NewGuid());
		    _stateHolder = _mocks.DynamicMock<ICommonStateHolder>();
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


			Expect.Call(_stateHolder.PersonsWithIds(new List<Guid>())).IgnoreArguments().Return(new List<IPerson> { person });
			Expect.Call(_stateHolder.GetSchedulePartOnPersonAndDate(person, studDay.RestrictionDate, scenario)).Return(schedDay);

			Expect.Call(schedDay.Scenario).Return(scenario);
			Expect.Call(schedDay.Person).Return(person);
			Expect.Call(schedDay.IsScheduled()).Return(true);
			Expect.Call(schedDay.ProjectionService()).Return(projService);
			Expect.Call(projService.CreateProjection()).Return(layers);
			Expect.Call(layers.HasLayers).Return(true);
			Expect.Call(layers.WorkTime()).Return(TimeSpan.FromHours(5));
			_mocks.ReplayAll();
			_target.Transform(new List<IStudentAvailabilityDay> { studDay }, table, _stateHolder, scenario);
			Assert.That(table.Rows.Count, Is.EqualTo(1));
			_mocks.VerifyAll();
		}       
    }
}
