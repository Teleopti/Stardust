using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.ScheduleSortingCommands
{
    [TestFixture]
	public class ScheduleSortBySeniorityRankingCommandsTest
    {
        private MockRepository _mocks;
        private SchedulerStateHolder _stateHolder;
        private ISchedulingResultStateHolder _resultStateHolder;
        private IScheduleDictionary _scheduleDictionary;
        private IScheduleSortCommand _target;
	    private IRankedPersonBasedOnStartDate _personRankCalculator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _resultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_stateHolder = new SchedulerStateHolder(_resultStateHolder, _mocks.StrictMock<ICommonStateHolder>(), new TimeZoneGuard());
            _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
	        _personRankCalculator = _mocks.StrictMock<IRankedPersonBasedOnStartDate>();
        }

		[Test]
        public void VerifySortAscending()
        {
            IPerson person1 = PersonFactory.CreatePerson("Rank 3");
			person1.SetId(Guid.NewGuid());
			IPerson person2 = PersonFactory.CreatePerson("Rank 2");
			person2.SetId(Guid.NewGuid());
			IPerson person3 = PersonFactory.CreatePerson("Rank 4");
			person3.SetId(Guid.NewGuid());
			IPerson person4 = PersonFactory.CreatePerson("Rank 5");
			person4.SetId(Guid.NewGuid());
			IPerson person5 = PersonFactory.CreatePerson("Rank 1");
			person5.SetId(Guid.NewGuid());
      
			setMock(person1, person2, person3, person4, person5);

			_target = new SortBySeniorityRankingAscendingCommand(_stateHolder, _personRankCalculator);
			_target.Execute(new DateOnly(2009, 1, 1));

            Assert.IsTrue(_stateHolder.FilteredCombinedAgentsDictionary.Count == 5);

            Assert.AreEqual(person5, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(0).Value);
            Assert.AreEqual(person2, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(1).Value);
            Assert.AreEqual(person1, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(2).Value);
            Assert.AreEqual(person3, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(3).Value);
            Assert.AreEqual(person4, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(4).Value);
        }

		[Test]
		public void VerifySortDescending()
		{
			IPerson person1 = PersonFactory.CreatePerson("Rank 3");
			person1.SetId(Guid.NewGuid());
			IPerson person2 = PersonFactory.CreatePerson("Rank 2");
			person2.SetId(Guid.NewGuid());
			IPerson person3 = PersonFactory.CreatePerson("Rank 4");
			person3.SetId(Guid.NewGuid());
			IPerson person4 = PersonFactory.CreatePerson("Rank 5");
			person4.SetId(Guid.NewGuid());
			IPerson person5 = PersonFactory.CreatePerson("Rank 1");
			person5.SetId(Guid.NewGuid());

			setMock(person1, person2, person3, person4, person5);

			_target = new SortBySeniorityRankingDescendingCommand(_stateHolder, _personRankCalculator);
			_target.Execute(new DateOnly(2009, 1, 1));

			Assert.IsTrue(_stateHolder.FilteredCombinedAgentsDictionary.Count == 5);

			Assert.AreEqual(person4, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(0).Value);
			Assert.AreEqual(person3, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(1).Value);
			Assert.AreEqual(person1, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(2).Value);
			Assert.AreEqual(person2, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(3).Value);
			Assert.AreEqual(person5, _stateHolder.FilteredCombinedAgentsDictionary.ElementAt(4).Value);
		}

	    private void setMock(IPerson person1, IPerson person2, IPerson person3, IPerson person4, IPerson person5)
	    {

			IList<IPerson> persons = new List<IPerson> { person1, person2, person3, person4, person5 };
			_stateHolder.FilterPersons(persons);

		    IScheduleDay schedulePart1 = _mocks.StrictMock<IScheduleDay>();
		    IScheduleDay schedulePart2 = _mocks.StrictMock<IScheduleDay>();
		    IScheduleDay schedulePart3 = _mocks.StrictMock<IScheduleDay>();
		    IScheduleDay schedulePart4 = _mocks.StrictMock<IScheduleDay>();
		    IScheduleDay schedulePart5 = _mocks.StrictMock<IScheduleDay>();

		    IScheduleRange range1 = _mocks.StrictMock<IScheduleRange>();
		    IScheduleRange range2 = _mocks.StrictMock<IScheduleRange>();
		    IScheduleRange range3 = _mocks.StrictMock<IScheduleRange>();
		    IScheduleRange range4 = _mocks.StrictMock<IScheduleRange>();
		    IScheduleRange range5 = _mocks.StrictMock<IScheduleRange>();

		    IDictionary<IPerson, int> unsortedDictionary = new Dictionary<IPerson, int>();
		    unsortedDictionary.Add(person1, 3);
		    unsortedDictionary.Add(person2, 2);
		    unsortedDictionary.Add(person3, 4);
		    unsortedDictionary.Add(person4, 5);
		    unsortedDictionary.Add(person5, 1);

		    using (_mocks.Record())
		    {
			    Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
			    Expect.Call(_scheduleDictionary[person1]).Return(range1).Repeat.AtLeastOnce();
			    Expect.Call(_scheduleDictionary[person2]).Return(range2).Repeat.AtLeastOnce();
			    Expect.Call(_scheduleDictionary[person3]).Return(range3).Repeat.AtLeastOnce();
			    Expect.Call(_scheduleDictionary[person4]).Return(range4).Repeat.AtLeastOnce();
			    Expect.Call(_scheduleDictionary[person5]).Return(range5).Repeat.AtLeastOnce();

			    Expect.Call(range1.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart1).Repeat.AtLeastOnce();
			    Expect.Call(range2.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart2).Repeat.AtLeastOnce();
			    Expect.Call(range3.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart3).Repeat.AtLeastOnce();
			    Expect.Call(range4.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart4).Repeat.AtLeastOnce();
			    Expect.Call(range5.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart5).Repeat.AtLeastOnce();

			    Expect.Call(schedulePart1.Person).Return(person1);
			    Expect.Call(schedulePart2.Person).Return(person2);
			    Expect.Call(schedulePart3.Person).Return(person3);
			    Expect.Call(schedulePart4.Person).Return(person4);
			    Expect.Call(schedulePart5.Person).Return(person5);

			    Expect.Call(_personRankCalculator.GetRankedPersonDictionary(persons)).IgnoreArguments()
			          .Return(unsortedDictionary);
		    }
		    _mocks.ReplayAll();
	    }
    }
}