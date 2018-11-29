using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    //tests when permissions are disabled on scheduledictionary
    [TestFixture]
    public class ScheduleRangeLowPermissionCheckTest
    {
	    private const string function = DefinedRaptorApplicationFunctionPaths.ViewSchedules;
    
        private IScheduleParameters parameters;
        private IPerson person;
        private IScenario scenario;
        private ScheduleRange target;
        private IScheduleDictionary dic;
        private IAuthorization authorization;

        [SetUp]
        public void Setup()
        {
            person = PersonFactory.CreatePerson();
            scenario = new Scenario("sdf");
            authorization = MockRepository.GenerateMock<IAuthorization>();
            parameters = new ScheduleParameters(scenario, person, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			dic = MockRepository.GenerateMock<IScheduleDictionary>();
            target = new ScheduleRange(dic, parameters, new PersistableScheduleDataPermissionChecker(new ThisAuthorization(authorization)), new ThisAuthorization(authorization));
        }

        [Test]
        public void InitializeCanAddOutsidePermissionScopeAndAnExtractorCanReadThisData()
        {
            extractor extractor = new extractor();
            DateTime paramStart = parameters.Period.StartDateTime;
            IPersonAssignment assOutSide = createPersonAssignment(new DateTimePeriod(paramStart.AddDays(20), paramStart.AddDays(32)));
            IPersonAssignment assInside = createPersonAssignment(new DateTimePeriod(paramStart.AddDays(1), paramStart.AddDays(10)));
            var absOutSide = createPersonAbsence(new DateTimePeriod(paramStart.AddDays(20), paramStart.AddDays(30)));
            absOutSide.SetId(Guid.NewGuid());
            var absInside = createPersonAbsence(new DateTimePeriod(paramStart.AddDays(1), paramStart.AddDays(10)));
            absInside.SetId(Guid.NewGuid());

            var dop = new DateOnlyPeriod(2000, 1, 1, 2000, 12, 31);
            authorization.Stub(x => x.PermittedPeriods(function, new DateOnlyPeriod(), parameters.Person))
                    .IgnoreArguments()
                    .Return(new List<DateOnlyPeriod> { new DateOnlyPeriod(dop.StartDate, dop.StartDate.AddDays(15)) })
                    .Repeat.AtLeastOnce();
            dic.Stub(x => x.Scenario).Return(scenario);
            dic.Stub(x => x.PermissionsEnabled).Return(false);

	        using (CurrentAuthorization.ThreadlyUse(authorization))
	        {
		        target.AddRange(new List<IPersonAssignment> {assInside, assOutSide});
		        target.Add(absInside);
		        target.Add(absOutSide);
		        target.ExtractAllScheduleData(extractor, new DateTimePeriod(2000, 1, 1, 2000, 12, 31));

		        Assert.IsTrue(target.Contains(assInside));
		        Assert.IsTrue(target.Contains(absInside));
		        Assert.IsFalse(target.Contains(assOutSide));
		        Assert.IsFalse(target.Contains(absOutSide));
		        Assert.AreEqual(2, extractor.PersonAbsences.Count);
		        Assert.AreEqual(2, extractor.PersonAssignments.Count);
	        }
        }

	    [Test]
	    public void VerifyExtractorOnlyReadsDataWithoutPermissionInsidePeriod()
	    {
		    extractor extractor = new extractor();
		    DateTime paramStart = parameters.Period.StartDateTime;
		    IPersonAssignment ass1 = createPersonAssignment(new DateTimePeriod(paramStart.AddDays(20), paramStart.AddDays(22)));
		    IPersonAssignment ass2 = createPersonAssignment(new DateTimePeriod(paramStart.AddDays(16), paramStart.AddDays(17)));

		    var dop = new DateOnlyPeriod(2000, 1, 1, 2000, 12, 31);
			authorization.Stub(x => x.PermittedPeriods(function, new DateOnlyPeriod(), parameters.Person))
					.IgnoreArguments()
					.Return(new List<DateOnlyPeriod> { new DateOnlyPeriod(dop.StartDate, dop.StartDate.AddDays(15)) })
					.Repeat.AtLeastOnce();
			dic.Stub(x => x.Scenario).Return(scenario);
			dic.Stub(x => x.PermissionsEnabled).Return(false);

		    using (CurrentAuthorization.ThreadlyUse(authorization))
		    {
			    target.AddRange(new List<IPersonAssignment> {ass1, ass2});
			    target.ExtractAllScheduleData(extractor,
				    new DateTimePeriod(paramStart.AddDays(18), paramStart.AddDays(22)));

			    var asses = extractor.PersonAssignments;
			    Assert.AreEqual(0, extractor.PersonAbsences.Count);
			    Assert.AreEqual(1, asses.Count);
			    Assert.AreEqual(new DateTimePeriod(paramStart.AddDays(20), paramStart.AddDays(22)),
				    asses.First().Period);
		    }
	    }

	    [Test]
	    public void VerifyAddingDataOutsidePeriodAreIgnored()
	    {
			IPersistableScheduleData inside = MockRepository.GenerateMock<IPersistableScheduleData>();
			IPersistableScheduleData outside = MockRepository.GenerateMock<IPersistableScheduleData>();

		    var dop = new DateOnlyPeriod(2000, 1, 1, 2000, 12, 31);

		    authorization.Stub(x => x.PermittedPeriods(function, new DateOnlyPeriod(), parameters.Person))
			    .IgnoreArguments()
			    .Return(new List<DateOnlyPeriod> {dop})
			    .Repeat.AtLeastOnce();

		    dic.Stub(x => x.PermissionsEnabled).Return(false);

		    inside.Stub(x => x.BelongsToScenario(scenario)).Return(true);
		    outside.Stub(x => x.BelongsToScenario(scenario)).Return(true);
		    inside.Stub(x => x.Person).Return(person);
		    outside.Stub(x => x.Person).Return(person);
		    inside.Stub(x => x.Period).Return(parameters.Period);
		    outside.Stub(x => x.Period).Return(new DateTimePeriod(parameters.Period.StartDateTime.AddMinutes(-2),parameters.Period.StartDateTime.AddMinutes(-1)));

			inside.Stub(x => x.BelongsToPeriod(new DateOnlyPeriod())).IgnoreArguments().Return(true); //check permission - should be ok
		    
		    using (CurrentAuthorization.ThreadlyUse(authorization))
		    {
			    target.Add(outside);
			    target.Add(inside);
		    }

		    Assert.IsTrue(target.Contains(inside));
		    Assert.IsFalse(target.Contains(outside));
	    }

	    [Test]
	    public void VerifySchedulePartGetsFullAccessSetToFalse()
	    {
		    var paramStartLocal = new DateOnly(parameters.Period.StartDateTimeLocal(TimeZoneInfoFactory.StockholmTimeZoneInfo()));

		    var dop = new DateOnlyPeriod(2000, 1, 1, 2000, 12, 31);
		    authorization.Stub(x => x.PermittedPeriods(function, new DateOnlyPeriod(), parameters.Person))
			    .IgnoreArguments()
			    .Return(new List<DateOnlyPeriod> {new DateOnlyPeriod(dop.StartDate, dop.StartDate.AddDays(15))})
			    .Repeat.AtLeastOnce();
		    dic.Stub(x => x.Scenario).Return(scenario);
		    authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
			    .Return(true);

		    using (CurrentAuthorization.ThreadlyUse(authorization))
		    {
			    IScheduleDay part = target.ScheduledDay(paramStartLocal.AddDays(20));
			    Assert.AreEqual(false, part.FullAccess);
		    }
	    }

	    private IPersonAbsence createPersonAbsence(DateTimePeriod period)
        {
            return
                new PersonAbsence(person, scenario,
                                  new AbsenceLayer(new Absence(), period));
        }

        private IPersonAssignment createPersonAssignment(DateTimePeriod period)
        {
            return PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
                                        scenario, ActivityFactory.CreateActivity("sdfsdf"), period, ShiftCategoryFactory.CreateShiftCategory("sdf"));
        }

        private class extractor : IScheduleExtractor
        {
			private readonly object ExtractorLock = new object();

            public extractor()
            {
                PersonAssignments = new HashSet<IPersonAssignment>();
                PersonAbsences = new HashSet<IPersonAbsence>();
            }           

            public ICollection<IPersonAssignment> PersonAssignments { get; private set; }
            public ICollection<IPersonAbsence> PersonAbsences { get; private set; }


            public void AddSchedulePart(IScheduleDay schedulePart)
            {
	            lock (ExtractorLock)
	            {
		            var ass = schedulePart.PersonAssignment();
		            if (ass != null)
		            {
			            PersonAssignments.Add(ass);
		            }
		            schedulePart.PersonAbsenceCollection().ForEach(PersonAbsences.Add);
	            }
            }
        }
    }
}
