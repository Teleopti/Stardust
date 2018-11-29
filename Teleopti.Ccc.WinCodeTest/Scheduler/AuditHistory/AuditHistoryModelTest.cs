using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AuditHistory;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AuditHistory
{
    [TestFixture]
    public class AuditHistoryModelTest
    {
        private AuditHistoryModel _model;
        private IScheduleDay _scheduleDay;
        private IScheduleHistoryRepository _scheduleHistoryRepository;
        private IAuditHistoryScheduleDayCreator _auditHistoryScheduleDayCreator;
        private MockRepository _mocks;
        private IList<IRevision> _revisions;
        private IList<IPersistableScheduleData> _scheduleData;
        private IPerson _person
            ;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _scheduleHistoryRepository = _mocks.StrictMock<IScheduleHistoryRepository>();
            _auditHistoryScheduleDayCreator = _mocks.StrictMock<IAuditHistoryScheduleDayCreator>();
            _model = new AuditHistoryModel(_scheduleDay, _scheduleHistoryRepository, _auditHistoryScheduleDayCreator);
            _person = PersonFactory.CreatePerson();
            _revisions = revisonList();
            _scheduleData = new List<IPersistableScheduleData>();

        }

        [Test]
        public void ShouldGetSetSelectedScheduleDay()
        {
            _model.SelectedScheduleDay = _scheduleDay;
            Assert.AreEqual(_scheduleDay, _model.SelectedScheduleDay);
        }

        [Test]
        public void ShouldGetHeaderText()
        {
            IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2000, 1, 1), TimeZoneInfoFactory.UtcTimeZoneInfo());

            using(_mocks.Record())
            {
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
                Expect.Call(_scheduleDay.Person).Return(_person);
            }

            using(_mocks.Playback())
            {
                var expectedString = dateOnlyAsDateTimePeriod.DateOnly.Date.ToShortDateString() + " - " + _person.Name;
                Assert.AreEqual(expectedString, _model.HeaderText);
            }
        }

        [Test]
        public void ShouldGetCurrentScheduleDay()
        {
            Assert.AreEqual(_scheduleDay, _model.CurrentScheduleDay);    
        }

        [Test]
        public void VerifyFirstEarlierAndLater()
        {
            using(_mocks.Record())
            {
                mockExpectationsFirst();
                for (int i = 10; i < 12; i++)
                {
									var refetchedScheduleDay = MockRepository.GenerateMock<IScheduleDay>();
									Expect.Call(_scheduleDay.ReFetch()).Return(refetchedScheduleDay);
                    Expect.Call(_scheduleHistoryRepository.FindSchedules(_revisions[i], _person, new DateOnly(2000, 1, 1))).Return(_scheduleData);
										Expect.Call(() => _auditHistoryScheduleDayCreator.Apply(refetchedScheduleDay, _scheduleData));
                }
                for (int i = 0; i < 10; i++)
								{
									var refetchedScheduleDay = MockRepository.GenerateMock<IScheduleDay>();
									Expect.Call(_scheduleDay.ReFetch()).Return(refetchedScheduleDay);
                    Expect.Call(_scheduleHistoryRepository.FindSchedules(_revisions[i], _person, new DateOnly(2000, 1, 1))).Return(_scheduleData);
                    Expect.Call(() => _auditHistoryScheduleDayCreator.Apply(refetchedScheduleDay, _scheduleData));
                }
            }

            using (_mocks.Playback())
            {
                _model.First();
                Assert.AreEqual(10, _model.PageRows.Count);
                Assert.AreEqual(1, _model.CurrentPage);
                Assert.AreEqual(2, _model.NumberOfPages);
               
                _model.Earlier();
                Assert.AreEqual(2, _model.PageRows.Count);
                Assert.AreEqual(2, _model.CurrentPage);
                
                _model.Later();
                Assert.AreEqual(10, _model.PageRows.Count);
                Assert.AreEqual(1, _model.CurrentPage);
            }
            
        }

        private void mockExpectationsFirst()
        {
            IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2000, 1, 1),
                                                                                          TimeZoneInfoFactory.
                                                                                              UtcTimeZoneInfo());

            Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_scheduleHistoryRepository.FindRevisions(_person, new DateOnly(2000, 1, 1), 10000)).Return(_revisions);
            for (int i = 0; i < Math.Min(_revisions.Count, 10); i++)
            {
							var refetchedScheduleDay = MockRepository.GenerateMock<IScheduleDay>();
							Expect.Call(_scheduleDay.ReFetch()).Return(refetchedScheduleDay);
                Expect.Call(_scheduleHistoryRepository.FindSchedules(_revisions[i], _person, new DateOnly(2000, 1, 1))).Return(_scheduleData);
                Expect.Call(() => _auditHistoryScheduleDayCreator.Apply(refetchedScheduleDay, _scheduleData));
            }
        }

        private static IList<IRevision> revisonList()
        {
            IList<IRevision> retList = new List<IRevision>();
            Revision rev = new Revision();
            rev.Id = 1;
            rev.SetRevisionData(PersonFactory.CreatePerson("p1"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 2;
            rev.SetRevisionData(PersonFactory.CreatePerson("p2"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 3;
            rev.SetRevisionData(PersonFactory.CreatePerson("p3"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 4;
            rev.SetRevisionData(PersonFactory.CreatePerson("p4"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 5;
            rev.SetRevisionData(PersonFactory.CreatePerson("p5"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 6;
            rev.SetRevisionData(PersonFactory.CreatePerson("p6"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 7;
            rev.SetRevisionData(PersonFactory.CreatePerson("p7"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 8;
            rev.SetRevisionData(PersonFactory.CreatePerson("p8"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 9;
            rev.SetRevisionData(PersonFactory.CreatePerson("p9"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 10;
            rev.SetRevisionData(PersonFactory.CreatePerson("p10"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 11;
            rev.SetRevisionData(PersonFactory.CreatePerson("p1"));
            retList.Add(rev);

            rev = new Revision();
            rev.Id = 12;
            rev.SetRevisionData(PersonFactory.CreatePerson("p12"));
            retList.Add(rev);

            return retList;
        }
    }
}