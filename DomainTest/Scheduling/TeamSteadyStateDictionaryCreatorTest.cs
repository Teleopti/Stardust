using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class TeamSteadyStateDictionaryCreatorTest
	{
		private TeamSteadyStateDictionaryCreator _target;
		private MockRepository _mocks;
		private IGroupPerson _groupPerson1;
		private IGroupPerson _groupPerson2;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private IPerson _person4;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IScheduleMatrixPro _scheduleMatrixPro3;
		private IScheduleMatrixPro _scheduleMatrixPro4;
		private IList<IScheduleMatrixPro> _matrixes;
		private DateOnly _dateOnly;
		private ISchedulingResultStateHolder _stateHolder;
		private IGroupPersonsBuilder _groupPersonsBuilder;
		private ISchedulingOptions _schedulingOptions;
		private DateOnlyPeriod _dates;
		private Guid _guid1;
		private Guid _guid2;
		private ITeamSteadyStateRunner _teamSteadyStateRunner;
		private IList<IPerson> _persons;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_guid1 = Guid.NewGuid();
			_guid2 = Guid.NewGuid();

			_dateOnly = new DateOnly(2012, 1, 1);
			_dates = new DateOnlyPeriod(_dateOnly, _dateOnly);
			var dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(30));

			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person1"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person2"), _dateOnly);
			_person3 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person3"), _dateOnly);
			_person4 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("person4"), _dateOnly);

			_groupPerson1 = new GroupPerson(new List<IPerson> { _person1, _person2 }, _dateOnly, "groupPerson1", _guid1);
			_groupPerson2 = new GroupPerson(new List<IPerson> { _person3, _person4 }, _dateOnly, "groupPerson2", _guid2);

			setupStateHolder();

			_scheduleMatrixPro1 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person1, _person1.VirtualSchedulePeriod(_dateOnly));
			_scheduleMatrixPro2 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person2, _person2.VirtualSchedulePeriod(_dateOnly));
			_scheduleMatrixPro3 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person3, _person3.VirtualSchedulePeriod(_dateOnly));
			_scheduleMatrixPro4 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person4, _person4.VirtualSchedulePeriod(_dateOnly));

			_matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, _scheduleMatrixPro2, _scheduleMatrixPro3, _scheduleMatrixPro4 };

			_groupPersonsBuilder = _mocks.StrictMock<IGroupPersonsBuilder>();
			_schedulingOptions = new SchedulingOptions();
			_teamSteadyStateRunner = _mocks.StrictMock<ITeamSteadyStateRunner>();
			_target = new TeamSteadyStateDictionaryCreator(_teamSteadyStateRunner, _matrixes, _groupPersonsBuilder, _schedulingOptions);
			_persons = new List<IPerson>{_person1, _person2, _person3, _person4};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void setupStateHolder()
		{
			var wholePeriod = new DateTimePeriod(2012, 1, 1, 2012, 3, 31);
			IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(wholePeriod);
			IScenario scenario = new Scenario("Scenario");
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, scheduleDateTimePeriod, new Dictionary<IPerson, IScheduleRange>());

			var dayPeriod = new DateTimePeriod(2012, 1, 1, 2012, 3, 31);
			IScheduleParameters parameters1 = new ScheduleParameters(scenario, _person1, dayPeriod);
			IScheduleParameters parameters2 = new ScheduleParameters(scenario, _person2, dayPeriod);
			IScheduleParameters parameters3 = new ScheduleParameters(scenario, _person3, dayPeriod);
			IScheduleParameters parameters4 = new ScheduleParameters(scenario, _person4, dayPeriod);
			IScheduleRange range1 = new ScheduleRange(scheduleDictionary, parameters1);
			IScheduleRange range2 = new ScheduleRange(scheduleDictionary, parameters2);
			IScheduleRange range3 = new ScheduleRange(scheduleDictionary, parameters3);
			IScheduleRange range4 = new ScheduleRange(scheduleDictionary, parameters4);

			scheduleDictionary.AddTestItem(_person1, range1);
			scheduleDictionary.AddTestItem(_person2, range2);
			scheduleDictionary.AddTestItem(_person3, range3);
			scheduleDictionary.AddTestItem(_person4, range4);

			_stateHolder = new SchedulingResultStateHolder { Schedules = scheduleDictionary };
		}

		[Test]
		public void ShouldCreateDictionary()
		{
			using(_mocks.Record())
			{
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly, _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson1 });
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly, _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson1 });
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly, _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson2 });
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly, _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson2 });
				Expect.Call(_teamSteadyStateRunner.Run(_groupPerson1, _dateOnly)).Return(new KeyValuePair<Guid, bool>(_guid1, true));
				Expect.Call(_teamSteadyStateRunner.Run(_groupPerson2, _dateOnly)).Return(new KeyValuePair<Guid, bool>(_guid2, true));
			}

			using(_mocks.Playback())
			{
				var result = _target.Create(_dates);
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Count);
				Assert.IsTrue(result[_guid1]);
				Assert.IsTrue(result[_guid2]);
			}				
		}

		[Test]
		public void ShouldSetSteadyStateToFalseIfRunnerReturnFalse()
		{
			using (_mocks.Record())
			{
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly, _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson1 });
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly, _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson1 });
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly, _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson2 });
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly, _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson2 });
				Expect.Call(_teamSteadyStateRunner.Run(_groupPerson1, _dateOnly)).Return(new KeyValuePair<Guid, bool>(_guid1, false));
				Expect.Call(_teamSteadyStateRunner.Run(_groupPerson2, _dateOnly)).Return(new KeyValuePair<Guid, bool>(_guid2, true));
			}

			using (_mocks.Playback())
			{
				var result = _target.Create(_dates);
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Count);
				Assert.IsFalse(result[_guid1]);
				Assert.IsTrue(result[_guid2]);
			}			
		}

		[Test]
		public void ShouldHandleMultiplePeriods()
		{
			var selection = new DateOnlyPeriod(_dateOnly.AddDays(5), _dateOnly.AddDays(40));
			var dateOnlyPeriod = new DateOnlyPeriod(_dateOnly.AddDays(31), _dateOnly.AddDays(60));
			_person1.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(dateOnlyPeriod.StartDate));
			var scheduleMatrixPro2 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person1, _person1.VirtualSchedulePeriod(dateOnlyPeriod.StartDate));
			_matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, scheduleMatrixPro2};
			_target = new TeamSteadyStateDictionaryCreator(_teamSteadyStateRunner, _matrixes, _groupPersonsBuilder, _schedulingOptions);
			_persons = new List<IPerson> { _person1 };

			using (_mocks.Record())
			{
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly.AddDays(5), _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson1 });
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly.AddDays(40), _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson1 });
				Expect.Call(_teamSteadyStateRunner.Run(_groupPerson1, _dateOnly)).Return(new KeyValuePair<Guid, bool>(_guid1, true));
				Expect.Call(_teamSteadyStateRunner.Run(_groupPerson1, dateOnlyPeriod.StartDate)).Return(new KeyValuePair<Guid, bool>(_guid1, true));	
			}

			using (_mocks.Playback())
			{
				var result = _target.Create(selection);
				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Count);
				Assert.IsTrue(result[_guid1]);	
			}
		}

		[Test]
		public void ShouldSetSteadyStateToFalseIfAnyPeriodIsFalse()
		{
			var selection = new DateOnlyPeriod(_dateOnly.AddDays(5), _dateOnly.AddDays(40));
			var dateOnlyPeriod = new DateOnlyPeriod(_dateOnly.AddDays(31), _dateOnly.AddDays(60));
			_person1.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(dateOnlyPeriod.StartDate));
			var scheduleMatrixPro2 = ScheduleMatrixProFactory.Create(dateOnlyPeriod, _stateHolder, _person1, _person1.VirtualSchedulePeriod(dateOnlyPeriod.StartDate));
			_matrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro1, scheduleMatrixPro2 };
			_target = new TeamSteadyStateDictionaryCreator(_teamSteadyStateRunner, _matrixes, _groupPersonsBuilder, _schedulingOptions);
			_persons = new List<IPerson> { _person1 };

			using (_mocks.Record())
			{
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly.AddDays(5), _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson1 });
				Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnly.AddDays(40), _persons, true, _schedulingOptions)).Return(new List<IGroupPerson> { _groupPerson1 });
				Expect.Call(_teamSteadyStateRunner.Run(_groupPerson1, _dateOnly)).Return(new KeyValuePair<Guid, bool>(_guid1, false));
			}

			using (_mocks.Playback())
			{
				var result = _target.Create(selection);
				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Count);
				Assert.IsFalse(result[_guid1]);
			}
		}
	}
}
