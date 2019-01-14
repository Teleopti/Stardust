using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class StaffingThresholdValidatorTest
	{
		private StaffingThresholdValidator _target;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private PersonRequestFactory _personRequestFactory;
		private IResourceCalculation _resourceOptimizationHelper;
		private IPerson _person;

		private ISkill _skill;
		private ISkillDay _skillDay;
		private ScheduleDictionaryForTest _dictionary;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void Setup()
		{
			_target = new StaffingThresholdValidator();
			DateTimePeriod schedulingDateTimePeriod = new DateTimePeriod(2010, 02, 01, 2010, 02, 28);
			_dictionary = new ScheduleDictionaryForTest(MockRepository.GenerateMock<IScenario>(), schedulingDateTimePeriod.StartDateTime);
			_schedulingResultStateHolder = SchedulingResultStateHolderFactory.Create(schedulingDateTimePeriod);
			_schedulingResultStateHolder.Schedules = _dictionary;
			_resourceOptimizationHelper = MockRepository.GenerateMock<IResourceCalculation>();
			_personRequestFactory = new PersonRequestFactory();
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2001, 1, 1));
			_personRequestFactory.Person = _person;
			_person.PermissionInformation.SetCulture(new CultureInfo(1033));
			_timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_person.PermissionInformation.SetDefaultTimeZone(_timeZone);
			createSkill();
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.AreEqual(UserTexts.Resources.Intraday, _target.DisplayText);
		}

		[Test]
		public void VerifySchedulingResultStateHolderCannotBeNull()
		{
			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			Assert.Throws<ArgumentNullException>(() => _target.Validate(_personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod), new RequiredForHandlingAbsenceRequest(null, null,null, _resourceOptimizationHelper, null, null)));
		}

		[Test]
		public void VerifyResourceOptimizationHelperCannotBeNull()
		{
			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			Assert.Throws<ArgumentNullException>(() => _target.Validate(_personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod), new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder,null, null, null, null, null)));
		}

		[Test]
		public void VerifyCanCreateNewInstance()
		{
			var newInstance = _target.CreateInstance();
			Assert.AreNotSame(_target, newInstance);
			Assert.IsInstanceOf<StaffingThresholdValidator>(newInstance);
		}


		private IScheduleRange GetExpectationForTwoDays(DateOnly date, IAbsence absence, DateTimePeriod requestedDateTimePeriod, IList<IVisualLayer> extraLayers = null)
		{
			IScheduleRange range = MockRepository.GenerateMock<IScheduleRange>();
			IScheduleDay scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			IScheduleDay scheduleDay2 = MockRepository.GenerateMock<IScheduleDay>();
			IProjectionService projectionService = MockRepository.GenerateMock<IProjectionService>();
			IVisualLayerCollection visualLayerCollection = MockRepository.GenerateMock<IVisualLayerCollection>();
			IVisualLayer visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			IList<IVisualLayer> visualLayers = new List<IVisualLayer> { visualLayer };
			if (extraLayers != null)
			{
				foreach (var extraLayer in extraLayers)
				{
					visualLayers.Add(extraLayer);
				}
			}

			var filteredVisualLayers = MockRepository.GenerateMock<IFilteredVisualLayerCollection>();

			range.Stub(x => x.ScheduledDayCollection(new DateOnlyPeriod(date, date.AddDays(1)))).Return(new[] { scheduleDay, scheduleDay2 }).IgnoreArguments();
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, _timeZone));
			scheduleDay2.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date.AddDays(1), _timeZone));
			scheduleDay.Stub(x => x.ProjectionService()).Return(projectionService);
			scheduleDay2.Stub(x => x.ProjectionService()).Return(projectionService);
			projectionService.Stub(x => x.CreateProjection()).Return(visualLayerCollection);
			visualLayerCollection.Stub(x => x.FilterLayers(absence)).Return(filteredVisualLayers);
			filteredVisualLayers.Stub(x => x.GetEnumerator()).Return(visualLayers.GetEnumerator());
			visualLayer.Stub(x => x.Period).Return(requestedDateTimePeriod);

			return range;
		}

		private IAbsenceRequest GetAbsenceRequest(IAbsence absence, DateTimePeriod requestedDateTimePeriod)
		{
			IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
			absenceRequest.Person.SetId(Guid.NewGuid());

			var personPeriod = new PersonPeriod(new DateOnly(2010, 01, 01),
												PersonContractFactory
													.CreateFulltimePersonContractWithWorkingWeekContractSchedule
													(), TeamFactory.CreateSimpleTeam("Test Team"));

			var skill = _skill;
			var wl = WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);

			foreach (var day in wl.TemplateWeekCollection)
			{
				day.Value.MakeOpen24Hours();
			}

			skill.AddWorkload(wl);
			personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(0.5)));
			absenceRequest.Person.AddPersonPeriod(personPeriod);
			return absenceRequest;
		}

		[Test]
		public void CanValidateIfRequestedOnlyOneDayOfAbsence()
		{
			DateTimePeriod requestedDateTimePeriod =
				DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 0);
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
			var date = new DateOnly(2010, 02, 01);

			createSkillDay(requestedDateTimePeriod);
			var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill,
																		new List<ISkillDay> { _skillDay });
			stateHolder.Schedules = _dictionary;

			_dictionary.AddTestItem(absenceRequest.Person, GetExpectationsForOneDay(date, absence, requestedDateTimePeriod));

			var result = _target.Validate(absenceRequest,
										  new RequiredForHandlingAbsenceRequest(stateHolder, null,null, _resourceOptimizationHelper, null, null));

			Assert.IsFalse(result.IsValid);
		}

		private IScheduleRange GetExpectationsForOneDay(DateOnly date, IAbsence absence, DateTimePeriod requestedDateTimePeriod)
		{
			IScheduleRange range = MockRepository.GenerateMock<IScheduleRange>();
			IScheduleDay scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			IProjectionService projectionService = MockRepository.GenerateMock<IProjectionService>();
			IVisualLayerCollection visualLayerCollection = MockRepository.GenerateMock<IVisualLayerCollection>();
			IVisualLayer visualLayer = MockRepository.GenerateMock<IVisualLayer>();
			IList<IVisualLayer> visualLayers = new List<IVisualLayer> { visualLayer };
			var filteredVisualLayers = MockRepository.GenerateMock<IFilteredVisualLayerCollection>();

			range.Stub(x => x.ScheduledDayCollection(new DateOnlyPeriod(date, date))).Return(new[] { scheduleDay }).IgnoreArguments();
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, _timeZone));
			scheduleDay.Stub(x => x.ProjectionService()).Return(projectionService);
			projectionService.Stub(x => x.CreateProjection()).Return(visualLayerCollection);
			visualLayerCollection.Stub(x => x.FilterLayers(absence)).Return(filteredVisualLayers);
			filteredVisualLayers.Stub(x => x.GetEnumerator()).Return(visualLayers.GetEnumerator());
			visualLayer.Stub(x => x.Period).Return(requestedDateTimePeriod);

			return range;
		}

		[Test]
		public void ShouldDenyIfSeriouslyUnderStaffedDuringOnlyInterval()
		{
			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 02, 01, 0, 15, 0, DateTimeKind.Utc));
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
			var date = new DateOnly(2010, 02, 01);

			createSkillDay(requestedDateTimePeriod.ChangeEndTime(TimeSpan.FromMinutes(15)));
			var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
			stateHolder.Schedules = _dictionary;

			_dictionary.AddTestItem(absenceRequest.Person, GetExpectationsForOneDay(date, absence, requestedDateTimePeriod.ChangeEndTime(TimeSpan.FromMinutes(15))));

			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null,null, _resourceOptimizationHelper, null, null));
			Assert.IsFalse(result.IsValid);
		}

		[Test]
		public void ShouldValidateWhenNoPersonalSkills()
		{
			var requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 0);
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var date = new DateOnly(2010, 02, 01);
			var absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
			var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });

			absenceRequest.Person.SetId(Guid.NewGuid());
			createSkillDay(requestedDateTimePeriod);
			stateHolder.Schedules = _dictionary;
			_dictionary.AddTestItem(absenceRequest.Person, GetExpectationsForOneDay(date, absence, requestedDateTimePeriod));

			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null,null, _resourceOptimizationHelper, null, null));
			Assert.IsTrue(result.IsValid);
		}

		[Test]
		public void CanValidateWithAgentInDifferentTimeZone()
		{
			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
			var date = new DateOnly(2010, 02, 01);

			createSkillDay(requestedDateTimePeriod);
			var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
			stateHolder.Schedules = _dictionary;

			_dictionary.AddTestItem(absenceRequest.Person, GetExpectationForTwoDays(date, absence, requestedDateTimePeriod));

			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null,null, _resourceOptimizationHelper, null, null));
			Assert.IsFalse(result.IsValid);
		}

		[Test]
		public void CanValidateIfNotUnderstaffed()
		{
			DateTimePeriod requestedDateTimePeriod =
				DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
			var date = new DateOnly(2010, 02, 01);


			_skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod.StartDateTime));

			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());

			skillStaffPeriod.IsAvailable = true;
			_skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay },
																  requestedDateTimePeriod.ToDateOnlyPeriod(
																	  _skill.TimeZone));

			var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
			_skillDay.SetCalculatedStaffCollection(updatedValues);
			updatedValues.BatchCompleted();

			var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod,
																		_skill,
																		new List<ISkillDay>
																		{
																			_skillDay
																		});
			stateHolder.Schedules = _dictionary;

			GetExpectationForTwoDays(date, absence, requestedDateTimePeriod);

			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder,null, null, _resourceOptimizationHelper, null, null));
			Assert.IsTrue(result.IsValid);
		}

		[Test]
		public void CanValidateIfNotUnderstaffedWithSameAbsenceInPeriodSinceBefore()
		{
			DateTimePeriod requestedDateTimePeriod = new DateTimePeriod(new DateTime(2010, 02, 01, 1, 0, 0, DateTimeKind.Utc), new DateTime(2010, 02, 02, 3, 0, 0, DateTimeKind.Utc));
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
			var date = new DateOnly(2010, 02, 01);
			var existingLayerWithSameAbsence = new VisualLayer(absence, new DateTimePeriod(new DateTime(2010, 02, 02, 3, 0, 0, DateTimeKind.Utc), new DateTime(2010, 02, 02, 4, 0, 0, DateTimeKind.Utc)),
															   ActivityFactory.CreateActivity("Phone"));


			_skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod.StartDateTime));

			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());

			ISkillStaffPeriod skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				existingLayerWithSameAbsence.Period, new Task(100, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());

			skillStaffPeriod1.IsAvailable = true;
			_skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay },
																  requestedDateTimePeriod.ToDateOnlyPeriod(
																	  _skill.TimeZone));

			var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2 });
			_skillDay.SetCalculatedStaffCollection(updatedValues);
			updatedValues.BatchCompleted();

			var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod,
																		_skill,
																		new List<ISkillDay>
																		{
																			_skillDay
																		});
			stateHolder.Schedules = _dictionary;

			GetExpectationForTwoDays(date, absence, requestedDateTimePeriod, new List<IVisualLayer> { existingLayerWithSameAbsence });

			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null,null, _resourceOptimizationHelper, null, null));
			Assert.IsTrue(result.IsValid);
		}
		
		
		[Test]
		public void ShouldValidateWhenUnderstaffingForMaxOneHundredPercent()
		{	
			var requestedDateTimePeriod1 = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
			var requestedDateTimePeriod2 = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 02, 0, 0, 0, DateTimeKind.Utc), 1);

			var skillDay1 = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod1.StartDateTime));

			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod1, new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod1.IsAvailable = true;
			skillDay1.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay1 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
			var updatedValues1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1 });
			skillDay1.SetCalculatedStaffCollection(updatedValues1);
			updatedValues1.BatchCompleted();

			var skillDay2 = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod2.StartDateTime));
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod2, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod2.IsAvailable = true;
			skillDay2.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay2 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
			var updatedValues2 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod2 });
			skillDay2.SetCalculatedStaffCollection(updatedValues2);
			updatedValues2.BatchCompleted();

			var understaffingDetails = new UnderstaffingDetails();

			var validateUnderStaffingSkillDay1 = _target.ValidateUnderstaffing(_skill,
																			   new List<ISkillStaffPeriod>
																			   {
																				   skillDay1
																					   .SkillStaffPeriodCollection
																					   [0]
																			   }, _timeZone, understaffingDetails);

			var validatedUnderStaffingSkillDay2 = _target.ValidateUnderstaffing(_skill,
																				new List<ISkillStaffPeriod>
																				{
																					skillDay2
																						.SkillStaffPeriodCollection
																						[0]
																				}, _timeZone, understaffingDetails);

			Assert.IsFalse(validateUnderStaffingSkillDay1.IsValid);
			Assert.IsTrue(validatedUnderStaffingSkillDay2.IsValid);
		}

		[Test]
		public void ShouldValidateWhenUnderstaffingForMaxFortyPercent()
		{
			var requestedDateTimePeriod1 = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
			var requestedDateTimePeriod2 = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 02, 0, 0, 0, DateTimeKind.Utc), 1);

			_skill = SkillFactory.CreateSkill("TunaFish", SkillTypeFactory.CreateSkillTypePhone(), 15);
			_skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent(), new Percent(0.4));

			var skillDay1 = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod1.StartDateTime));

			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod1, new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod1.IsAvailable = true;
			skillDay1.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay1 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
			var updatedValues1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1 });
			skillDay1.SetCalculatedStaffCollection(updatedValues1);
			updatedValues1.BatchCompleted();

			var skillDay2 = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod2.StartDateTime));
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod2, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod2.IsAvailable = true;
			skillDay2.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay2 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
			var updatedValues2 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod2 });
			skillDay2.SetCalculatedStaffCollection(updatedValues2);
			updatedValues2.BatchCompleted();
			var detail = new UnderstaffingDetails();
			var validatedUnderStaffing = _target.ValidateUnderstaffing(_skill,
																	   new List<ISkillStaffPeriod>
																	   {
																		   skillDay1
																			   .SkillStaffPeriodCollection[0],
																		   skillDay2
																			   .SkillStaffPeriodCollection[0]
																	   },
																	   _timeZone, detail);

			Assert.IsTrue(validatedUnderStaffing.IsValid);
			Assert.That(detail.IsNotUnderstaffed(), Is.True);
		}

		[Test]
		public void ShouldValidateWhenUnderstaffingForMaxFortyPercentForOnlyDayOnly()
		{
			var requestedDateTimePeriod1 = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);

			_skill = SkillFactory.CreateSkill("TunaFish", SkillTypeFactory.CreateSkillTypePhone(), 15);
			_skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent(), new Percent(0.4));

			var skillDay1 = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod1.StartDateTime));

			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod1, new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod1.IsAvailable = true;
			skillDay1.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay1 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
			var updatedValues1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1 });
			skillDay1.SetCalculatedStaffCollection(updatedValues1);
			updatedValues1.BatchCompleted();
			var validatedUnderStaffing = _target.ValidateUnderstaffing(_skill,
																	   new List<ISkillStaffPeriod>
																	   {
																		   skillDay1
																			   .SkillStaffPeriodCollection[0]
																	   },
																	   _timeZone, new UnderstaffingDetails());

			Assert.IsFalse(validatedUnderStaffing.IsValid);
		}

		private void createSkillDay(DateTimePeriod period)
		{
			_skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(period.StartDateTime));

			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				period, new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
				ServiceAgreement.DefaultValues());

			skillStaffPeriod.IsAvailable = true;
			_skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay }, period.ToDateOnlyPeriod(_skill.TimeZone));
			var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
			_skillDay.SetCalculatedStaffCollection(updatedValues);
			updatedValues.BatchCompleted();
		}

		private void createSkill()
		{
			_skill = SkillFactory.CreateSkill("TunaFish", SkillTypeFactory.CreateSkillTypePhone(), 15);
			_skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent());
			_skill.WithId();
		}

		[Test]
		public void VerifyEquals()
		{
			var otherValidatorOfSameKind = new StaffingThresholdValidator();
			var otherValidator = new AbsenceRequestNoneValidator();

			Assert.IsTrue(otherValidatorOfSameKind.Equals(_target));
			Assert.IsFalse(_target.Equals(otherValidator));
		}

		[Test]
		public void ShouldGetHashCodeInReturn()
		{
			var result = _target.GetHashCode();
			Assert.IsNotNull(result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldValidatedTrueIfNotUnderstaffing()
		{
			var date = new DateOnly(2010, 02, 01);
			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);

			_skillDay = SkillDayFactory.CreateSkillDay(_skill, date);

			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());

			skillStaffPeriod.IsAvailable = true;
			_skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay }, requestedDateTimePeriod.ToDateOnlyPeriod(_skill.TimeZone));
			var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod>());
			_skillDay.SetCalculatedStaffCollection(updatedValues);
			updatedValues.BatchCompleted();

			var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
			stateHolder.Schedules = _dictionary;

			_dictionary.AddTestItem(absenceRequest.Person, GetExpectationForTwoDays(date, absence, requestedDateTimePeriod));

			var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null,null, _resourceOptimizationHelper, null, null));
			Assert.IsTrue(result.IsValid);
		}

		[Test]
		public void ShouldThrowExceptionIfSkillStaffPeriodListIsNull()
		{
			var skill = SkillFactory.CreateSkill("test");
			Assert.Throws<ArgumentNullException>(() => _target.ValidateUnderstaffing(skill, null, skill.TimeZone, new UnderstaffingDetails()));
		}

		[Test]
		public void ShouldThrowExceptionIfSkillStaffPeriodListArgumentIsNull()
		{
			var skill = SkillFactory.CreateSkill("test");
			Assert.Throws<ArgumentNullException>(() => _target.ValidateUnderstaffing(skill, null, skill.TimeZone, new UnderstaffingDetails()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void VerifyUnderstaffingDateString()
		{
			var underStaffDict = new UnderstaffingDetails();
			underStaffDict.AddUnderstaffingDay(new DateOnly(2012, 12, 01));

			var target = new StaffingThresholdValidator();
			var result = target.GetUnderStaffingDateString(underStaffDict, new CultureInfo(1033), new CultureInfo(1033));

			Assert.That(result, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void ShouldCreateOneStringOfAdjacentPeriods()
		{
			var underStaffDict = new UnderstaffingDetails();
			underStaffDict.AddUnderstaffingPeriod(new DateTimePeriod(new DateTime(2017, 4, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2017, 4, 1, 10, 10, 0, DateTimeKind.Utc)));
			underStaffDict.AddUnderstaffingPeriod(new DateTimePeriod(new DateTime(2017, 4, 1, 10, 30, 0, DateTimeKind.Utc), new DateTime(2017, 4, 1, 10, 45, 0, DateTimeKind.Utc)));
			underStaffDict.AddUnderstaffingPeriod(new DateTimePeriod(new DateTime(2017, 4, 1, 10, 45, 0, DateTimeKind.Utc), new DateTime(2017, 4, 1, 11, 0, 0, DateTimeKind.Utc)));
			underStaffDict.AddUnderstaffingPeriod(new DateTimePeriod(new DateTime(2017, 4, 1, 12, 45, 0, DateTimeKind.Utc), new DateTime(2017, 4, 1, 13, 0, 0, DateTimeKind.Utc)));

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var target = new StaffingThresholdWithShrinkageValidator();
			var result = target.GetUnderStaffingPeriodsString(underStaffDict, new CultureInfo(1033), new CultureInfo(1033), timeZone);

			result.Length.Should().Be.GreaterThan(100);
		}

		[Test]
		public void ShouldReturnAllUnderstaffedDays()
		{
			var requestedDateTimePeriod = new DateTimePeriod(new DateTime(2010, 02, 01, 1, 0, 0, DateTimeKind.Utc), new DateTime(2010, 02, 02, 20, 0, 0, DateTimeKind.Utc));
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
			var date = new DateOnly(2010, 02, 01);
			var existingLayerWithSameAbsence = new VisualLayer(absence, new DateTimePeriod(new DateTime(2010, 02, 01, 3, 0, 0, DateTimeKind.Utc), new DateTime(2010, 02, 01, 4, 0, 0, DateTimeKind.Utc)),
															   ActivityFactory.CreateActivity("Phone"));

			var existingLayerWithSameAbsence2 = new VisualLayer(absence, new DateTimePeriod(new DateTime(2010, 02, 02, 3, 0, 0, DateTimeKind.Utc), new DateTime(2010, 02, 02, 4, 0, 0, DateTimeKind.Utc)),
															   ActivityFactory.CreateActivity("Phone"));

			_skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod.StartDateTime));
			var skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod.StartDateTime.AddDays(1)));

			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());

			ISkillStaffPeriod skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				existingLayerWithSameAbsence2.Period, new Task(100, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());

			skillStaffPeriod1.IsAvailable = true;
			skillStaffPeriod2.IsAvailable = true;
			_skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay },
																  requestedDateTimePeriod.ToDateOnlyPeriod(
																	  _skill.TimeZone));
			skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay },
																  requestedDateTimePeriod.ToDateOnlyPeriod(
																	  _skill.TimeZone));
			var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2 });
			_skillDay.SetCalculatedStaffCollection(updatedValues);
			skillDay.SetCalculatedStaffCollection(updatedValues);
			updatedValues.BatchCompleted();

			var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod,
																		_skill,
																		new List<ISkillDay>
																		{
																			_skillDay, skillDay
																		});
			stateHolder.Schedules = _dictionary;
			_dictionary.AddTestItem(_person, GetExpectationForTwoDays(date, absence, requestedDateTimePeriod, new List<IVisualLayer> { existingLayerWithSameAbsence, existingLayerWithSameAbsence2 }));

			_person.AddSkill(_skill, date);

			var personSkillProvider = new PersonSkillProvider();

			var result = _target.GetUnderStaffingDays(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder,null,null, _resourceOptimizationHelper, null), personSkillProvider);
			result.UnderstaffingDays.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSayIsValidFalseWhenSeriousUnderstaffed()
		{
			var start = new DateTime(2010, 02, 01, 7, 0, 0, DateTimeKind.Utc);
			var requestedDateTimePeriod1 = new DateTimePeriod(start, start.AddHours(1));
			
			_skill = SkillFactory.CreateSkill("TunaFish", SkillTypeFactory.CreateSkillTypePhone(), 15);
			_skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent(), new Percent(0.4));
			((PersonPeriod)_person.PersonPeriodCollection.First()).AddPersonSkill((new PersonSkill(_skill, new Percent(1))));

			// to get it open
			SkillDayFactory.CreateSkillDay(_skill, new DateOnly(requestedDateTimePeriod1.StartDateTime));

			var intervals = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval
				{
					FStaff = 200,
					//Serious understaffed
					CalculatedResource = 100,
					StartDateTime = start,
					EndDateTime = start.AddMinutes(15),
					SkillId = _skill.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					FStaff = 200,
					CalculatedResource = 175,
					StartDateTime = start.AddMinutes(15),
					EndDateTime = start.AddMinutes(30),
					SkillId = _skill.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					FStaff = 200,
					CalculatedResource = 180,
					StartDateTime = start.AddMinutes(30),
					EndDateTime = start.AddMinutes(45),
					SkillId = _skill.Id.GetValueOrDefault()
				},
				new SkillStaffingInterval
				{
					FStaff = 200,
					CalculatedResource = 180,
					StartDateTime = start.AddMinutes(45),
					EndDateTime = start.AddMinutes(60),
					SkillId = _skill.Id.GetValueOrDefault()
				}
			};

			IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
			var req = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod1);

			var validatedUnderStaffing = _target.ValidateLight(req, intervals);

			validatedUnderStaffing.IsValid.Should().Be.False();
		}
	}
}
