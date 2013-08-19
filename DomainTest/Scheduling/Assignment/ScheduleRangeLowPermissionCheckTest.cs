using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    //tests when permissions are disabled on scheduledictionary
    [TestFixture]
    public class ScheduleRangeLowPermissionCheckTest
    {
        private string function;
        private MockRepository mocks;
        private IScheduleParameters parameters;
        private IPerson person;
        private IScenario scenario;
        private ScheduleRange target;
        private IScheduleDictionary dic;
        private IPrincipalAuthorization principalAuthorization;

        [SetUp]
        public void Setup()
        {
            person = PersonFactory.CreatePerson();
            scenario = new Scenario("sdf");
            mocks = new MockRepository();
            principalAuthorization = mocks.StrictMock<IPrincipalAuthorization>();
            function = DefinedRaptorApplicationFunctionPaths.ViewSchedules;
            parameters =
                new ScheduleParameters(scenario, person, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            dic = mocks.StrictMock<IScheduleDictionary>();
            target = new ScheduleRange(dic, parameters);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
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

            using (mocks.Record())
            {
                var dop = new DateOnlyPeriod(2000, 1, 1, 2000, 12, 31);
                Expect.Call(principalAuthorization.PermittedPeriods(function, new DateOnlyPeriod(), parameters.Person))
                        .IgnoreArguments()
                        .Return(new List<DateOnlyPeriod> { new DateOnlyPeriod(dop.StartDate, dop.StartDate.AddDays(15)) })
                        .Repeat.AtLeastOnce();
                Expect.Call(dic.Scenario)
                        .Return(scenario)
                        .Repeat.Any();
                Expect.Call(dic.PermissionsEnabled).Return(false).Repeat.Any();
            }
            using (mocks.Playback())
            {
                using (new CustomAuthorizationContext(principalAuthorization))
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
        }

        [Test]
        public void VerifyExtractorOnlyReadsDataWithoutPermissionInsidePeriod()
        {
            extractor extractor = new extractor();
            DateTime paramStart = parameters.Period.StartDateTime;
            IPersonAssignment ass1 = createPersonAssignment(new DateTimePeriod(paramStart.AddDays(20), paramStart.AddDays(22)));
            IPersonAssignment ass2 = createPersonAssignment(new DateTimePeriod(paramStart.AddDays(16), paramStart.AddDays(17)));

            using (mocks.Record())
            {
                var dop = new DateOnlyPeriod(2000, 1, 1, 2000, 12, 31);
                Expect.Call(principalAuthorization.PermittedPeriods( function, new DateOnlyPeriod(), parameters.Person))
                        .IgnoreArguments()
                        .Return(new List<DateOnlyPeriod> { new DateOnlyPeriod(dop.StartDate, dop.StartDate.AddDays(15)) })
                        .Repeat.AtLeastOnce();
                Expect.Call(dic.Scenario)
                        .Return(scenario)
                        .Repeat.Any();
                Expect.Call(dic.PermissionsEnabled).Return(false).Repeat.Any();
            }
            using (mocks.Playback())
            {
                using (new CustomAuthorizationContext(principalAuthorization))
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
        }

        [Test]
        public void VerifyAddingDataOutsidePeriodAreIgnored()
        {
            IPersistableScheduleData inside = mocks.StrictMock<IPersistableScheduleData>();
            IPersistableScheduleData outside = mocks.StrictMock<IPersistableScheduleData>();
            using (mocks.Record())
            {
                var dop = new DateOnlyPeriod(2000, 1, 1, 2000, 12, 31);
                Expect.Call(principalAuthorization.PermittedPeriods(function, new DateOnlyPeriod(), parameters.Person))
                        .IgnoreArguments()
                        .Return(new List<DateOnlyPeriod> { dop })
                        .Repeat.AtLeastOnce();


                Expect.Call(inside.BelongsToScenario(scenario)).Return(true);
                Expect.Call(outside.BelongsToScenario(scenario)).Return(true);
                Expect.Call(inside.Person).Return(person);
                Expect.Call(outside.Person).Return(person);
                Expect.Call(inside.Period).Return(parameters.Period);
                Expect.Call(outside.Period).Return(new DateTimePeriod(parameters.Period.StartDateTime.AddMinutes(-2), parameters.Period.StartDateTime.AddMinutes(-1)));

                Expect.Call(inside.BelongsToPeriod(new DateOnlyPeriod()))
                    .IgnoreArguments() //now a class
                    .Return(true); //check permission - should be ok
                Expect.Call(dic.PermissionsEnabled).Return(false).Repeat.Any();
            }

            using(mocks.Playback())
            {
                using (new CustomAuthorizationContext(principalAuthorization))
                {
                    target.Add(outside);
                    target.Add(inside);
                }
            }
            Assert.IsTrue(target.Contains(inside));
            Assert.IsFalse(target.Contains(outside));
        }

        [Test]
        public void VerifySchedulePartGetsFullAccessSetToFalse()
        {
            DateOnly paramStartLocal = new DateOnly(parameters.Period.LocalStartDateTime);

            using (mocks.Record())
            {
                var dop = new DateOnlyPeriod(2000, 1, 1, 2000, 12, 31);
                Expect.Call(principalAuthorization.PermittedPeriods(function, new DateOnlyPeriod(), parameters.Person))
                        .IgnoreArguments()
                        .Return(new List<DateOnlyPeriod> { new DateOnlyPeriod(dop.StartDate, dop.StartDate.AddDays(15)) })
                        .Repeat.AtLeastOnce();
                Expect.Call(dic.Scenario).Return(scenario).Repeat.Any();
                Expect.Call(principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)).Return(true);
            }
            using (mocks.Playback())
            {
                using (new CustomAuthorizationContext(principalAuthorization))
                {
                    IScheduleDay part = target.ScheduledDay(paramStartLocal.AddDays(20));
                    Assert.AreEqual(false, part.FullAccess);
                }
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
            return PersonAssignmentFactory.CreateAssignmentWithMainShift(
                                        ActivityFactory.CreateActivity("sdfsdf"),
                                        person,
                                        period,
                                        ShiftCategoryFactory.CreateShiftCategory("sdf"),
                                        scenario);
        }

        private PersonDayOff createPersonDayOff(DateTime period)
        {
            DayOffTemplate dOff = new DayOffTemplate(new Description("test"));
            dOff.Anchor = TimeSpan.FromHours(3);
            dOff.SetTargetAndFlexibility(TimeSpan.FromHours(35), TimeSpan.FromHours(1));
            return new PersonDayOff(person, scenario, dOff, new DateOnly(period.Date));
        }


        private class extractor : IScheduleExtractor
        {
            public extractor()
            {
                PersonAssignments = new HashSet<IPersonAssignment>();
                PersonAbsences = new HashSet<IPersonAbsence>();
            }           

            public ICollection<IPersonAssignment> PersonAssignments { get; private set; }
            public ICollection<IPersonAbsence> PersonAbsences { get; private set; }


            public void AddSchedulePart(IScheduleDay schedulePart)
            {
                schedulePart.PersonAssignmentCollectionDoNotUse().ForEach(PersonAssignments.Add);
                schedulePart.PersonAbsenceCollection().ForEach(PersonAbsences.Add);
            }
        }
    }
}
