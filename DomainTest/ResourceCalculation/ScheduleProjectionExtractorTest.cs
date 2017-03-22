using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
	    [Test]
	    public void VerifyEverythingWorks()
	    {
		    IPerson p1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 1, 1));
		    IPerson p2 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2000, 1, 1));

		    IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
		    DateTimePeriod dtp = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);

		    ScheduleDictionary dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(dtp),
			    new PersistableScheduleDataPermissionChecker());
		    var period = new DateTimePeriod(2000, 6, 1, 2000, 7, 1);
		    IPersonAssignment pAss =
			    PersonAssignmentFactory.CreateAssignmentWithMainShift(p1, scenario, period);

		    IPersonAbsence pAbs =
			    PersonAbsenceFactory.CreatePersonAbsence(p2, scenario,
				    new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
		    ((ScheduleRange) dic[pAss.Person]).Add(pAss);
		    ((ScheduleRange) dic[pAbs.Person]).Add(pAbs);

			var target = new ScheduleProjectionExtractor(new PersonSkillProvider(), 15, false);
			var retList = target.CreateRelevantProjectionList(dic);
		    Assert.IsTrue(retList.HasItems());

		    retList = target.CreateRelevantProjectionList(dic, period);
		    Assert.IsTrue(retList.HasItems());
	    }
    }
}
