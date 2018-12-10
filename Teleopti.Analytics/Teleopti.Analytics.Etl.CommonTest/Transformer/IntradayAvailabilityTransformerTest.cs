using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class IntradayAvailabilityTransformerTest
	{
		private IntradayHourlyAvailabilityTransformer _target;
		private IScenario _scenario;
		private ICommonStateHolder _stateHolder;

		[SetUp]
		public void Setup()
		{
			_target = new IntradayHourlyAvailabilityTransformer();
			_scenario = new Scenario("name");
			_scenario.SetId(Guid.NewGuid());
			_stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
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
			var schedDay = MockRepository.GenerateMock<IScheduleDay>();
			var studRestriction = new StudentAvailabilityRestriction();
			var date = new DateOnly(2013, 5, 15);
			var studDay = new StudentAvailabilityDay(person, date, new List<IStudentAvailabilityRestriction> { studRestriction });
			var schedDic = MockRepository.GenerateMock<IScheduleDictionary>();
			var range = MockRepository.GenerateMock<IScheduleRange>();
			var dic = new Dictionary<DateOnly, IScheduleDictionary> { { date, schedDic } };

			var projService = MockRepository.GenerateMock<IProjectionService>();
			var layers = MockRepository.GenerateMock<IVisualLayerCollection>();

			_stateHolder.Stub(x => x.GetSchedules(Arg<HashSet<IStudentAvailabilityDay>>.Is.Anything, Arg<IScenario>.Is.Same(scenario))).Return(dic);
			_stateHolder.Stub(x => x.PersonsWithIds(new List<Guid>())).IgnoreArguments().Return(new List<IPerson> { person });
			schedDic.Stub(x => x.TryGetValue(person, out range)).OutRef(range).Return(true);
			
			range.Stub(x => x.ScheduledDay(date)).Return(schedDay);
			schedDay.Stub(x => x.Scenario).Return(scenario);
			schedDay.Stub(x => x.Person).Return(person);
			schedDay.Stub(x => x.IsScheduled()).Return(true);
			schedDay.Stub(x => x.ProjectionService()).Return(projService);
			projService.Stub(x => x.CreateProjection()).Return(layers);
			layers.Stub(x => x.HasLayers).Return(true);
			layers.Stub(x => x.WorkTime()).Return(TimeSpan.FromHours(5));

			_target.Transform(new List<IStudentAvailabilityDay> { studDay }, table, _stateHolder, scenario);
			Assert.That(table.Rows.Count, Is.EqualTo(1));
		}

		[Test]
		public void ShouldOnlyUseUniqeDays()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());

			var studDay1 = new StudentAvailabilityDay(person, new DateOnly(2013, 5, 15), new List<IStudentAvailabilityRestriction>());
			var studDay2 = new StudentAvailabilityDay(person, new DateOnly(2013, 5, 15), new List<IStudentAvailabilityRestriction>());
			var uniqueDays = _target.CreateStudentAvailabilityDaySetWithPersonDateEqualitity();
			uniqueDays.Add(studDay1);
			uniqueDays.Add(studDay2);
			uniqueDays.Count.Should().Be.EqualTo(1);
		}
	}
}
