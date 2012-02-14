//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Teleopti.Ccc.Domain.AgentInfo;
//using Teleopti.Ccc.Domain.Collection;
//using Teleopti.Ccc.Domain.Common;
//using Teleopti.Ccc.Domain.Forecasting;
//using Teleopti.Ccc.Domain.Scheduling;
//using Teleopti.Ccc.Domain.Scheduling.Assignment;
//using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
//{
//    [TestFixture]
//    public class SeatLimitationWorkShiftCalculatorTest
//    {
//        private SeatLimitationWorkShiftCalculator _target;
//        private MockRepository _mocks;
//        private ITeam _team;
//        private ISite _site;
		
//        private IVirtualSchedulePeriod _virtualPeriod;
//        private IDictionary<ISkill, ISkillStaffPeriodDictionary> _skillStaffPeriods;
//        private VisualLayerFactory _layerFactory;
//        //private DateTime _date;
//        private IPersonPeriod _personPeriod;
//        private IList<IPersonSkill> _maxSeatSkills;
//        private Skill _skill;

//        [SetUp]
//        public void Setup()
//        {
//            //_date = new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc);
//            _skillStaffPeriods = new Dictionary<ISkill, ISkillStaffPeriodDictionary>();
//            _layerFactory = new VisualLayerFactory();
//            _mocks = new MockRepository();
//            _team = new Team();
//            _site = new Site("VH");
//            _site.AddTeam(_team);
//            _personPeriod = _mocks.StrictMock<IPersonPeriod>();
//            _skill = new Skill("MaxSeat", "", Color.Blue, 15, new SkillTypePhone(new Description(), ForecastSource.MaxSeatSkill));
//            _maxSeatSkills = new List<IPersonSkill>{new PersonSkill(_skill,new Percent(1))};
//            _virtualPeriod = _mocks.DynamicMock<IVirtualSchedulePeriod>();
//            _target = new SeatLimitationWorkShiftCalculator();
//        }

//        [Test]
//        public void ShouldReturnZeroIfPersonHasNoSeatLimitationSkill()
//        {
            
//            Expect.Call(_virtualPeriod.PersonPeriod).Return(_personPeriod);
//            Expect.Call(_personPeriod.PersonMaxSeatSkillCollection).Return(new List<IPersonSkill>());

//            _mocks.ReplayAll();
//            var result = _target.CalculateShiftValue(_virtualPeriod, new VisualLayerCollection(new Person(), new List<IVisualLayer>()), _skillStaffPeriods);
//            Assert.That(result,Is.EqualTo(0));
//            _mocks.VerifyAll();
//        }

//        [Test]
//        public void ShouldReturnZeroIfNoLayerHasActivityThatRequiresSeat()
//        {
//            var date = new DateTime(2009, 2, 2, 10, 0, 0, DateTimeKind.Utc);
			
//            IActivity activity1 = new Activity("ittan") {RequiresSeat = false};
//            IVisualLayer layer1 = _layerFactory.CreateShiftSetupLayer(activity1, new DateTimePeriod(date, date.AddMinutes(60)));

//            IList<IVisualLayer> layers = new List<IVisualLayer> { layer1 };
//            IVisualLayerCollection layerCollection = new VisualLayerCollection(null, layers);

//            Expect.Call(_virtualPeriod.PersonPeriod).Return(_personPeriod);
//            Expect.Call(_personPeriod.PersonMaxSeatSkillCollection).Return(_maxSeatSkills);

//            _mocks.ReplayAll();
//            var result = _target.CalculateShiftValue(_virtualPeriod, layerCollection, _skillStaffPeriods);
//            Assert.That(result, Is.EqualTo(0));
//            _mocks.VerifyAll();
//        }

//        //[Test]
//        //public void ShouldReturnSomethingElseThanZeroIfActivityRequiresSeat()
//        //{
//        //    var layerPeriod = new DateTimePeriod(_date, _date.AddMinutes(30));
//        //    var holderPeriod1 = new DateTimePeriod(_date, _date.AddMinutes(15));
//        //    var holderPeriod2 = new DateTimePeriod(_date.AddMinutes(15), _date.AddMinutes(30));
//        //    var holderPeriod3 = new DateTimePeriod(_date.AddMinutes(30), _date.AddMinutes(45));
            
