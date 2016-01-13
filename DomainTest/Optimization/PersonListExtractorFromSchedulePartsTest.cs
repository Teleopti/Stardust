using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class PersonListExtractorFromSchedulePartsTest
    {
        private PersonListExtractorFromScheduleParts _target;
        private MockRepository _mock;
 
        private IList<IScheduleDay> _scheduleDays;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private IPerson _person1;
        private IPerson _person2;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _person1 = PersonFactory.CreatePerson("A");
            _person2 = PersonFactory.CreatePerson("B");
            _scheduleDays = new List<IScheduleDay>{ _scheduleDay1, _scheduleDay2, _scheduleDay3 };
            _target = new PersonListExtractorFromScheduleParts();
        }

        [Test]
        public void ShouldFindUniquePersons()
        {
            const int uniquePersons = 2;

            using(_mock.Record())
            {
                Expect.Call(_scheduleDay1.Person)
                    .Return(_person1);
                Expect.Call(_scheduleDay2.Person)
                    .Return(_person1);
                Expect.Call(_scheduleDay3.Person)
                    .Return(_person2);

            }
            using(_mock.Playback())
            {
				IList<IPerson> result = _target.ExtractPersons(_scheduleDays);
                Assert.AreEqual(uniquePersons, result.Count);
                Assert.AreEqual(_person1, result[0]);
                Assert.AreEqual(_person2, result[1]);
            } 

        }


    }
}