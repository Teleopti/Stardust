using System;
using System.Drawing;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Absence=Domain.Absence;
using ShiftCategory=Domain.ShiftCategory;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class PreferenceMapperTest : MapperTest<AgentDayPreference>
    {
        private PreferenceMapper _target;
        private bool _veto;
        private TimeSpan? _eariliestEnd;
        private TimeSpan? _latestEnd;
        private TimeSpan? _eariliestStart;
        private TimeSpan? _latestStart;
        private TimeSpan? _maxWorkTime;
        private TimeSpan? _minWorkTime;
        private DateOnly _preferenceDate;
        private Absence _preferenceAbsence;
        private ShiftCategory _preferenceShiftCategory;
        private Agent _agent;
        private AgentDayPreference _oldPreference;
        private ObjectPairCollection<Agent, IPerson> _agentPairList;
        private ObjectPairCollection<Absence, IAbsence> _absencePairList;
        private ObjectPairCollection<ShiftCategory, IShiftCategory> _shiftCatPairList;
        private ObjectPairCollection<Absence, IDayOffTemplate> _dayOffPairList;
        private IPerson _person;
        private IAbsence _absence;
        private IShiftCategory _shiftCategory;
        private IDayOffTemplate _dayOff;
        private readonly MappedObjectPair _mappedObject = new MappedObjectPair();

        protected override int NumberOfPropertiesToConvert
        {
            get { return 11; }
        }

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _dayOff = DayOffFactory.CreateDayOff();
            _person = PersonFactory.CreatePerson("nyKalle", "Stropp");
            _absence = AbsenceFactory.CreateAbsence("DayOff");
            _shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Natt");
            _agentPairList = new ObjectPairCollection<Agent, IPerson>();
            _absencePairList = new ObjectPairCollection<Absence, IAbsence>();
            _shiftCatPairList = new ObjectPairCollection<ShiftCategory, IShiftCategory>();
            _dayOffPairList = new ObjectPairCollection<Absence, IDayOffTemplate>();
            _agent = new Agent(1, "Kalle", "Stropp", "", "", null, null, null, "");
            _agentPairList.Add(_agent, _person);
            _preferenceAbsence = new Absence(1, "Free", "DO", true, Color.YellowGreen, true, true, false, false, false, null);
            _absencePairList.Add(_preferenceAbsence, _absence);
            _preferenceShiftCategory = new ShiftCategory(1, "Natt", "NA", Color.DodgerBlue, true, true, 1);
            _shiftCatPairList.Add(_preferenceShiftCategory, _shiftCategory);
            _dayOffPairList.Add(_preferenceAbsence, _dayOff);
            _oldPreference = GetOldPreference();
            _mappedObject.ShiftCategory = _shiftCatPairList;
            _mappedObject.Absence = _absencePairList;
            _mappedObject.Agent = _agentPairList;
            _mappedObject.DayOff = _dayOffPairList;
            _target = new PreferenceMapper(_mappedObject, new CccTimeZoneInfo(TimeZoneInfo.Utc)); 
        }

        /// <summary>
        /// Verifies that a correct mapping between 6x object and Raptor object i performed
        /// </summary>
        [Test]
        public void CanMapPreference()
        {

            IPreferenceDay personRestriction = _target.Map(_oldPreference);

            Assert.AreEqual(_preferenceDate, personRestriction.RestrictionDate);
            Assert.AreEqual(string.Concat("ny", _agent.FirstName), personRestriction.Person.Name.FirstName);
            Assert.AreEqual(_eariliestEnd, personRestriction.Restriction.EndTimeLimitation.StartTime);
            Assert.AreEqual(_latestEnd, personRestriction.Restriction.EndTimeLimitation.EndTime);
            Assert.AreEqual(_eariliestStart, personRestriction.Restriction.StartTimeLimitation.StartTime);
            Assert.AreEqual(_latestStart, personRestriction.Restriction.StartTimeLimitation.EndTime);
            Assert.AreEqual(_minWorkTime, personRestriction.Restriction.WorkTimeLimitation.StartTime);
            Assert.AreEqual(_maxWorkTime, personRestriction.Restriction.WorkTimeLimitation.EndTime);
            Assert.AreEqual(_shiftCategory, (personRestriction.Restriction).ShiftCategory);

            Assert.IsNotNull((personRestriction.Restriction).DayOffTemplate);
        }


        [Test]
        public void DoNotConvertWhenCountRulesIsTrue()
        {
            _oldPreference.PreferenceAbsence = new Absence(1, "Free", "DO", true, Color.YellowGreen, true, false, false, false, false, null);
            IPreferenceDay personRestriction = _target.Map(_oldPreference);
            Assert.IsNull((personRestriction.Restriction).DayOffTemplate);
        }

        private AgentDayPreference GetOldPreference()
        {
            _veto = false;
            _eariliestEnd = new TimeSpan(15, 0, 0);
            _latestEnd = new TimeSpan(17, 0, 0);
            _eariliestStart = new TimeSpan(8, 0, 0);
            _latestStart = new TimeSpan(10, 0, 0);
            _maxWorkTime = new TimeSpan(9, 0, 0);
            _minWorkTime = new TimeSpan(5, 0, 0);
            _preferenceDate = new DateOnly(2008, 12, 24);

            var oldPreference = new AgentDayPreference(_veto)
                                    {
                                        PreferenceAbsence = _preferenceAbsence,
                                        ShiftCategory = _preferenceShiftCategory,
                                        Agent = _agent,
                                        PreferenceDate = _preferenceDate.Date
                                    };
            oldPreference.EariliestStart = _eariliestStart;
            oldPreference.EariliestEnd = _eariliestEnd;
            oldPreference.LatestEnd = _latestEnd;
            oldPreference.LatestStart = _latestStart;
            oldPreference.MaxWorkTime = _maxWorkTime;
            oldPreference.MinWorkTime = _minWorkTime;
            oldPreference.Agent = _agent;
            return oldPreference;
        }
    }
}
