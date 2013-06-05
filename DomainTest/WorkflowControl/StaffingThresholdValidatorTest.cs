using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class StaffingThresholdValidatorTest
    {
        private IAbsenceRequestValidator _target;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private PersonRequestFactory _personRequestFactory;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IValidatedRequest _validatedRequest;
        private IPerson _person;

        private ISkill _skill;
        private ISkillDay _skillDay;
        private IScheduleDictionary _dictionary;

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
            _validatedRequest = new ValidatedRequest{IsValid = true, ValidationErrors = ""};
            _person = new Person();
            _person.SetId(new Guid());
            _person.PermissionInformation.SetCulture(new CultureInfo(1033));
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
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
            Assert.IsTrue(typeof(StaffingThresholdValidator).IsInstanceOfType(newInstance));
        }

        [Test]
        public void CanValidate()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer>{visualLayer};
			var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            
            createSkill();
            createSkillDay(requestedDateTimePeriod);
            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
            stateHolder.Schedules = _dictionary;

            _validatedRequest.IsValid = false;
            _validatedRequest.ValidationErrors = "Not Valid";

            using (_mocks.Record())
            {
                Expect.Call(()=>_resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 01), true, true)).Repeat.Times(2);
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 02), true, true)).Repeat.Times(3);
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 03), true, true));
                Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 1))).Return(scheduleDay).Repeat.Twice();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 2))).Return(scheduleDay);
                Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
                Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
                Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder,null,_resourceOptimizationHelper,null,null));
                Assert.IsFalse(result.IsValid);
            }
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanValidateIfRequestedOnlyOneDayOfAbsence()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 0);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> { visualLayer };
            var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();

            createSkill();
            createSkillDay(requestedDateTimePeriod);
            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
            stateHolder.Schedules = _dictionary;

            _validatedRequest.IsValid = false;
            _validatedRequest.ValidationErrors = "Not Valid";

            using (_mocks.Record())
            {
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 01), true, true)).Repeat.Times(2);
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 02), true, true)).Repeat.Times(2);
                Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 1))).Return(scheduleDay).Repeat.Twice();
                Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
                Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
                Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder,null,_resourceOptimizationHelper,null,null));
                Assert.IsFalse(result.IsValid);
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void CanValidateWithAgentInDifferentTimeZone()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
            absenceRequest.Person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
			var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> { visualLayer };

            _validatedRequest.IsValid = false;
            _validatedRequest.ValidationErrors = "Not Valid";

            createSkill();
            createSkillDay(requestedDateTimePeriod);
            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
            stateHolder.Schedules = _dictionary;

            using (_mocks.Record())
            {
                Expect.Call(
                    () => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 01), true, true))
                      .Repeat.Twice();
                Expect.Call(
                    () => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 02), true, true))
                      .Repeat.Times(3);
                Expect.Call(
                    () => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 03), true, true))
                      .Repeat.Times(3);
                Expect.Call(
                    () => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 04), true, true))
                      .Repeat.Twice();
                Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 1))).Return(scheduleDay).Repeat.Twice();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 2))).Return(scheduleDay);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 3))).Return(scheduleDay);
                Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
                Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
                Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
            }

            var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder,null,_resourceOptimizationHelper,null,null));
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void CanValidateIfNotUnderstaffed()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
			var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> { visualLayer };

            _validatedRequest.IsValid = true;
            _validatedRequest.ValidationErrors = "";

            createSkill();

			_skillDay = SkillDayFactory.CreateSkillDay(_skill, requestedDateTimePeriod.StartDateTime);

			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
				requestedDateTimePeriod, new Task(0, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0)),
				ServiceAgreement.DefaultValues());

			skillStaffPeriod.IsAvailable = true;
			_skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay }, requestedDateTimePeriod.ToDateOnlyPeriod(_skill.TimeZone));
            var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> {skillStaffPeriod});
			_skillDay.SetCalculatedStaffCollection(updatedValues);
            updatedValues.BatchCompleted();

            var stateHolder = SchedulingResultStateHolderFactory.Create(requestedDateTimePeriod, _skill, new List<ISkillDay> { _skillDay });
            stateHolder.Schedules = _dictionary;

            using (_mocks.Record())
            {
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 01), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 01), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 02), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 02), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 03), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 03), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 04), true, true);
                Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 1))).Return(scheduleDay);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 2))).Return(scheduleDay);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 3))).Return(scheduleDay);
                Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
                Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
                Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
            }

            var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder,null,_resourceOptimizationHelper,null,null));
            Assert.IsTrue(result.IsValid);
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

		    var validateUnderStaffingSkillDay1 = StaffingThresholdValidator.ValidateUnderstaffing(_skill,
		                                                                                          new List<ISkillStaffPeriod>
		                                                                                              {
		                                                                                                  skillDay1
		                                                                                              .SkillStaffPeriodCollection
		                                                                                              [0]
		                                                                                              }, _person);

		    var validatedUnderStaffingSkillDay2 = StaffingThresholdValidator.ValidateUnderstaffing(_skill,
		                                                                                           new List<ISkillStaffPeriod>
		                                                                                               {
		                                                                                                   skillDay2
		                                                                                               .SkillStaffPeriodCollection
		                                                                                               [0]
		                                                                                               }, _person);

            Assert.IsTrue(validateUnderStaffingSkillDay1.IsValid);
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
		                                                                                  _person);

			Assert.IsFalse(validatedUnderStaffing.IsValid);
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
                                                                                          _person);

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
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
            IScheduleRange range = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay scheduleDay = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var filteredVisualLayers = _mocks.StrictMock<IFilteredVisualLayerCollection>();
            IVisualLayer visualLayer = _mocks.StrictMock<IVisualLayer>();
            IList<IVisualLayer> visualLayers = new List<IVisualLayer> { visualLayer };

            _validatedRequest.IsValid = true;
            _validatedRequest.ValidationErrors = "";

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
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 01), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 02), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 02), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 03), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 03), true, true);
                _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2010, 02, 04), true, true);
                Expect.Call(_dictionary[absenceRequest.Person]).Return(range).Repeat.AtLeastOnce();
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 1))).Return(scheduleDay);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 2))).Return(scheduleDay);
                Expect.Call(range.ScheduledDay(new DateOnly(2010, 2, 3))).Return(scheduleDay);
                Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.FilterLayers(absence)).Return(filteredVisualLayers).Repeat.AtLeastOnce();
                Expect.Call(filteredVisualLayers.GetEnumerator()).Return(visualLayers.GetEnumerator()).Repeat.AtLeastOnce();
                Expect.Call(visualLayer.Period).Return(requestedDateTimePeriod).Repeat.AtLeastOnce();
            }

            var result = _target.Validate(absenceRequest, new RequiredForHandlingAbsenceRequest(stateHolder,null,_resourceOptimizationHelper,null,null));
            Assert.IsTrue(result.IsValid);
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void ShouldThrowExceptionIfSkillStaffPeriodListIsNull()
        {
            var skill = SkillFactory.CreateSkill("test");
            StaffingThresholdValidator.ValidateSeriousUnderstaffing(skill, null, _person);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfSkillStaffPeriodListArgumentIsNull()
        {
            var skill = SkillFactory.CreateSkill("test");
            StaffingThresholdValidator.ValidateUnderstaffing(skill, null, _person);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyUnderstaffingDateString()
        {
            var underStaffDict = new UnderStaffingData();
            underStaffDict.UnderStaffingDates = new Dictionary<string, IList<string>>();
            underStaffDict.UnderStaffingDates.Add("UnderStaffing", new List<string>{"2012-12-01, 2012-12-02, 2012-12-03,2012-12,04,2012-12-05,2012-12-06"});
            underStaffDict.UnderStaffingDates.Add("SeriousUnderStaffing", new List<string> { "2012-12-01, 2012-12-02,2012-12-03,2012-12,04,2012-12-05,2012-12-06" });

            var target = new StaffingThresholdValidator();
            var result = target.GetUnderStaffingDateString(underStaffDict, new CultureInfo(1033));

            Assert.IsNotNullOrEmpty(result);
        }

        [Test]
        public void VerifyUnderstaffingHourString()
        {
            var underStaffDict = new UnderStaffingData();
            underStaffDict.UnderStaffingHours = new Dictionary<string, IList<string>>();
            underStaffDict.UnderStaffingHours.Add("UnderStaffingHours", new List<string> { "10:00-10:15, 10:15-10:30, 10:30-10:45, 10:45-11:00, 11:00-11:15, 11:15-11:30" });
            underStaffDict.UnderStaffingHours.Add("SeriousUnderStaffingHours", new List<string> { "10:00-10:15, 10:15-10:30, 10:30-10:45, 10:45-11:00, 11:00-11:15, 11:15-11:30" });

            var target = new StaffingThresholdValidator();
            var result = target.GetUnderStaffingHourString(underStaffDict, new CultureInfo(1033),_person.PermissionInformation.DefaultTimeZone(), new DateTime(2012,01,01));

            Assert.IsNotNullOrEmpty(result);
        }
        
     }
}
