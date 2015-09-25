using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ScheduleSortingCommands
{
    [TestFixture]
    public class ScheduleSortingCommandsTest
    {
        private MockRepository _mocks;
        private ISchedulerStateHolder _stateHolder;
        private ISchedulingResultStateHolder _resultStateHolder;
        private IScheduleDictionary _scheduleDictionary;
        private IScheduleSortCommand _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _resultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_stateHolder = new SchedulerStateHolder(_resultStateHolder, _mocks.StrictMock<ICommonStateHolder>(), new TimeZoneGuardWrapper());
            _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifySort()
        {
            IPerson person1 = PersonFactory.CreatePerson("a");
			person1.SetId(Guid.NewGuid());
			IPerson person2 = PersonFactory.CreatePerson("b");
			person2.SetId(Guid.NewGuid());
			IPerson person3 = PersonFactory.CreatePerson("c");
			person3.SetId(Guid.NewGuid());
			IPerson person4 = PersonFactory.CreatePerson("d");
			person4.SetId(Guid.NewGuid());
			IPerson person5 = PersonFactory.CreatePerson("e");
			person5.SetId(Guid.NewGuid());
            IPerson person6 = PersonFactory.CreatePerson("f");
            person6.SetId(Guid.NewGuid());
            IPerson person7 = PersonFactory.CreatePerson("g");
            person7.SetId(Guid.NewGuid());
			IPerson person8 = PersonFactory.CreatePerson("h");
            person8.SetId(Guid.NewGuid());
            IList<IPerson> persons = new List<IPerson> { person1, person2, person3, person4, person5, person6, person7, person8 };
            _stateHolder.FilterPersons(persons);

			IScheduleDay schedulePart1 = _mocks.StrictMock<IScheduleDay>();
			IScheduleDay schedulePart2 = _mocks.StrictMock<IScheduleDay>();
			IScheduleDay schedulePart3 = _mocks.StrictMock<IScheduleDay>();
			IScheduleDay schedulePart4 = _mocks.StrictMock<IScheduleDay>();
			IScheduleDay schedulePart5 = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay schedulePart6 = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay schedulePart7 = _mocks.StrictMock<IScheduleDay>();
        	IScheduleDay schedulePart8 = _mocks.StrictMock<IScheduleDay>();

			//IList<IPerson> persons = new List<IPerson> { person1, person2, person3, person4, person5 };
			DateTimePeriod dateTimePeriod = new DateTimePeriod(new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2009, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(1));
			DateTimePeriod absencePeriod = new DateTimePeriod(new DateTime(2009, 1, 1, 8, 0, 0, DateTimeKind.Utc),
															  new DateTime(2009, 1, 1, 16, 0, 0, DateTimeKind.Utc));
			IScheduleRange range1 = _mocks.StrictMock<IScheduleRange>();
            IScheduleRange range2 = _mocks.StrictMock<IScheduleRange>();
            IScheduleRange range3 = _mocks.StrictMock<IScheduleRange>();
            IScheduleRange range4 = _mocks.StrictMock<IScheduleRange>();
            IScheduleRange range5 = _mocks.StrictMock<IScheduleRange>();
            IScheduleRange range6 = _mocks.StrictMock<IScheduleRange>();
            IScheduleRange range7 = _mocks.StrictMock<IScheduleRange>();
        	IScheduleRange range8 = _mocks.StrictMock<IScheduleRange>();

			IProjectionService projectionService1 = _mocks.StrictMock<IProjectionService>();
			IProjectionService projectionService2 = _mocks.StrictMock<IProjectionService>();
			IProjectionService projectionService3 = _mocks.StrictMock<IProjectionService>();
			IProjectionService projectionService4 = _mocks.StrictMock<IProjectionService>();
			IProjectionService projectionService5 = _mocks.StrictMock<IProjectionService>();
        	IProjectionService projectionService8 = _mocks.StrictMock<IProjectionService>();

			var layerFactory = new VisualLayerFactory();
			var activityLayerInBottom = layerFactory.CreateShiftSetupLayer( ActivityFactory.CreateActivity("underliggande"),absencePeriod,person1);
			var absenceLayer = layerFactory.CreateAbsenceSetupLayer(AbsenceFactory.CreateAbsence("Sick"), activityLayerInBottom, dateTimePeriod);

			IVisualLayerCollection layerCollection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, new TimeSpan(7, 0, 0), new TimeSpan(17, 30, 0));
			IVisualLayerCollection layerCollection2 = VisualLayerCollectionFactory.CreateForWorkShift(person2, new TimeSpan(6, 0, 0), new TimeSpan(16, 0, 0));
			IVisualLayerCollection layerCollection3 = VisualLayerCollectionFactory.CreateForWorkShift(person3, new TimeSpan(5, 30, 0), new TimeSpan(15, 0, 0));
			IVisualLayerCollection layerCollection4 = VisualLayerCollectionFactory.CreateForWorkShift(person4, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0));
			IVisualLayerCollection layerCollectionAllDay = new VisualLayerCollection(person7, new List<IVisualLayer> { absenceLayer }, new ProjectionPayloadMerger());
        	IVisualLayerCollection layerCollection8 = VisualLayerCollectionFactory.CreateForWorkShift(person8, new TimeSpan(18, 0, 0), new TimeSpan(18, 30, 0));

			using (_mocks.Record())
			{
			    Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary[person1]).Return(range1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDictionary[person2]).Return(range2).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDictionary[person3]).Return(range3).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDictionary[person4]).Return(range4).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDictionary[person5]).Return(range5).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDictionary[person6]).Return(range6).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDictionary[person7]).Return(range7).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary[person8]).Return(range8).Repeat.AtLeastOnce();

			    Expect.Call(range1.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart1).Repeat.AtLeastOnce();
                Expect.Call(range2.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart2).Repeat.AtLeastOnce();
                Expect.Call(range3.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart3).Repeat.AtLeastOnce();
                Expect.Call(range4.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart4).Repeat.AtLeastOnce();
                Expect.Call(range5.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart5).Repeat.AtLeastOnce();
                Expect.Call(range6.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart6).Repeat.AtLeastOnce();
                Expect.Call(range7.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart7).Repeat.AtLeastOnce();
				Expect.Call(range8.ScheduledDay(new DateOnly(2009, 1, 1))).Return(schedulePart8).Repeat.AtLeastOnce();

			    Expect.Call(schedulePart1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(schedulePart2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(schedulePart3.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(schedulePart4.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(schedulePart5.SignificantPart()).Return(SchedulePartView.None).Repeat.AtLeastOnce();
                Expect.Call(schedulePart6.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();
                Expect.Call(schedulePart7.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.AtLeastOnce();
				Expect.Call(schedulePart8.SignificantPart()).Return(SchedulePartView.Overtime).Repeat.AtLeastOnce();

				//Expect.Call(_scheduleDictionary.SchedulesForDay(new DateOnly(2009,1,1))).Return(readOnlyParts);

				Expect.Call(schedulePart1.Person).Return(person1);
				Expect.Call(schedulePart2.Person).Return(person2);
				Expect.Call(schedulePart3.Person).Return(person3);
				Expect.Call(schedulePart4.Person).Return(person4);
				Expect.Call(schedulePart7.Person).Return(person7);
				Expect.Call(schedulePart8.Person).Return(person8);

				Expect.Call(schedulePart1.ProjectionService()).Return(projectionService1).Repeat.AtLeastOnce();
                Expect.Call(schedulePart2.ProjectionService()).Return(projectionService2).Repeat.AtLeastOnce();
                Expect.Call(schedulePart3.ProjectionService()).Return(projectionService3).Repeat.AtLeastOnce();
                Expect.Call(schedulePart4.ProjectionService()).Return(projectionService4).Repeat.AtLeastOnce();
                Expect.Call(schedulePart7.ProjectionService()).Return(projectionService5).Repeat.AtLeastOnce();
				Expect.Call(schedulePart8.ProjectionService()).Return(projectionService8).Repeat.AtLeastOnce();

                Expect.Call(projectionService1.CreateProjection()).Return(layerCollection1).Repeat.AtLeastOnce();
                Expect.Call(projectionService2.CreateProjection()).Return(layerCollection2).Repeat.AtLeastOnce();
                Expect.Call(projectionService3.CreateProjection()).Return(layerCollection3).Repeat.AtLeastOnce();
                Expect.Call(projectionService4.CreateProjection()).Return(layerCollection4).Repeat.AtLeastOnce();
                Expect.Call(projectionService5.CreateProjection()).Return(layerCollectionAllDay).Repeat.AtLeastOnce();
				Expect.Call(projectionService8.CreateProjection()).Return(layerCollection8).Repeat.AtLeastOnce();
			}
			_mocks.ReplayAll();

            _target = new SortByEndDescendingCommand(_stateHolder);
			_target.Execute(new DateOnly(2009, 1, 1));

            Assert.IsTrue(_stateHolder.FilteredPersonDictionary.Count == 8);


				Assert.AreEqual(person8, _stateHolder.FilteredPersonDictionary.ElementAt(0).Value);
                Assert.AreEqual(person1, _stateHolder.FilteredPersonDictionary.ElementAt(1).Value);
                Assert.AreEqual(person2, _stateHolder.FilteredPersonDictionary.ElementAt(2).Value);
                Assert.AreEqual(person3, _stateHolder.FilteredPersonDictionary.ElementAt(3).Value);
                Assert.AreEqual(person4, _stateHolder.FilteredPersonDictionary.ElementAt(4).Value);
                Assert.AreEqual(person7, _stateHolder.FilteredPersonDictionary.ElementAt(5).Value);
                Assert.AreEqual(person6, _stateHolder.FilteredPersonDictionary.ElementAt(6).Value);
                Assert.AreEqual(person5, _stateHolder.FilteredPersonDictionary.ElementAt(7).Value);


                _target = new SortByEndAscendingCommand(_stateHolder);
                _target.Execute(new DateOnly(2009, 1, 1));

                Assert.AreEqual(person4, _stateHolder.FilteredPersonDictionary.ElementAt(0).Value);
                Assert.AreEqual(person3, _stateHolder.FilteredPersonDictionary.ElementAt(1).Value);
                Assert.AreEqual(person2, _stateHolder.FilteredPersonDictionary.ElementAt(2).Value);
                Assert.AreEqual(person1, _stateHolder.FilteredPersonDictionary.ElementAt(3).Value);
        		Assert.AreEqual(person8, _stateHolder.FilteredPersonDictionary.ElementAt(4).Value);
                Assert.AreEqual(person7, _stateHolder.FilteredPersonDictionary.ElementAt(5).Value);	
                Assert.AreEqual(person6, _stateHolder.FilteredPersonDictionary.ElementAt(6).Value);
                Assert.AreEqual(person5, _stateHolder.FilteredPersonDictionary.ElementAt(7).Value);


                _target = new SortByStartDescendingCommand(_stateHolder);
                _target.Execute(new DateOnly(2009, 1, 1));

				Assert.AreEqual(person8, _stateHolder.FilteredPersonDictionary.ElementAt(0).Value);
                Assert.AreEqual(person4, _stateHolder.FilteredPersonDictionary.ElementAt(1).Value);
                Assert.AreEqual(person1, _stateHolder.FilteredPersonDictionary.ElementAt(2).Value);
                Assert.AreEqual(person2, _stateHolder.FilteredPersonDictionary.ElementAt(3).Value);
                Assert.AreEqual(person3, _stateHolder.FilteredPersonDictionary.ElementAt(4).Value);
                Assert.AreEqual(person7, _stateHolder.FilteredPersonDictionary.ElementAt(5).Value);
                Assert.AreEqual(person6, _stateHolder.FilteredPersonDictionary.ElementAt(6).Value);
                Assert.AreEqual(person5, _stateHolder.FilteredPersonDictionary.ElementAt(7).Value);


                _target = new SortByStartAscendingCommand(_stateHolder);
                _target.Execute(new DateOnly(2009, 1, 1));

                Assert.AreEqual(person3, _stateHolder.FilteredPersonDictionary.ElementAt(0).Value);
                Assert.AreEqual(person2, _stateHolder.FilteredPersonDictionary.ElementAt(1).Value);
                Assert.AreEqual(person1, _stateHolder.FilteredPersonDictionary.ElementAt(2).Value);
                Assert.AreEqual(person4, _stateHolder.FilteredPersonDictionary.ElementAt(3).Value);
				Assert.AreEqual(person8, _stateHolder.FilteredPersonDictionary.ElementAt(4).Value);
                Assert.AreEqual(person7, _stateHolder.FilteredPersonDictionary.ElementAt(5).Value);	
                Assert.AreEqual(person6, _stateHolder.FilteredPersonDictionary.ElementAt(6).Value);
                Assert.AreEqual(person5, _stateHolder.FilteredPersonDictionary.ElementAt(7).Value);

                _target = new NoSortCommand(_stateHolder);
                _target.Execute(new DateOnly(2009, 1, 1));

                Assert.AreEqual(person3, _stateHolder.FilteredPersonDictionary.ElementAt(0).Value);
                Assert.AreEqual(person2, _stateHolder.FilteredPersonDictionary.ElementAt(1).Value);
                Assert.AreEqual(person1, _stateHolder.FilteredPersonDictionary.ElementAt(2).Value);
                Assert.AreEqual(person4, _stateHolder.FilteredPersonDictionary.ElementAt(3).Value);
                Assert.AreEqual(person8, _stateHolder.FilteredPersonDictionary.ElementAt(4).Value);
				Assert.AreEqual(person7, _stateHolder.FilteredPersonDictionary.ElementAt(5).Value);
                Assert.AreEqual(person6, _stateHolder.FilteredPersonDictionary.ElementAt(6).Value);
                Assert.AreEqual(person5, _stateHolder.FilteredPersonDictionary.ElementAt(7).Value);

                _target = new SortByContractTimeAscendingCommand(_stateHolder);
                _target.Execute(new DateOnly(2009, 1, 1));

				Assert.AreEqual(person8, _stateHolder.FilteredPersonDictionary.ElementAt(0).Value);
                Assert.AreEqual(person4, _stateHolder.FilteredPersonDictionary.ElementAt(1).Value);
                Assert.AreEqual(person3, _stateHolder.FilteredPersonDictionary.ElementAt(2).Value);
                Assert.AreEqual(person2, _stateHolder.FilteredPersonDictionary.ElementAt(3).Value);
                Assert.AreEqual(person1, _stateHolder.FilteredPersonDictionary.ElementAt(4).Value);
                Assert.AreEqual(person7, _stateHolder.FilteredPersonDictionary.ElementAt(5).Value);
                Assert.AreEqual(person6, _stateHolder.FilteredPersonDictionary.ElementAt(6).Value);
                Assert.AreEqual(person5, _stateHolder.FilteredPersonDictionary.ElementAt(7).Value);

                _target = new SortByContractTimeDescendingCommand(_stateHolder);
                _target.Execute(new DateOnly(2009, 1, 1));

                Assert.AreEqual(person1, _stateHolder.FilteredPersonDictionary.ElementAt(0).Value);
                Assert.AreEqual(person2, _stateHolder.FilteredPersonDictionary.ElementAt(1).Value);
                Assert.AreEqual(person3, _stateHolder.FilteredPersonDictionary.ElementAt(2).Value);
                Assert.AreEqual(person4, _stateHolder.FilteredPersonDictionary.ElementAt(3).Value);
				Assert.AreEqual(person8, _stateHolder.FilteredPersonDictionary.ElementAt(4).Value);
                Assert.AreEqual(person7, _stateHolder.FilteredPersonDictionary.ElementAt(5).Value);
                Assert.AreEqual(person6, _stateHolder.FilteredPersonDictionary.ElementAt(6).Value);
                Assert.AreEqual(person5, _stateHolder.FilteredPersonDictionary.ElementAt(7).Value);

        }
    }
}