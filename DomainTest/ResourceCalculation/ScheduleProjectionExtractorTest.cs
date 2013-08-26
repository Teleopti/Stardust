using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ScheduleProjectionExtractorTest
    {

        ScheduleProjectionExtractor _target;

        [SetUp]
        public void Setup()
        {
            _target = new ScheduleProjectionExtractor();
        }

        [Test]
        public void VerifyEverythingWorks()
        {
            IPerson p1 = PersonFactory.CreatePerson();
            IPerson p2 = PersonFactory.CreatePerson();

            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            DateTimePeriod dtp =  new DateTimePeriod(2000, 1, 1, 2001, 1, 1);

            ScheduleDictionary dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(dtp));
            var period = new DateTimePeriod(2000, 6, 1, 2000, 7, 1);
            IPersonAssignment pAss =
                PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, p1,period);
  
            
            IPersonAbsence pAbs =
                PersonAbsenceFactory.CreatePersonAbsence(p2, scenario,
                                                         new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            ((ScheduleRange)dic[pAss.Person]).Add(pAss);
            ((ScheduleRange)dic[pAbs.Person]).Add(pAbs);

            var retList = _target.CreateRelevantProjectionList(dic);
            Assert.AreEqual(1, retList.Count);

            retList = _target.CreateRelevantProjectionList(dic, period);
            Assert.AreEqual(1, retList.Count);      

        }

    }
}
