using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture]
    public class StaffingThresholdValidatorTest
    {
        private IAbsenceRequestValidator _target;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private PersonRequestFactory _personRequestFactory;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IPerson _person;

        private ISkill _skill;
        private ISkillDay _skillDay;
        private IScheduleDictionary _dictionary;
        private TimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _target = new StaffingThresholdValidator();
            _mocks = new MockRepository();
            DateTimePeriod schedulingDateTimePeriod = new DateTimePeriod(2010, 02, 01, 2010, 02, 28);
            _dictionary = _mocks.StrictMock<IScheduleDictionary>();
            _schedulingResultStateHolder = SchedulingResultStateHolderFactory.Create(schedulingDateTimePeriod);
            _schedulingResultStateHolder.Schedules = _dictionary;
            _resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
            _personRequestFactory = new PersonRequestFactory();
            _person = new Person();
            _person.SetId(new Guid());
            _person.PermissionInformation.SetCulture(new CultureInfo(1033));
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            _person.PermissionInformation.SetDefaultTimeZone(_timeZone);
        }

        [Test]
        public void VerifyInvalidReason()
        {
            Assert.AreEqual("RequestDenyReasonSkillThreshold", _target.InvalidReason);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(UserTexts.Resources.Intraday, _target.DisplayText);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifySchedulingResultStateHolderCannotBeNull()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            _target.Validate(_personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod), new RequiredForHandlingAbsenceRequest(null,null,_resourceOptimizationHelper,null,null));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyResourceOptimizationHelperCannotBeNull()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            _target.Validate(_personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod), new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder,null,null,null,null));
        }

        [Test]
        public void VerifyCanCreateNewInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.AreNotSame(_target, newInstance);
            Assert.IsInstanceOf<StaffingThresholdValidator>(newInstance);
        }

        [Test]
        public void CanValidate()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
            
            createSkill();
            createSkillDay(requestedDateTimePeriod);
            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
            stateHolder.Schedules = _dictionary;

            var date = new DateOnly(2010, 02, 01);

            using (_mocks.Record())
            {
                GetExpectationIfItIsValid(date, absenceRequest, absence, requestedDateTimePeriod);
            }
            using (_mocks.Playback())
            {
                var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder,null,_resourceOptimizationHelper,null,null));
                Assert.IsFalse(result.IsValid);
            }
            _mocks.VerifyAll();
        }

        private void GetExpectationIfItIsValid(DateOnly date, IAbsenceRequest absenceRequest, IAbsence absence,
                                               DateTimePeriod requestedDateTimePeriod)
        {
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> {visualLayer};

            var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();

            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date, true, true));
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true));
            Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();
            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(date, date.AddDays(1)))).Return(new[] { scheduleDay, scheduleDay }).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, _timeZone)).Repeat.Twice();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date.AddDays(1), _timeZone)).Repeat.Twice();
            Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
            Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
            Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
        }

        private IAbsenceRequest GetAbsenceRequest(IAbsence absence, DateTimePeriod requestedDateTimePeriod)
        {
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
            absenceRequest.Person.SetId(Guid.NewGuid());
            
            var personPeriod = new PersonPeriod(new DateOnly(2010, 01, 01),
                                                PersonContractFactory
                                                    .CreateFulltimePersonContractWithWorkingWeekContractSchedule
                                                    (), TeamFactory.CreateSimpleTeam("Test Team"));

            var skill = PersonSkillFactory.CreatePersonSkill("Test Skill1", 0.5).Skill;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanValidateIfRequestedOnlyOneDayOfAbsence()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 0);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
            var date = new DateOnly(2010, 02, 01);
           
            createSkill();
            createSkillDay(requestedDateTimePeriod);
            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
            stateHolder.Schedules = _dictionary;

            using (_mocks.Record())
            {
                GetExpectationsIfRequestedOnlyOneDayOfAbsence(date, absenceRequest, absence, requestedDateTimePeriod);
            }
            using (_mocks.Playback())
            {
                var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null, _resourceOptimizationHelper, null, null));
                Assert.IsFalse(result.IsValid);
            }
            _mocks.VerifyAll();
        }

        private void GetExpectationsIfRequestedOnlyOneDayOfAbsence(DateOnly date, IAbsenceRequest absenceRequest,
                                                                   IAbsence absence, DateTimePeriod requestedDateTimePeriod)
        {
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> {visualLayer};
            var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();

            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date, true, true));
            Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();
            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(date, date))).Return(new[] { scheduleDay }).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, _timeZone)).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
            Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
            Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
        }

        [Test]
        public void CanValidateWithAgentInDifferentTimeZone()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
            var date = new DateOnly(2010, 02, 01);
            
            createSkill();
            createSkillDay(requestedDateTimePeriod);
            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
            stateHolder.Schedules = _dictionary;

            using (_mocks.Record())
            {
                GetValueWithAgentInDifferentTimeZone(date, absenceRequest, absence, requestedDateTimePeriod);
            }
            using (_mocks.Playback())
            {
                var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null, _resourceOptimizationHelper, null, null));
                Assert.IsFalse(result.IsValid);
            }
        }

        private void GetValueWithAgentInDifferentTimeZone(DateOnly date, IAbsenceRequest absenceRequest, IAbsence absence,
                                                          DateTimePeriod requestedDateTimePeriod)
        {
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> {visualLayer};

            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date, true, true));
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true));
            Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();

            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(date, date.AddDays(1)))).Return(new[] { scheduleDay, scheduleDay }).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, _timeZone)).Repeat.Twice();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date.AddDays(1), _timeZone)).Repeat.Twice();
            Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
            Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
            Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
        }

        [Test]
        public void CanValidateIfNotUnderstaffed()
        {
            DateTimePeriod requestedDateTimePeriod =
                DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            var absenceRequest = GetAbsenceRequest(absence, requestedDateTimePeriod);
            var date = new DateOnly(2010, 02, 01);
            
            createSkill();

            _skillDay = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod.StartDateTime);

            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                requestedDateTimePeriod, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
                ServiceAgreement.DefaultValues());

            skillStaffPeriod.IsAvailable = true;
            _skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> {_skillDay},
                                                                  requestedDateTimePeriod.ToDateOnlyPeriod(
                                                                      _skill.TimeZone));

            var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> {skillStaffPeriod});
            _skillDay.SetCalculatedStaffCollection(updatedValues);
            updatedValues.BatchCompleted();

            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod,
                                                                                            _skill,
                                                                                            new List<ISkillDay>
                                                                                                {
                                                                                                    _skillDay
                                                                                                });
            stateHolder.Schedules = _dictionary;

            using (_mocks.Record())
            {
                getExpectationsIfNotUnderStaffed(date, absenceRequest, absence, requestedDateTimePeriod);
            }

            var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null, _resourceOptimizationHelper, null, null));
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
                                 ActivityFactory.CreateActivity("Phone"), _person);

            createSkill();

            _skillDay = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod.StartDateTime);

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

            var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1,skillStaffPeriod2 });
            _skillDay.SetCalculatedStaffCollection(updatedValues);
            updatedValues.BatchCompleted();

            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod,
                                                                                            _skill,
                                                                                            new List<ISkillDay>
                                                                                                {
                                                                                                    _skillDay
                                                                                                });
            stateHolder.Schedules = _dictionary;

            using (_mocks.Record())
            {
                getExpectationsIfNotUnderStaffed(date, absenceRequest, absence, requestedDateTimePeriod, existingLayerWithSameAbsence);
            }

            var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null, _resourceOptimizationHelper, null, null));
            Assert.IsTrue(result.IsValid);
        }

        private void getExpectationsIfNotUnderStaffed(DateOnly date, IAbsenceRequest absenceRequest, IAbsence absence,
                                                      DateTimePeriod requestedDateTimePeriod, IVisualLayer extraLayer = null)
        {
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> {visualLayer};
            if (extraLayer!=null) visualLayers.Add(extraLayer);

            Expect.Call(()=>_resourceOptimizationHelper.ResourceCalculateDate(date, true, true)).Repeat.Twice();
            Expect.Call(()=>_resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true));
            Expect.Call(()=>_resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(2), true, true));
            Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();
            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(date, date.AddDays(1)))).Return(new[] { scheduleDay, scheduleDay }).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, _timeZone)).Repeat.Twice();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date.AddDays(1), _timeZone)).Repeat.Twice();
            Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(visualLayerCollection.FilterLayers(absence))
                  .Return(filteredVisualLayers)
                  .Repeat.AtLeastOnce();
            Expect.Call(filteredVisualLayers.GetEnumerator())
                  .Return(visualLayers.GetEnumerator())
                  .Repeat.AtLeastOnce();
            Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
        }

        [Test]
		public void ShouldValidateWhenUnderstaffingForMaxOneHundredPercent()
		{
            var requestedDateTimePeriod1 = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            var requestedDateTimePeriod2 = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 02, 0, 0, 0, DateTimeKind.Utc), 1);
			createSkill();

			var skillDay1 = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod1.StartDateTime);

			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod1, new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod1.IsAvailable = true;
			skillDay1.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay1 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
            var updatedValues1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1 });
			skillDay1.SetCalculatedStaffCollection(updatedValues1);
            updatedValues1.BatchCompleted();

			var skillDay2 = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod2.StartDateTime);
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod2, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod2.IsAvailable = true;
            skillDay2.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay2 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
            var updatedValues2 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod2 });
			skillDay2.SetCalculatedStaffCollection(updatedValues2);
            updatedValues2.BatchCompleted();

            var understaffingDetails = new UnderstaffingDetails();
            var validateUnderStaffingSkillDay1 = StaffingThresholdValidator.ValidateUnderstaffing(_skill,
		                                                                                          new List<ISkillStaffPeriod>
		                                                                                              {
		                                                                                                  skillDay1
		                                                                                              .SkillStaffPeriodCollection
		                                                                                              [0]
		                                                                                              }, _timeZone, understaffingDetails);

		    var validatedUnderStaffingSkillDay2 = StaffingThresholdValidator.ValidateUnderstaffing(_skill,
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
			
			_skill = SkillFactory.CreateSkill("TunaFish", SkillTypeFactory.CreateSkillType(), 15);
			_skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent(), new Percent(0.4));
     
			var skillDay1 = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod1.StartDateTime);

			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod1, new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod1.IsAvailable = true;
            skillDay1.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay1 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
            var updatedValues1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1 });
			skillDay1.SetCalculatedStaffCollection(updatedValues1);
            updatedValues1.BatchCompleted();

			var skillDay2 = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod2.StartDateTime);
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod2, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());
			skillStaffPeriod2.IsAvailable = true;
            skillDay2.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay2 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
            var updatedValues2 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod2 });
		    skillDay2.SetCalculatedStaffCollection(updatedValues2);
            updatedValues2.BatchCompleted();

		    var validatedUnderStaffing = StaffingThresholdValidator.ValidateUnderstaffing(_skill,
		                                                                                  new List<ISkillStaffPeriod>
		                                                                                      {
		                                                                                          skillDay1
		                                                                                      .SkillStaffPeriodCollection[0],
		                                                                                          skillDay2
		                                                                                      .SkillStaffPeriodCollection[0]
		                                                                                      },
		                                                                                  _timeZone, new UnderstaffingDetails());

			Assert.IsTrue(validatedUnderStaffing.IsValid);
		}

        [Test]
        public void ShouldValidateWhenUnderstaffingForMaxFortyPercentForOnlyDayOnly()
        {
            var requestedDateTimePeriod1 = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
         
            _skill = SkillFactory.CreateSkill("TunaFish", SkillTypeFactory.CreateSkillType(), 15);
            _skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent(), new Percent(0.4));

            var skillDay1 = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod1.StartDateTime);

            var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                requestedDateTimePeriod1, new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
                ServiceAgreement.DefaultValues());
            skillStaffPeriod1.IsAvailable = true;
            skillDay1.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay1 }, requestedDateTimePeriod1.ToDateOnlyPeriod(_skill.TimeZone));
            var updatedValues1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod1 });
            skillDay1.SetCalculatedStaffCollection(updatedValues1);
            updatedValues1.BatchCompleted();

            var validatedUnderStaffing = StaffingThresholdValidator.ValidateUnderstaffing(_skill,
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
            _skillDay = SkillDayFactory.CreateSkillDay(_skill, period.StartDateTime);

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
            _skill = SkillFactory.CreateSkill("TunaFish", SkillTypeFactory.CreateSkillType(), 15);
            _skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent());
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
            
            createSkill();

            _skillDay = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod.StartDateTime);

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

            using (_mocks.Record())
            {
                GetExpectationsIfNotUnderStaffing(date, absenceRequest, absence, requestedDateTimePeriod);
            }

            var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder, null, _resourceOptimizationHelper, null, null));
            Assert.IsTrue(result.IsValid);
        }

        private void GetExpectationsIfNotUnderStaffing(DateOnly date, IAbsenceRequest absenceRequest, IAbsence absence,
                                                       DateTimePeriod requestedDateTimePeriod)
        {
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> {visualLayer};

            Expect.Call(()=>_resourceOptimizationHelper.ResourceCalculateDate(date, true, true)).Repeat.Twice();
            Expect.Call(()=>_resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(1), true, true)).Repeat.Twice();
            Expect.Call(()=>_resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(2), true, true)).Repeat.Twice();
            Expect.Call(()=>_resourceOptimizationHelper.ResourceCalculateDate(date.AddDays(3), true, true));
            Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();

            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(date, date.AddDays(1)))).Return(new[] { scheduleDay, scheduleDay }).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, _timeZone)).Repeat.Twice();
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date.AddDays(1), _timeZone)).Repeat.Twice();
            Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
            Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
            Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowExceptionIfSkillStaffPeriodListIsNull()
        {
            var skill = SkillFactory.CreateSkill("test");
            StaffingThresholdValidator.ValidateSeriousUnderstaffing(skill, null, skill.TimeZone, new UnderstaffingDetails());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfSkillStaffPeriodListArgumentIsNull()
        {
            var skill = SkillFactory.CreateSkill("test");
            StaffingThresholdValidator.ValidateUnderstaffing(skill, null, skill.TimeZone, new UnderstaffingDetails());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyUnderstaffingDateString()
        {
            var underStaffDict = new UnderstaffingDetails();
            underStaffDict.AddUnderstaffingDay(new DateOnly(2012,12,01));
            underStaffDict.AddSeriousUnderstaffingDay(new DateOnly(2012,12,01));
            
            var target = new StaffingThresholdValidator();
            var result = target.GetUnderStaffingDateString(underStaffDict, new CultureInfo(1033), new CultureInfo(1033));

            Assert.IsNotNullOrEmpty(result);
        }

        [Test]
        public void VerifyUnderstaffingHourString()
        {
            var underStaffDict = new UnderstaffingDetails();
            underStaffDict.AddUnderstaffingTime(new TimePeriod(10,00,10,15));
            underStaffDict.AddSeriousUnderstaffingTime(new TimePeriod(10,00,10,15));
            
            var target = new StaffingThresholdValidator();
            var result = target.GetUnderStaffingHourString(underStaffDict, new CultureInfo(1033), new CultureInfo(1033), _timeZone, new DateTime(2012,01,01));

            Assert.IsNotNullOrEmpty(result);
        }
     }
}
