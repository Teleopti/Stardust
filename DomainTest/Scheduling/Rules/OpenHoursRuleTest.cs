﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class OpenHoursRuleTest
    {
        private OpenHoursRule _target;
        private IScheduleDay _day;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _state;
        private DateTime _date;
        private readonly DateOnly _dateOnlyDate = new DateOnly(2009,2,2);
        private IPerson _person;
        private IPersonPeriod _personPeriod;
        private IVisualLayerFactory _visualLayerFactory;
        private IScheduleRange _range;
        private IList<IScheduleDay> _days;
        private Dictionary<IPerson, IScheduleRange> _dic;
        private IPermissionInformation _permissionInformation;
        private ICccTimeZoneInfo _timeZone;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            _timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"));

            _date = new DateTime(2009, 2, 2, 11, 0, 0, DateTimeKind.Utc);
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZone);
            
            _day = _mocks.StrictMock<IScheduleDay>();
            _state = _mocks.StrictMock<ISchedulingResultStateHolder>();
            
            _person = _mocks.StrictMock<IPerson>();
            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
            _visualLayerFactory = new VisualLayerFactory();
            _range = _mocks.StrictMock<IScheduleRange>();
            _days = new List<IScheduleDay>{_day};
            _dic = new Dictionary<IPerson, IScheduleRange> {{_person, _range}};
           
            Expect.Call(_day.Person).Return(_person);
            Expect.Call(_day.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
            Expect.Call(_range.BusinessRuleResponseInternalCollection).Return(new List<IBusinessRuleResponse>());
            Expect.Call(_person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
            Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
        }

        
        [Test]
        public void VerifyValidateTrue()
        {
            
             Expect.Call(_day.HasProjection).Return(false).Repeat.AtLeastOnce();
            _mocks.ReplayAll();

            _target = new OpenHoursRule(_state);
            
             Assert.AreEqual(0, _target.Validate(_dic, _days).Count());
            
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyValidateCompleteAssignmentWithinOpenHours()
        {
        	IActivity activity = ActivityFactory.CreateActivity("adf");
			activity.RequiresSkill = true;
            var type = _mocks.StrictMock<ISkillType>();
        	ISkill skill = SkillFactory.CreateSkill("aslfm", type, 15);
			skill.Activity = activity;

        	IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
            IList<IPersonSkill> personSkills = new List<IPersonSkill>{personSkill};
            var holder = _mocks.StrictMock<ISkillStaffPeriodHolder>();

            var period = new DateTimePeriod(_date, _date.AddMinutes(30));
            IVisualLayer layer = _visualLayerFactory.CreateShiftSetupLayer(activity, period, _person);
            IList<IVisualLayer> layers = new List<IVisualLayer> {layer};
        	IVisualLayerCollection layerCollection = new VisualLayerCollection(_person, layers, new ProjectionPayloadMerger());
            
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var skillSkillStaffPeriodExtendedDictionary = _mocks.StrictMock<ISkillSkillStaffPeriodExtendedDictionary>();
            var skillStaffPeriodDictionary = _mocks.StrictMock<ISkillStaffPeriodDictionary>();
            var skillOpenHoursCollection =
                new ReadOnlyCollection<DateTimePeriod>(new List<DateTimePeriod>{period});
        
			Expect.Call(_person.Period(_dateOnlyDate)).Return(_personPeriod).Repeat.Once();
            Expect.Call(_personPeriod.PersonSkillCollection).Return(personSkills).Repeat.AtLeastOnce();
            Expect.Call(_state.SkillStaffPeriodHolder).Return(holder).Repeat.AtLeastOnce();
            Expect.Call(_day.HasProjection).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_day.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.AtLeastOnce();
            Expect.Call(holder.SkillSkillStaffPeriodDictionary).Return(skillSkillStaffPeriodExtendedDictionary).Repeat.Twice();
            Expect.Call(skillSkillStaffPeriodExtendedDictionary.Count).Return(1);
            Expect.Call(skillSkillStaffPeriodExtendedDictionary.TryGetValue(personSkill.Skill, out skillStaffPeriodDictionary)).Return(true).OutRef(skillStaffPeriodDictionary);
            Expect.Call(skillStaffPeriodDictionary.SkillOpenHoursCollection).Return(skillOpenHoursCollection);

            _mocks.ReplayAll();

            _target = new OpenHoursRule(_state);
            Assert.AreEqual(0, _target.Validate(_dic, _days).Count());
        
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyValidateActivityStartingBeforeOpenHours()
        {
        	IActivity activity = ActivityFactory.CreateActivity("adf");
			activity.RequiresSkill = true;
            var type = _mocks.StrictMock<ISkillType>();
        	ISkill skill = SkillFactory.CreateSkill("aslfm", type, 15);
			skill.Activity = activity;
        	ISkill skill1 = SkillFactory.CreateSkill("aslfm", type, 15);
			skill1.Activity = activity;

			IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			IPersonSkill personSkill1 = PersonSkillFactory.CreatePersonSkill(skill1, 1);
			
			IList<IPersonSkill> personSkills = new List<IPersonSkill> { personSkill, personSkill1 };
            var holder = _mocks.StrictMock<ISkillStaffPeriodHolder>();

            var period = new DateTimePeriod(_date, _date.AddMinutes(30));
            IVisualLayer layer = _visualLayerFactory.CreateShiftSetupLayer(activity, period, _person);
        	
            IList<IVisualLayer> layers = new List<IVisualLayer> {layer};
        	IVisualLayerCollection layerCollection = new VisualLayerCollection(_person, layers, new ProjectionPayloadMerger());

            var projectionService = _mocks.StrictMock<IProjectionService>();
            var skillSkillStaffPeriodExtendedDictionary = _mocks.StrictMock<ISkillSkillStaffPeriodExtendedDictionary>();
            var skillStaffPeriodDictionary = _mocks.StrictMock<ISkillStaffPeriodDictionary>();
            
            var skillOpenHoursCollection =
                new ReadOnlyCollection<DateTimePeriod>(new List<DateTimePeriod> { period.MovePeriod(TimeSpan.FromMinutes(15)) });
            
			Expect.Call(_person.Period(_dateOnlyDate)).Return(_personPeriod).Repeat.Once();
            Expect.Call(_personPeriod.PersonSkillCollection).Return(personSkills).Repeat.AtLeastOnce();
            Expect.Call(_state.SkillStaffPeriodHolder).Return(holder).Repeat.AtLeastOnce();
            Expect.Call(_day.HasProjection).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_day.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.AtLeastOnce();
            Expect.Call(holder.SkillSkillStaffPeriodDictionary).Return(skillSkillStaffPeriodExtendedDictionary).Repeat.Times(3);
            Expect.Call(skillSkillStaffPeriodExtendedDictionary.Count).Return(2);
            Expect.Call(skillSkillStaffPeriodExtendedDictionary.TryGetValue(personSkill.Skill, out skillStaffPeriodDictionary)).Return(true).OutRef(skillStaffPeriodDictionary);
            Expect.Call(skillSkillStaffPeriodExtendedDictionary.TryGetValue(personSkill1.Skill, out skillStaffPeriodDictionary)).Return(true).OutRef(skillStaffPeriodDictionary);
            Expect.Call(skillStaffPeriodDictionary.SkillOpenHoursCollection).Return(skillOpenHoursCollection).Repeat.Twice();

            _mocks.ReplayAll();

            _target = new OpenHoursRule(_state);
            var response = _target.Validate(_dic, _days);

            Assert.AreNotEqual(0, response.Count());

            var expectedErrorMessage = string.Format(TeleoptiPrincipal.Current.Regional.Culture,
                                           UserTexts.Resources.BusinessRuleNoSkillsOpenErrorMessage,
                                           layer.DisplayDescription(),
                                           TimeZoneHelper.ConvertFromUtc(layer.Period.StartDateTime, _timeZone),
                                           TimeZoneHelper.ConvertFromUtc(layer.Period.EndDateTime, _timeZone));

            Assert.AreEqual(expectedErrorMessage, response.First().Message);
            _mocks.VerifyAll();

        }

        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyValidateNotRequiresSkillActivityStartingBeforeOpenHours()
        {
        	IActivity activity = ActivityFactory.CreateActivity("adf");
			activity.RequiresSkill = false;
              
            var period = new DateTimePeriod(_date, _date.AddMinutes(30));
            IVisualLayer layer = _visualLayerFactory.CreateShiftSetupLayer(activity, period,_person);
            IList<IVisualLayer> layers = new List<IVisualLayer> {layer};
        	IVisualLayerCollection layerCollection = new VisualLayerCollection(_person, layers, new ProjectionPayloadMerger());
            
            var projectionService = _mocks.StrictMock<IProjectionService>();
                  
			 Expect.Call(_day.HasProjection).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_day.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.AtLeastOnce();
             _mocks.ReplayAll();

            _target = new OpenHoursRule(_state);
            Assert.AreEqual(0, _target.Validate(_dic, _days).Count());

            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyValidateActivityWithNoSkill()
        {
        	IActivity activity = ActivityFactory.CreateActivity("Activity1");
			activity.RequiresSkill = true;
            var type = _mocks.StrictMock<ISkillType>();

        	IActivity activity2 = ActivityFactory.CreateActivity("Activity2");
			activity2.RequiresSkill = false;
        	ISkill skill = SkillFactory.CreateSkill("Skill1", type, 15);
			skill.Activity = activity2;

            IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
            IList<IPersonSkill> personSkills = new List<IPersonSkill> { personSkill };
            var holder = _mocks.StrictMock<ISkillStaffPeriodHolder>();

            var period = new DateTimePeriod(_date, _date.AddMinutes(30));
            IVisualLayer layer = _visualLayerFactory.CreateShiftSetupLayer(activity, period, _person);
            IList<IVisualLayer> layers = new List<IVisualLayer> { layer };
            IVisualLayerCollection layerCollection = new VisualLayerCollection(_person, layers, new ProjectionPayloadMerger());

            var projectionService = _mocks.StrictMock<IProjectionService>();
            var skillSkillStaffPeriodExtendedDictionary = _mocks.StrictMock<ISkillSkillStaffPeriodExtendedDictionary>();
            
            Expect.Call(_person.Period(_dateOnlyDate)).Return(_personPeriod).Repeat.Once();
            Expect.Call(_personPeriod.PersonSkillCollection).Return(personSkills).Repeat.AtLeastOnce();
            Expect.Call(_state.SkillStaffPeriodHolder).Return(holder).Repeat.AtLeastOnce();
            Expect.Call(_day.HasProjection).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_day.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.AtLeastOnce();
            Expect.Call(holder.SkillSkillStaffPeriodDictionary).Return(skillSkillStaffPeriodExtendedDictionary);
            Expect.Call(skillSkillStaffPeriodExtendedDictionary.Count).Return(1);
            
            _mocks.ReplayAll();

            _target = new OpenHoursRule(_state);
            Assert.AreEqual(1, _target.Validate(_dic, _days).Count());

            _mocks.VerifyAll();
        }

        [Test]
        public void WhenLayerCollectionIsNullOrEmptyRuleSaysOk()
        {
            IList<IVisualLayer> layers = new List<IVisualLayer>();
            IVisualLayerCollection layerCollection = new VisualLayerCollection(_person, layers, new ProjectionPayloadMerger());
            
            var projectionService = _mocks.StrictMock<IProjectionService>();
            
            Expect.Call(_day.HasProjection).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_day.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.AtLeastOnce();
            _mocks.ReplayAll();

            _target = new OpenHoursRule(_state);
            Assert.AreEqual(0, _target.Validate(_dic, _days).Count());

            _mocks.VerifyAll();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void WhenAgentHasNoSkillsRuleReportsError()
        {
        	IActivity activity = ActivityFactory.CreateActivity("adf");
			activity.RequiresSkill = true;
           IList<IPersonSkill> personSkills = new List<IPersonSkill> ();

            var period = new DateTimePeriod(_date, _date.AddMinutes(30));
            IVisualLayer layer = _visualLayerFactory.CreateShiftSetupLayer(activity, period, _person);
            IList<IVisualLayer> layers = new List<IVisualLayer> {layer};
        	IVisualLayerCollection layerCollection = new VisualLayerCollection(_person, layers, new ProjectionPayloadMerger());

            var projectionService = _mocks.StrictMock<IProjectionService>();

			Expect.Call(_person.Period(_dateOnlyDate)).Return(_personPeriod).Repeat.Once();
            Expect.Call(_personPeriod.PersonSkillCollection).Return(personSkills).Repeat.AtLeastOnce();
            Expect.Call(_day.HasProjection).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_day.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.AtLeastOnce();
           _mocks.ReplayAll();

            _target = new OpenHoursRule(_state);
            Assert.AreEqual(1, _target.Validate(_dic, _days).Count());

            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void WhenNoSkillStaffPeriodsRuleReportsError()
        {
        	IActivity activity = ActivityFactory.CreateActivity("adf");
			activity.RequiresSkill = true;
            var type = _mocks.StrictMock<ISkillType>();
        	ISkill skill = SkillFactory.CreateSkill("aslfm", type, 15);
			skill.Activity = activity;

        	IPersonSkill personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			personSkill.Active = true;
            IList<IPersonSkill> personSkills = new List<IPersonSkill>{personSkill};
            var holder = _mocks.StrictMock<ISkillStaffPeriodHolder>();

            var period = new DateTimePeriod(_date, _date.AddMinutes(30));
            IVisualLayer layer = _visualLayerFactory.CreateShiftSetupLayer(activity, period, _person);
            IList<IVisualLayer> layers = new List<IVisualLayer> {layer};
        	IVisualLayerCollection layerCollection = new VisualLayerCollection(_person, layers, new ProjectionPayloadMerger());

            var projectionService = _mocks.StrictMock<IProjectionService>();
            var skillSkillStaffPeriodExtendedDictionary = _mocks.StrictMock<ISkillSkillStaffPeriodExtendedDictionary>();

			Expect.Call(_person.Period(_dateOnlyDate)).Return(_personPeriod).Repeat.Once();
            Expect.Call(_personPeriod.PersonSkillCollection).Return(personSkills).Repeat.AtLeastOnce();
            Expect.Call(_state.SkillStaffPeriodHolder).Return(holder).Repeat.AtLeastOnce();
            Expect.Call(_day.HasProjection).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_day.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(layerCollection).Repeat.AtLeastOnce();
            Expect.Call(holder.SkillSkillStaffPeriodDictionary).Return(skillSkillStaffPeriodExtendedDictionary);
            Expect.Call(skillSkillStaffPeriodExtendedDictionary.Count).Return(0);
            _mocks.ReplayAll();

            _target = new OpenHoursRule(_state);
            Assert.AreEqual(1, _target.Validate(_dic, _days).Count());

            _mocks.VerifyAll();
        }
        [Test]
        public void CanCreateRuleAndAccessSimpleProperties()
        {
            _target = new OpenHoursRule(_state);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            // ska man kunna ändra det??
            _target.HaltModify = false;
            Assert.IsFalse(_target.HaltModify);
            Assert.AreEqual("", _target.ErrorMessage);
        }

    }
}