//        //    IActivity activity1 = new Activity("ittan") { RequiresSeat = true };
//        //    IVisualLayer layer1 = _layerFactory.CreateShiftSetupLayer(activity1, layerPeriod);
//        //    IList<IVisualLayer> layers = new List<IVisualLayer> { layer1 };
//        //    IVisualLayerCollection layerCollection = new VisualLayerCollection(null, layers);

//        //    var skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
//        //    var skillStaffPeriod2 = _mocks.StrictMock<ISkillStaffPeriod>();
//        //    var skillStaffPeriod3 = _mocks.StrictMock<ISkillStaffPeriod>();

//        //    Expect.Call(_virtualPeriod.PersonPeriod).Return(_personPeriod);
//        //    Expect.Call(_personPeriod.PersonMaxSeatSkillCollection).Return(_maxSeatSkills);
//        //    Expect.Call(skillStaffPeriod1.Period).Return(holderPeriod1).Repeat.AtLeastOnce();
//        //    Expect.Call(skillStaffPeriod2.Period).Return(holderPeriod2).Repeat.AtLeastOnce();
//        //    Expect.Call(skillStaffPeriod3.Period).Return(holderPeriod3).Repeat.AtLeastOnce();

//        //    _mocks.ReplayAll();
//        //    var dictionary = new SkillStaffPeriodDictionary(_skill) { skillStaffPeriod1, skillStaffPeriod2, skillStaffPeriod3 };

//        //    _skillStaffPeriods = new Dictionary<ISkill, ISkillStaffPeriodDictionary> { { _skill, dictionary } };
			
//        //    var result = _target.CalculateShiftValue(_virtualPeriod, layerCollection, _skillStaffPeriods);
//        //    Assert.That(result, Is.EqualTo(2));
//        //    _mocks.VerifyAll();
//        //}

//        //[Test]
//        //public void ShouldCalculatePartOfPeriodIfLayerIntersectPart()
//        //{
//        //    var layerPeriod = new DateTimePeriod(_date, _date.AddMinutes(7).AddSeconds(30));
//        //    var holderPeriod1 = new DateTimePeriod(_date, _date.AddMinutes(15));
//        //    var holderPeriod2 = new DateTimePeriod(_date.AddMinutes(15), _date.AddMinutes(30));
//        //    var holderPeriod3 = new DateTimePeriod(_date.AddMinutes(30), _date.AddMinutes(45));
			
//        //    IActivity activity1 = new Activity("ittan") { RequiresSeat = true };
//        //    IVisualLayer layer1 = _layerFactory.CreateShiftSetupLayer(activity1, layerPeriod);
//        //    IList<IVisualLayer> layers = new List<IVisualLayer> { layer1 };
//        //    IVisualLayerCollection layerCollection = new VisualLayerCollection(null, layers);

//        //    var skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
//        //    var skillStaffPeriod2 = _mocks.StrictMock<ISkillStaffPeriod>();
//        //    var skillStaffPeriod3 = _mocks.StrictMock<ISkillStaffPeriod>();

//        //    Expect.Call(_virtualPeriod.PersonPeriod).Return(_personPeriod);
//        //    Expect.Call(_personPeriod.PersonMaxSeatSkillCollection).Return(_maxSeatSkills);
//        //    Expect.Call(skillStaffPeriod1.Period).Return(holderPeriod1).Repeat.AtLeastOnce();
//        //    Expect.Call(skillStaffPeriod2.Period).Return(holderPeriod2).Repeat.AtLeastOnce();
//        //    Expect.Call(skillStaffPeriod3.Period).Return(holderPeriod3).Repeat.AtLeastOnce();

//        //    _mocks.ReplayAll();
//        //    var dictionary = new SkillStaffPeriodDictionary(_skill) { skillStaffPeriod1, skillStaffPeriod2, skillStaffPeriod3 };
//        //    _skillStaffPeriods = new Dictionary<ISkill, ISkillStaffPeriodDictionary> { { _skill, dictionary } };
//        //    var result = _target.CalculateShiftValue(_virtualPeriod, layerCollection, _skillStaffPeriods);
//        //    _skillStaffPeriods = new Dictionary<ISkill, ISkillStaffPeriodDictionary> { { _skill, dictionary } };
//        //    Assert.That(result, Is.EqualTo(0.5));
//        //    _mocks.VerifyAll();
//        //}
//    }
	
		
//}