using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PersonAccountTest
    {
        private MockRepository _mocker;
        private DateOnly _baseDateTime;
        private IPerson _person;
        private IScenario _scenario;
      
        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _baseDateTime = new DateOnly(2001,1,1);
            _person = PersonFactory.CreatePerson();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
        }

        [Test]
        public void VerifyCanCalculateUsedFromRepository()
        {
            //-----Setup------
            DateOnly nextdateTime = new DateOnly(_baseDateTime.Date.AddMonths(6));
            IAbsence absenceWithDayTracker = new Absence {Tracker = Tracker.CreateDayTracker()};
            IPersonAccount personAccount1 = new PersonAccountDay(_baseDateTime, absenceWithDayTracker);
            IPersonAccount personAccount2 = new PersonAccountDay(nextdateTime, absenceWithDayTracker);
            _person.AddPersonAccount(personAccount1);
            _person.AddPersonAccount(personAccount2);

            IScheduleRepository repository = _mocker.CreateMock<IScheduleRepository>();
            IScheduleRange rangeToTrackfrom = _mocker.CreateMock<IScheduleRange>();
            IProjectionService projectionService = _mocker.CreateMock<IProjectionService>();
            IVisualLayerCollection projection = new VisualLayerCollection(_person, new List<IVisualLayer>());

            using(_mocker.Record())
            {

                Expect.Call(
                    repository.ScheduleRangeBasedOnAbsence(
                        personAccount1.Period().ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone()), _scenario, _person,
                        absenceWithDayTracker, AuthorizationService.DefaultService)).Return(rangeToTrackfrom);
                Expect.Call(rangeToTrackfrom.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(projection);
            }
            using(_mocker.Playback())
            {
                personAccount1.CalculateUsed(repository, null, _scenario);
            }
        }
    }

}