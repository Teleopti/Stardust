using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{

	[TestFixture]
	public class ScheduleRangeTest
	{
		private string _function;
		private MockRepository _mocks;
		private IScheduleParameters _parameters;
		private IPerson _person;
		private IScenario _scenario;
		private scheduleExposingInternalCollections _target;
		private IScheduleDictionary _dic;
	    private IPrincipalAuthorization _principalAuthorization;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dic = _mocks.StrictMock<IScheduleDictionary>();
		    _principalAuthorization = _mocks.StrictMock<IPrincipalAuthorization>();
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_person = PersonFactory.CreatePerson();
			_function = DefinedRaptorApplicationFunctionPaths.ViewSchedules;
			_parameters = new ScheduleParameters(_scenario,
								_person, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			_target = new scheduleExposingInternalCollections(_dic, _parameters);
		}

		[Test, Ignore("This is no longer valid - maybe it will be soon. Remove if still ignored on main")]
		public void VerifyExtractAllDataRegardingTimeZones()
		{
			_target = new scheduleExposingInternalCollections(_dic, new ScheduleParameters(_scenario, _person, new DateTimePeriod(1800,1,1,2200,1,1)));
			var extractor = new Extractor();
			var periodToExtract = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);

			var early = createPersonAssignment(new DateTimePeriod(new DateTime(1999, 12, 31, 23,0,0,DateTimeKind.Utc), new DateTime(2000, 1, 1).ToUniversalTime()));
			var late = createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			using(_mocks.Record())
			{
				fullPermission(false);
			}
			using (_mocks.Playback())
			{
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(early);
                    _target.Add(late);

                    _person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
                    _target.ExtractAllScheduleData(extractor, periodToExtract);
                    CollectionAssert.IsEmpty(extractor.ScheduleDataCollection);

                    extractor.ScheduleDataCollection.Clear();
                    _person.PermissionInformation.SetDefaultTimeZone(
                        (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))); //+1
                    _target.ExtractAllScheduleData(extractor, periodToExtract);
                    Assert.AreEqual(2, extractor.ScheduleDataCollection.Count(),
                                    "Yes it looks a bit strange, but it _should_ be 2 not 1");
                    CollectionAssert.Contains(extractor.ScheduleDataCollection, early);

                    extractor.ScheduleDataCollection.Clear();
                    _person.PermissionInformation.SetDefaultTimeZone(
                        (TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"))); //-6h
                    _target.ExtractAllScheduleData(extractor, periodToExtract);
                    Assert.AreEqual(2, extractor.ScheduleDataCollection.Count(),
                                    "Yes it looks a bit strange, but it _should_ be 2 not 1");
                    CollectionAssert.Contains(extractor.ScheduleDataCollection, late);
                }
			}
		}

		private class Extractor : IScheduleExtractor
		{
			public Extractor()
			{
				ScheduleDataCollection = new List<IScheduleData>();
			}

			public void AddSchedulePart(IScheduleDay schedulePart)
			{
				schedulePart.PersistableScheduleDataCollection().ForEach(ScheduleDataCollection.Add);
			}

			public ICollection<IScheduleData> ScheduleDataCollection { get; set; }
		}

		[Test]
		public void CannotSeeUnpublishedSchemaWithoutUnpublishedPermission()
		{
			using(_mocks.Record())
			{
				fullPermission(false);
			}
			using(_mocks.Playback())
			{
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
                    var part = _target.ScheduledDay(new DateOnly(2000, 1, 2));
	                part.PersonAssignment().Should().Be.Null();
                    Assert.IsFalse(part.IsFullyPublished);
                }
			}
		}

		[Test]
		public void VerifyReFetch()
		{
			IScheduleDay partWithHit = _mocks.StrictMock<IScheduleDay>();
			IScheduleDay partWithNoHit = _mocks.StrictMock<IScheduleDay>();
			var hit = new DateOnlyAsDateTimePeriod(new DateOnly(2000, 1, 2), (TimeZoneInfo.Utc));
			var noHit = new DateOnlyAsDateTimePeriod(new DateOnly(2000, 1, 1), (TimeZoneInfo.Utc));
			using(_mocks.Record())
			{
				fullPermission(true);
				Expect.Call(partWithHit.DateOnlyAsPeriod).Return(hit);
				Expect.Call(partWithNoHit.DateOnlyAsPeriod).Return(noHit);
			}
			using(_mocks.Playback())
			{
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
                    IScheduleDay yes = _target.ReFetch(partWithHit);
                    IScheduleDay no = _target.ReFetch(partWithNoHit);
	                yes.PersonAssignment().Should().Not.Be.Null();
                    no.PersonAssignment().Should().Be.Null();
                }
			}
		}

		[Test]
		public void VerifyUnsafeUpdateSetsCurrentVersionNumber()
		{
            using(_mocks.Record())
            {
                fullPermission(true);
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    var data = (PersonAssignment) createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3));
                    ((IEntity) data).SetId(Guid.NewGuid());
                    data.SetVersion(56);
                    var snapshotData = data.EntityClone();
                    _target.Add(data);
                    _target.Snapshot.Add(snapshotData);

                    var clientData = (PersonAssignment) data.EntityClone();
                    clientData.SetVersion(63);
                    _target.SolveConflictBecauseOfExternalUpdate(clientData, false);

                    Assert.AreEqual(63, _target.PersonAssignments.First().Version);
                }
            }
		}

		[Test]
		public void UnsafeUpdateDoNotSetCurrentVersionNumberIfNotPresent()
		{
            using(_mocks.Record())
            {
                fullPermission(true);
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    var clientData =
                        (PersonAssignment) createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3));
                    clientData.SetVersion(63);
                    _target.SolveConflictBecauseOfExternalUpdate(clientData, false);

                    Assert.AreEqual(0, _target.PersonAssignments.Count);
                }
            }
		}

		[Test, Ignore("Micke ska prata med Roger")]
		public void VerifyCalculatedContractTimeHolderAndCalculatedScheduleDaysOff()
		{
			_target.CalculatedContractTimeHolder = TimeSpan.FromMinutes(24);
			Assert.AreEqual(TimeSpan.FromMinutes(24), _target.CalculatedContractTimeHolder);
			IPersistableScheduleData data1 = createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			using (_mocks.Record())
			{
				fullPermission(false);
                Expect.Call(_principalAuthorization.IsPermitted("", DateOnly.Today, _person)).IgnoreArguments().Return(false).Repeat.Twice();
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(data1);
                    var part = _target.ScheduledDay(new DateOnly(2000, 1, 2));
                    _target.ModifyInternal(part);
                    Assert.IsFalse(_target.CalculatedContractTimeHolder.HasValue);
                    Assert.IsFalse(_target.CalculatedScheduleDaysOff.HasValue);
                }
            }
		}

		[Test]
		public void VerifyCalculatedTargetTimeHolder()
		{
			_target.CalculatedTargetTimeHolder = TimeSpan.FromMinutes(24);
			Assert.AreEqual(TimeSpan.FromMinutes(24), _target.CalculatedTargetTimeHolder);
			IPersistableScheduleData data1 = createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			using (_mocks.Record())
			{
				fullPermission(true);
                Expect.Call(_principalAuthorization.IsPermitted("",DateOnly.Today,_person)).IgnoreArguments().Return(true).Repeat.Twice();
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(data1);
                    var part = _target.ScheduledDay(new DateOnly(2000, 1, 2));
                    _target.ModifyInternal(part);
                    Assert.IsFalse(_target.CalculatedTargetTimeHolder.HasValue);
                }
            }
		}

		[Test]
		public void VerifyCalculatedTargetScheduleDaysOffHolder()
		{
			_target.CalculatedTargetScheduleDaysOff = 7;
			Assert.AreEqual(7, _target.CalculatedTargetScheduleDaysOff);
			IPersistableScheduleData data1 = createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			using (_mocks.Record())
			{
				fullPermission(true);
                Expect.Call(_principalAuthorization.IsPermitted("", DateOnly.Today, _person)).IgnoreArguments().Return(true).Repeat.Twice();
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(data1);
                    var part = _target.ScheduledDay(new DateOnly(2000, 1, 2));
                    _target.ModifyInternal(part);
                    Assert.IsFalse(_target.CalculatedTargetScheduleDaysOff.HasValue);
                }
            }
		}

		[Test]
		public void CanSeeUnpublishedSchemaWithUnpublishedPermission()
		{
			using (_mocks.Record())
			{
				fullPermission(true);
			}
			using (_mocks.Playback())
			{
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
                    var part = _target.ScheduledDay(new DateOnly(2000, 1, 2));
	                part.PersonAssignment().Should().Not.Be.Null();
                    Assert.IsFalse(part.IsFullyPublished);
                }
			}
		}

		[Test]
		public void CanSeePublishedDataWithNoUnpublishedPermission()
		{
			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2100, 1, 1); 
			
			_person.WorkflowControlSet = workflowControlSet;
			_target = new scheduleExposingInternalCollections(_dic, _parameters);

			using (_mocks.Record())
			{
				fullPermission(false);
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
                    var part = _target.ScheduledDay(new DateOnly(2000, 1, 2));
	                part.PersonAssignment().Should().Not.Be.Null();
                    Assert.IsTrue(part.IsFullyPublished);
                }
            }
		}

		private void fullPermission(bool canViewUnPublishedData)
		{
			var permPeriod = new List<DateOnlyPeriod> { new DateOnlyPeriod(1900, 1, 1, 2100, 1, 1) };
			Expect.Call(_principalAuthorization.PermittedPeriods(_function, new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1), _person)).IgnoreArguments().Return(permPeriod).Repeat.Any();
			Expect.Call(_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)).Return(canViewUnPublishedData).Repeat.Any();
			Expect.Call(_dic.Scenario).Return(_scenario).Repeat.Any();
		}

		[Test]
		public void VerifyGetParameters()
		{
			Assert.AreEqual(_parameters.Period, _target.Period);
			Assert.AreSame(_parameters.Person, _target.Person);
			Assert.AreSame(_parameters.Scenario, _target.Scenario);
		}

		[Test]
		public void VerifyTimeZonePositive()
		{
            using(_mocks.Record())
            {
                fullPermission(true);
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    DateOnly dateOnly = new DateOnly(2000, 1, 2);
                    _target.Add(createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));

                    _target.ScheduledDay(dateOnly.AddDays(-1)).PersonAssignment().Should().Be.Null();
										_target.ScheduledDay(dateOnly).PersonAssignment().Should().Not.Be.Null();
										_target.ScheduledDay(dateOnly.AddDays(1)).PersonAssignment().Should().Be.Null();
                }
            }
		}

		[Test]
		public void VerifyTimeZoneNegative()
		{
            using(_mocks.Record())
            {
                fullPermission(true);
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
									_person.PermissionInformation.SetDefaultTimeZone(
											(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")));
									
									DateOnly dateOnly = new DateOnly(2000, 1, 2);
                    _target.Add(createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));


                    _target.ScheduledDay(dateOnly.AddDays(-1)).PersonAssignment().Should().Not.Be.Null();
                    _target.ScheduledDay(dateOnly).PersonAssignment().Should().Be.Null();
										_target.ScheduledDay(dateOnly.AddDays(1)).PersonAssignment().Should().Be.Null();
                }
            }
		}


		[Test]
		public void VerifyCanCreateScheduleDay()
		{
            using (_mocks.Record())
            {
                fullPermission(true);
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    IPersonAssignment ass1 = createPersonAssignment(new DateTimePeriod(2000, 1, 2, 2000, 1, 3));
                    IPersonAbsence abs1 = createPersonAbsence(new DateTimePeriod(2000, 1, 1, 2000, 1, 4));
                    IPersonAbsence abs2 = createPersonAbsence(new DateTimePeriod(2000, 1, 2, 2000, 1, 4));
			IPersonMeeting personMeeting = createPersonMeeting(new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

                    _target.Add(ass1);
                    _target.Add(abs1);
                    _target.Add(abs2);
                    _target.Add(personMeeting);

                    IScheduleDay day1 = _target.ScheduledDay(new DateOnly(2000, 1, 2));
                    IScheduleDay day2 = _target.ScheduledDay(new DateOnly(2000, 1, 3));

										Assert.AreEqual(ass1.MainActivities().First().Period,
																		day1.PersonAssignment().MainActivities().First().Period);
                    Assert.AreEqual(2, day1.PersonAbsenceCollection().Count);
                    Assert.AreEqual(2, day2.PersonAbsenceCollection().Count);
                    Assert.AreEqual(personMeeting.Period, day1.PersonMeetingCollection()[0].Period);
                    Assert.IsInstanceOf<ScheduleProjectionService>(day1.ProjectionService());

                    Assert.IsTrue(day1.FullAccess);
                    Assert.IsTrue(day2.FullAccess);
                    Assert.AreEqual(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), day1.Period);
                }
            }
		}

		[Test]
		public void CanExtractRestrictions()
		{
            using(_mocks.Record())
            {
                fullPermission(true);
            }
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    var restriction1 = new AvailabilityRestriction();
                    var restriction2 = new RotationRestriction();
	                restriction2.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("sk");
	                var scheduleDataRestriction1 = new ScheduleDataRestriction(_person, restriction1,
	                                                                          new DateOnly(2000, 1, 1));
	                var scheduleDataRestriction2 = new ScheduleDataRestriction(_person, restriction2,
	                                                                           new DateOnly(2000, 1, 1));
                    _target.Add(scheduleDataRestriction1);
                    _target.Add(scheduleDataRestriction2);

                    var coll = _target.ScheduledDay(new DateOnly(2000, 1, 1)).RestrictionCollection();
                    Assert.AreEqual(2, coll.Count());
	                Assert.AreEqual(new DateOnly(2000, 1, 1), scheduleDataRestriction1.RestrictionDate);
	                Assert.AreEqual(new DateOnly(2000, 1, 1), scheduleDataRestriction2.RestrictionDate);
	                Assert.IsTrue(scheduleDataRestriction1.IsAvailabilityRestriction);
	                Assert.IsFalse(scheduleDataRestriction1.IsPreferenceRestriction);
	                Assert.IsFalse(scheduleDataRestriction1.IsRotationRestriction);
					Assert.IsFalse(scheduleDataRestriction2.IsAvailabilityRestriction);
	                Assert.IsFalse(scheduleDataRestriction2.IsPreferenceRestriction);
	                Assert.IsTrue(scheduleDataRestriction2.IsRotationRestriction);
                    CollectionAssert.Contains(coll, restriction1);
                    CollectionAssert.Contains(coll, restriction2);
                }
            }
		}

		[Test]
		public void CanExtractMultipleRestrictionsFromRestrictionOwner()
		{
			using (_mocks.Record())
			{
				fullPermission(true);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					var studenrestColl = new List<IStudentAvailabilityRestriction>
					                     	{
					                     		new StudentAvailabilityRestriction(),
					                     		new StudentAvailabilityRestriction()
					                     	};
					var prefRest = new PreferenceRestriction();
					var rest1 = new StudentAvailabilityDay(_person, new DateOnly(2000, 1, 1), studenrestColl);
					var rest2 = new PreferenceDay(_person, new DateOnly(2000, 1, 1), prefRest);
					_target.Add(rest1);
					_target.Add(rest2);

					var coll = _target.ScheduledDay(new DateOnly(2000, 1, 1)).RestrictionCollection();
					Assert.AreEqual(3, coll.Count());
				}
			}
		}

		[Test]
		public void ShouldRemoveOtherPreferenceForSameDayIfItIsNotSavedYet()
		{
			using (_mocks.Record())
			{
				fullPermission(true);
			}

			using (_mocks.Playback())
			{
				using (new CustomAuthorizationContext(_principalAuthorization))
				{
					var preferenceDay1 = new PreferenceDay(_person, new DateOnly(2000, 1, 1), new PreferenceRestriction());
                    preferenceDay1.SetId(Guid.NewGuid());
                    _target.Add(preferenceDay1);

					var preferenceDay2 = new PreferenceDay(_person, new DateOnly(2000, 1, 1), new PreferenceRestriction());
					_target.Add(preferenceDay2);
                    _target.Add(preferenceDay1);

					var coll = _target.ScheduledDay(new DateOnly(2000, 1, 1)).RestrictionCollection();

					Assert.AreEqual(1, coll.Count());
				}
			}
		}

		[Test]
		public void CanExtractSchedulePartWhenOvertimeUsesIncorrectDefinitionSet()
		{
			fullPermission(true);
			_mocks.ReplayAll();
            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                IMultiplicatorDefinitionSet defSet = new MultiplicatorDefinitionSet("def", MultiplicatorType.Overtime);
                PersonFactory.AddDefinitionSetToPerson(_person, defSet);
								IPersonAssignment ass = new PersonAssignment(_person, _scenario, new DateOnly(2000, 1, 1));
                ass.AddOvertimeActivity(ActivityFactory.CreateActivity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), defSet);
                _target.Add(ass);
                _person.RemoveAllPersonPeriods();
                Assert.AreEqual(1,
                       _target.ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment().OvertimeActivities().Count());
            }
			_mocks.VerifyAll();
		}


		[Test]
		public void VerifyDefaultConstructor()
		{
			ScheduleRange targetTemp = new ScheduleRange(_dic, _parameters);
			Assert.AreEqual(_parameters.Period, targetTemp.Period);
			Assert.AreSame(_parameters.Person, targetTemp.Person);
			Assert.AreSame(_parameters.Scenario, targetTemp.Scenario);
		}

		[Test]
		public void VerifyPersonAbsenceDateTimeSortOrder()
		{
			Expect.Call(_dic.PermissionsEnabled).Return(true);
			fullPermission(true);
			_mocks.ReplayAll();

            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                IPersonAbsence absOk1 = createPersonAbsence(createPeriod(2, 20));
                IPersonAbsence absOk2 = createPersonAbsence(createPeriod(5, 20));
                IPersonAbsence absOk3 = createPersonAbsence(createPeriod(3, 4));
                IPersonAbsence absOk4 = createPersonAbsence(createPeriod(4, 5));
                absOk4.Layer.Payload.Priority = 200;
                _target.Add(createPersonAbsence(createPeriod(-4, -2)));
                _target.Add(createPersonAbsence(createPeriod(26, 30)));
                _target.Add(absOk1);
                _target.Add(absOk2);
                _target.Add(absOk3);
                _target.Add(absOk4);

                var res = _target.ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection();

                Assert.AreEqual(4, res.Count());
                Assert.AreEqual(absOk4.Layer.Period, res.First().Layer.Period);
            }
			_mocks.VerifyAll();
		}

		private static DateTimePeriod createPeriod(int startHour, int endHour)
		{
			var start = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(startHour);
			var end = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(endHour);
			return new DateTimePeriod(start, end);
		}


		[Test]
		public void VerifyProperties()
		{
			ScheduleRange targetTemp = new ScheduleRange(_dic, _parameters);
			
                    Assert.AreEqual(_parameters.Period, targetTemp.Period);
                    Assert.AreSame(_parameters.Person, targetTemp.Person);
                    Assert.AreSame(_parameters.Scenario, targetTemp.Scenario);
		}

	    [Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void VerifyScheduleParametersIsNotNull()
		{
			new ScheduleRange(_dic, null);
		}

		[Test]
		public void VerifyTotalPeriodReturnsNullWhenEmpty()
		{  
			Assert.IsNull(_target.TotalPeriod());
		}

		[Test]
		public void VerifyTotalPeriodSelectsEarliestIScheduleData()
		{
			fullPermission(true);
			Expect.Call(_dic.PermissionsEnabled).Return(true).Repeat.Any();
			_mocks.ReplayAll();

            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                DateTimePeriod earlyPeriod = new DateTimePeriod(2001, 1, 3, 2001, 1, 12);
                DateTimePeriod latePeriod = new DateTimePeriod(2001, 1, 14, 2001, 1, 19);
                DateTimePeriod reallyLongPeriod = new DateTimePeriod(2001, 1, 1, 2001, 1, 25);

                _target.Add(createPersonAbsence(earlyPeriod));
                _target.Add(createPersonAbsence(latePeriod));

                DateTimePeriod longestPeriod = _target.TotalPeriod().Value;

                Assert.AreEqual(earlyPeriod.StartDateTime, longestPeriod.StartDateTime);
                Assert.AreEqual(latePeriod.EndDateTime, longestPeriod.EndDateTime);

                _target.Add(createPersonAbsence(reallyLongPeriod));
                longestPeriod = _target.TotalPeriod().Value;

                Assert.AreEqual(reallyLongPeriod.StartDateTime, longestPeriod.StartDateTime);
                Assert.AreEqual(reallyLongPeriod.EndDateTime, longestPeriod.EndDateTime);
            }
			_mocks.VerifyAll();
		}

		[Test]
		public void VerifyShiftCategoryFairness()
		{
			DateTimePeriod dtp = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			IScheduleDateTimePeriod sdtp = new ScheduleDateTimePeriod(dtp);
			IScheduleDictionary dic1 = new ScheduleDictionary(_scenario, sdtp);
			IShiftCategory low = ShiftCategoryFactory.CreateShiftCategory("Low");
			setShiftCategoryJusticeValues(low, 1);
			IShiftCategory high = ShiftCategoryFactory.CreateShiftCategory("High");
			setShiftCategoryJusticeValues(high, 5);

			var ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Activity("d"), _person,
			                                                                 new DateTimePeriod(2000, 1, 2, 2000, 1, 3), low,
			                                                                 _scenario);

			((ScheduleRange)dic1[_person]).Add(ass1);

			Assert.IsNotNull(dic1[_person].CachedShiftCategoryFairness());
		}

		[Test]
		public void VerifyFairnessPoints()
		{
			DateTimePeriod dtp = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
													new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			IScheduleDateTimePeriod sdtp = new ScheduleDateTimePeriod(dtp);
			IScheduleDictionary dic1 = new ScheduleDictionary(_scenario, sdtp);
			IShiftCategory low = ShiftCategoryFactory.CreateShiftCategory("Low");
			setShiftCategoryJusticeValues(low, 1);
			IShiftCategory high = ShiftCategoryFactory.CreateShiftCategory("High");
			setShiftCategoryJusticeValues(high, 5);
			IPersonAssignment ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
																						   new DateTimePeriod(2000, 1, 2, 2000, 1, 3), low);           
			IPersonAssignment ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person,
																						   new DateTimePeriod(2000, 1, 5, 2000, 1, 6), high);
			IPersonAbsence abs = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario,
																		  new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			((ScheduleRange)dic1[_person]).Add(ass1);
			Assert.AreEqual(1, dic1[_person].FairnessPoints().FairnessPoints);

			((ScheduleRange)dic1[_person]).Add(ass2);
			Assert.AreEqual(6, dic1[_person].FairnessPoints().FairnessPoints);

			((ScheduleRange)dic1[_person]).Add(abs);
			Assert.AreEqual(6, dic1[_person].FairnessPoints().FairnessPoints);
		}

		[Test]
		public void VerifyUnsafeDeleteIfDeletedIncludeCurrent([Values(true, false)] bool includeCurrent)
		{
			fullPermission(true);
			_mocks.ReplayAll();
            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                var id = Guid.NewGuid();
                var ass = createPersonAssignment(new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
                ass.SetId(id);

                var internalDic = new Dictionary<IPerson, IScheduleRange>();
                var dictionary = new ScheduleDictionaryForTest(_scenario,
                                                               new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1,
                                                                                                             2100, 1, 1)),
                                                               internalDic);
                var range = new scheduleExposingInternalCollections(dictionary,
                                                                    new ScheduleParameters(_scenario, ass.Person,
                                                                                           new DateTimePeriod(2000, 1, 1,
                                                                                                              2000, 11,
                                                                                                              5)));
                internalDic[ass.Person] = range;

                range.Add(ass);
                range.Owner.TakeSnapshot();
                range.Remove(ass);

                Assert.AreEqual(ass, range.SolveConflictBecauseOfExternalDeletion(id, includeCurrent));
                Assert.AreEqual(0, range.PersonAssignments.Count);
                Assert.AreEqual(0, range.Owner.DifferenceSinceSnapshot().Count());
            }
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldRemoveSnapshotsSnapshotWhenTakingNewSnapshot()
        {
            var snapshotBefore = _target.Snapshot;

            _target.TakeSnapshot();

            _target.Snapshot.Snapshot.Should().Not.Be.SameInstanceAs(snapshotBefore);
        }

		[Test]
		public void ShouldReset_CalculatedContractTimeAndDaysOff()
		{
			_target.CalculatedContractTimeHolder = new TimeSpan(10,10,10,10);
			_target.CalculatedScheduleDaysOff = 8;

			_target.ForceRecalculationOfContractTimeAndDaysOff();

			_target.CalculatedContractTimeHolder.Should().Be.EqualTo(null);
			_target.CalculatedScheduleDaysOff.Should().Be.EqualTo(null);
		}

		private static void setShiftCategoryJusticeValues(IShiftCategory shiftCategory, int value)
		{
			shiftCategory.DayOfWeekJusticeValues[DayOfWeek.Monday] = value;
			shiftCategory.DayOfWeekJusticeValues[DayOfWeek.Tuesday] = value;
			shiftCategory.DayOfWeekJusticeValues[DayOfWeek.Wednesday] = value;
			shiftCategory.DayOfWeekJusticeValues[DayOfWeek.Thursday] = value;
			shiftCategory.DayOfWeekJusticeValues[DayOfWeek.Friday] = value;
			shiftCategory.DayOfWeekJusticeValues[DayOfWeek.Saturday] = value;
			shiftCategory.DayOfWeekJusticeValues[DayOfWeek.Sunday] = value;
		}

		private IPersonAbsence createPersonAbsence(DateTimePeriod period)
		{
			return PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, period);
		}

		private IPersonAssignment createPersonAssignment(DateTimePeriod period)
		{
			var ret = PersonAssignmentFactory.CreateAssignmentWithMainShift(
										ActivityFactory.CreateActivity("sdfsdf"),
										_person,
										period,
										ShiftCategoryFactory.CreateShiftCategory("sdf"),
										_scenario);
			ret.SetId(Guid.NewGuid());
			return ret;
		}

		private IPersonMeeting createPersonMeeting(DateTimePeriod period)
		{
			MeetingPerson meetingPerson = new MeetingPerson(_person, true);

			IMeeting mainMeeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description",
					ActivityFactory.CreateActivity("activity"), _scenario);

			mainMeeting.AddMeetingPerson(meetingPerson);

			return new PersonMeeting(mainMeeting, meetingPerson, period);

		}

		private class scheduleExposingInternalCollections : ScheduleRange
		{

			public scheduleExposingInternalCollections(IScheduleDictionary owner, IScheduleParameters parameters)
				: base(owner, parameters)
			{
			}

			public IList<IPersonAbsence> PersonAbsences { get { return new List<IPersonAbsence>(ScheduleDataInternalCollection().OfType<IPersonAbsence>()); } }

			public IList<IPersonAssignment> PersonAssignments { get { return new List<IPersonAssignment>(ScheduleDataInternalCollection().OfType<IPersonAssignment>()); } }

			public IList<IPersonMeeting> PersonMeetings { get { return new List<IPersonMeeting>(ScheduleDataInternalCollection().OfType<IPersonMeeting>()); } }
		}


		#region Add/Remove
		[Test]
		public void CanAddAndRemovePersonAssignment()
		{
			fullPermission(true);
			_mocks.ReplayAll();
            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                IPersonAssignment pAss = createPersonAssignment(new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
                _target.Add(pAss);
                Assert.AreEqual(1, _target.PersonAssignments.Count);
                Assert.IsTrue(_target.PersonAssignments.Contains(pAss));

                _target.Remove(pAss);
                Assert.AreEqual(0, _target.PersonAssignments.Count);
            }
		}

		[Test]
		public void CanAddAndRemovePersonAbsence()
		{
			fullPermission(true);
			_mocks.ReplayAll();
            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                IPersonAbsence pAbs = createPersonAbsence(new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
                _target.Add(pAbs);
                Assert.AreEqual(1, _target.PersonAbsences.Count);
                Assert.IsTrue(_target.PersonAbsences.Contains(pAbs));

                _target.Remove(pAbs);
                Assert.AreEqual(0, _target.PersonAbsences.Count);
            }
		}

		[Test]
		public void CanAddRemoveMeeting()
		{
			fullPermission(true);
			_mocks.ReplayAll();
            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                IPersonMeeting meeting = createPersonMeeting(new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

                meeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(_target.Person, true));

                _target.Add(meeting);
                ICollection<IPersonMeeting> coll = _target.PersonMeetings;
                Assert.AreEqual(1, coll.Count);
                Assert.IsTrue(coll.Contains(meeting));

                _target.Remove(meeting);
                Assert.AreEqual(0, _target.PersonMeetings.Count);
            }
		}

		#endregion

		#region Add checks
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void VerifyCannotAddNullToScheduleDataPersonTimeActivity()
		{
			_target.Add(null);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void VerifyCorrectPerson()
		{
			IPersonAbsence pAbs = new PersonAbsence(PersonFactory.CreatePerson(),
												   _scenario,
												   new AbsenceLayer(AbsenceFactory.CreateAbsence("abs"), new DateTimePeriod(2000, 2, 1, 2000, 3, 2)));
			_target.Add(pAbs);
		}
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void VerifyCorrectScenario()
		{
			PersonAbsence pAbs = new PersonAbsence(_person,
												   ScenarioFactory.CreateScenarioAggregate(),
												   new AbsenceLayer(AbsenceFactory.CreateAbsence("abs"), new DateTimePeriod(2000, 2, 1, 2000, 3, 2)));
			_target.Add(pAbs);
		}

		[Test]
		[ExpectedException(typeof(PermissionException))]
		public void VerifyCannotAddIfNoPermission()
		{
			_target = new scheduleExposingInternalCollections(_dic, _parameters);
			using(_mocks.Record())
			{
				Expect.Call(_principalAuthorization.PermittedPeriods(_function, new DateOnlyPeriod(2001, 1, 1, 2002, 1, 1), _person))
					.IgnoreArguments()
					.Return(new List<DateOnlyPeriod>());
				Expect.Call(_dic.PermissionsEnabled).Return(true);
			}
			using(_mocks.Playback())
			{
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario,
                                                                         new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
                }
			}
		}


		[Test]
		[ExpectedException(typeof(PermissionException))]
		public void VerifyCannotAddIfOutsidePermission()
		{
			_target = new scheduleExposingInternalCollections(_dic, _parameters);
			using (_mocks.Record())
			{
				Expect.Call(_principalAuthorization.PermittedPeriods(_function, new DateOnlyPeriod(2001, 1, 1, 2002, 1, 1),_person))
					.IgnoreArguments()
					.Return(new List<DateOnlyPeriod> { new DateOnlyPeriod(2002, 1, 1, 2003, 1, 1) });
				Expect.Call(_dic.PermissionsEnabled).Return(true);
			}
			using (_mocks.Playback())
			{
                using (new CustomAuthorizationContext(_principalAuthorization))
                {
                    _target.Add(PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario,
                                                                         new DateTimePeriod(2000, 1, 2, 2000, 1, 3)));
                }
			}
		}

		[Test]
		public void VerifyAddRange()
		{
			fullPermission(true);
			_mocks.ReplayAll();
            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                IList<IPersonAssignment> pAsses = new List<IPersonAssignment>();
                pAsses.Add(createPersonAssignment(new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
                pAsses.Add(createPersonAssignment(new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
                IList<IPersonAbsence> pAbses = new List<IPersonAbsence>();
                pAbses.Add(createPersonAbsence(new DateTimePeriod(2000, 2, 2, 2000, 2, 3)));
                pAbses.Add(createPersonAbsence(new DateTimePeriod(2000, 2, 4, 2000, 2, 5)));
                IList<IPersonMeeting> personMeetings = new List<IPersonMeeting>();
                personMeetings.Add(createPersonMeeting(new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
                personMeetings.Add(createPersonMeeting(new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
                _target.AddRange(pAsses);
                _target.AddRange(pAbses);
                _target.AddRange(personMeetings);
                Assert.AreEqual(2, _target.PersonAbsences.Count);
                Assert.AreEqual(2, _target.PersonAssignments.Count);
                Assert.AreEqual(2, _target.PersonMeetings.Count);
            }
		}
		#endregion

		#region Clone
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyClone()
		{
			fullPermission(true);
			_mocks.ReplayAll();
            using (new CustomAuthorizationContext(_principalAuthorization))
            {
                //no deep clone, just shallow. Better test later if needed
                IActivity activity = ActivityFactory.CreateActivity("sdf");
                IPersonAssignment pAss = PersonAssignmentFactory.CreatePersonAssignment(_parameters.Person,
                                                                                        _parameters.Scenario);
                pAss.AddPersonalActivity(activity,_parameters.Period);

                _target.Add(new PersonAbsence(_parameters.Person, _parameters.Scenario,
                                              new AbsenceLayer(AbsenceFactory.CreateAbsence("abs"), _parameters.Period)));
                _target.Add(pAss);


                IPersonMeeting meeting = createPersonMeeting(new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

                _target.Add(meeting);

                ScheduleRange clone = (ScheduleRange) _target.Clone();
                var part = clone.ScheduledDay(new DateOnly(2000, 1, 1));

                Assert.AreEqual(1, part.PersistableScheduleDataCollection().OfType<IPersonAbsence>().Count());
                part.PersonAssignment().Should().Not.Be.Null();
                Assert.AreEqual(1, part.PersonMeetingCollection().Count);
            }
		}
		#endregion
	}
}
