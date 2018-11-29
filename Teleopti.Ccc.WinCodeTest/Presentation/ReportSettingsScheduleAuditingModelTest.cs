using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportSettingsScheduleAuditingModelTest
    {
        private ReportSettingsScheduleAuditingModel _target;
        private MockRepository _mocks;
        private IPerson _person;
        private IPerson _otherGuy;
        private DateOnlyPeriod _dateOnlyPeriod;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _person = _mocks.StrictMock<IPerson>();
            _otherGuy = _mocks.StrictMock<IPerson>();
            _dateOnlyPeriod = new DateOnlyPeriod(2011, 10, 10, 2011, 10, 11);
            _target = new ReportSettingsScheduleAuditingModel();
        }

        [Test]
        public void ShouldAddModifier()
        {
            _target.AddModifier(_person);
  
            Assert.AreEqual(1, _target.ModifiedBy.Count);
            Assert.AreEqual(_person, _target.ModifiedBy.First());
        }

        [Test]
        public void ShouldNotAddModifierMoreThanOnce()
        {
            _target.AddModifier(_person);
            _target.AddModifier(_person);

            Assert.AreEqual(1, _target.ModifiedBy.Count);
        }

        [Test]
        public void ShouldRemoveModifier()
        {
            _target.AddModifier(_person);
            _target.RemoveModifier(_person);

            Assert.IsTrue(_target.ModifiedBy.Count == 0);
        }

        [Test]
        public void ShouldGetSetChangePeriod()
        {
            _target.ChangePeriod = _dateOnlyPeriod;

            Assert.AreEqual(_dateOnlyPeriod, _target.ChangePeriod);
        }

        [Test]
        public void ShouldGetSetSchedulePeriod()
        {
            _target.SchedulePeriod = _dateOnlyPeriod;

            Assert.AreEqual(_dateOnlyPeriod, _target.SchedulePeriod);
        }

        [Test]
        public void ShouldGetSetChangePeriodDisplay()
        {
            _target.ChangePeriodDisplay = _dateOnlyPeriod;

            Assert.AreEqual(_dateOnlyPeriod, _target.ChangePeriodDisplay);
        }

        [Test]
        public void ShouldGetSetSchedulePeriodDisplay()
        {
            _target.SchedulePeriodDisplay = _dateOnlyPeriod;

            Assert.AreEqual(_dateOnlyPeriod, _target.SchedulePeriodDisplay);
        }

        [Test]
        public void ShouldAddAgent()
        {
            _target.AddAgent(_person);

            Assert.AreEqual(1, _target.Agents.Count);
            Assert.AreEqual(_person, _target.Agents.First());
        }

        [Test]
        public void ShouldNotAddAgentMoreThanOnce()
        {
            _target.AddAgent(_person);
            _target.AddAgent(_person);

            Assert.AreEqual(1, _target.Agents.Count);    
        }

        [Test]
        public void ShouldRemoveAgent()
        {
            _target.AddAgent(_person);
            _target.RemoveAgent(_person);

            Assert.IsTrue(_target.Agents.Count == 0);  
        }

        [Test]
        public void ShouldReturnAgentNamesSortedSeparatedByComma()
        {
            _target.AddAgent(_person);
            _target.AddAgent(_otherGuy);
            var personName = new Name("personFirst", "personLast");
            var otherGuyName = new Name("otherGuyFirst", "otherGuyLast");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0},", otherGuyName.ToString(NameOrderOption.FirstNameLastName));
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", personName.ToString(NameOrderOption.FirstNameLastName));
            
            using(_mocks.Record())
            {
                Expect.Call(_person.Name).Return(personName).Repeat.AtLeastOnce();
                Expect.Call(_otherGuy.Name).Return(otherGuyName).Repeat.AtLeastOnce();
            }

            using(_mocks.Playback())
            {
                var agentString = _target.AgentsNameCommaSeparated();
                Assert.AreEqual(stringBuilder.ToString(), agentString);
            }
        }

        [Test]
        public void ShouldReturnModifiedBynamesSortedSeparatedByComma()
        {
            _target.AddModifier(_person);
            _target.AddModifier(_otherGuy);
            var personName = new Name("personFirst", "personLast");
            var otherGuyName = new Name("otherGuyFirst", "otherGuyLast");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0},", otherGuyName.ToString(NameOrderOption.FirstNameLastName));
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", personName.ToString(NameOrderOption.FirstNameLastName));
            
            using (_mocks.Record())
            {
                Expect.Call(_person.Name).Return(personName).Repeat.AtLeastOnce();
                Expect.Call(_otherGuy.Name).Return(otherGuyName).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                var modifiedByString = _target.ModifiedByNameCommaSeparated();
                Assert.AreEqual(stringBuilder.ToString(), modifiedByString);
            }
        }

        [Test]
        public void ShouldTruncateLongString()
        {
            for(var i = 0; i < 20; i++)
            {
                var person = new Person().WithName(new Name("01234567890123456789", "01234567890123456789"));
                _target.AddModifier(person);
                _target.AddAgent(person);
            }

            var modifiedByString = _target.ModifiedByNameCommaSeparated();
            var agentString = _target.AgentsNameCommaSeparated();

            Assert.IsTrue(modifiedByString.Length < 650);
            Assert.AreEqual("...", modifiedByString.Substring(modifiedByString.Length - 3));

            Assert.IsTrue(agentString.Length < 650);
            Assert.AreEqual("...", agentString.Substring(agentString.Length - 3));
        }

        [Test]
        public void ShouldReturnEmptyStringWhenNoAgentsOrModifier()
        {
            Assert.AreEqual(string.Empty, _target.AgentsNameCommaSeparated());
            Assert.AreEqual(string.Empty, _target.ModifiedByNameCommaSeparated());
        }
    }
}
