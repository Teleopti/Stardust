using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class PublicNoteTest
    {
        private IPublicNote _target;
        private IPerson _person;
        private DateOnly _noteDate;
        private IScenario _scenario;
        private string _scheduleNote;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("Kalle");
            _noteDate = new DateOnly(2010, 4, 1);
            _scenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
            _scheduleNote = "Kalle got sick";
            _target = new PublicNote(_person, _noteDate, _scenario, _scheduleNote);
        }

        [Test]
        public void VerifyInstanceCanBeCreated()
        {
            Assert.IsNotNull(_target);
        }
        [Test]
        public void VerifyProtectedConstructorExists()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void VerifyProperties()
        {
            DateTime dateTime = TimeZoneHelper.ConvertToUtc(_noteDate.Date, _person.PermissionInformation.DefaultTimeZone());
            DateTimePeriod expectedPeriod = new DateTimePeriod(dateTime, dateTime.AddDays(1));
            Assert.AreEqual(_person, _target.Person);
            Assert.AreEqual(expectedPeriod, _target.Period);
            Assert.AreEqual(_scenario, _target.Scenario);
			Assert.AreEqual(_scheduleNote, _target.GetScheduleNote(new NoFormatting()));
            Assert.AreEqual(_person, _target.MainRoot);
            Assert.AreEqual(_noteDate, _target.NoteDate);
        }

        [Test]
        public void CanAppendNote()
        {
            string text = "And he went home";
            string extectedText = string.Concat(_scheduleNote, " ", text);
            _target.AppendScheduleNote(text);
			Assert.AreEqual(extectedText, _target.GetScheduleNote(new NoFormatting()));
        }

        [Test]
        public void CanClearNote()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_scheduleNote));
			Assert.AreEqual(_scheduleNote, _target.GetScheduleNote(new NoFormatting()));
            _target.ClearScheduleNote();
			Assert.AreEqual(string.Empty, _target.GetScheduleNote(new NoFormatting()));
        }

        [Test]
        public void VerifyBelongsToPeriod()
        {
            Assert.IsTrue(_target.BelongsToPeriod(new DateOnlyAsDateTimePeriod(_noteDate, _person.PermissionInformation.DefaultTimeZone())));
            Assert.IsFalse(
                _target.BelongsToPeriod(new DateOnlyAsDateTimePeriod(_noteDate.AddDays(-1),
                                                                     _person.PermissionInformation.DefaultTimeZone())));
        }

        [Test]
        public void VerifyBelongsToPeriod2()
        {
            Assert.IsTrue(_target.BelongsToPeriod(new DateOnlyPeriod(_noteDate, _noteDate)));
            Assert.IsFalse(
                _target.BelongsToPeriod(new DateOnlyPeriod(_noteDate.AddDays(2), _noteDate.AddDays(3))));
        }

        [Test]
        public void VerifyClone()
        {
            PublicNote clone = (PublicNote)_target.Clone();

            Assert.AreNotSame(clone, _target);
			Assert.AreEqual(clone.GetScheduleNote(new NoFormatting()), _target.GetScheduleNote(new NoFormatting()));
            Assert.AreEqual(clone.Period, _target.Period);
            Assert.AreEqual(clone.Person, _target.Person);
            Assert.AreEqual(clone.Scenario, _target.Scenario);
        }
        [Test]
        public void VerifyNoneEntityClone()
        {
            IPublicNote clone = _target.NoneEntityClone();

            Assert.AreNotSame(clone, _target);
			Assert.AreEqual(clone.GetScheduleNote(new NoFormatting()), _target.GetScheduleNote(new NoFormatting()));
            Assert.AreEqual(clone.Period, _target.Period);
            Assert.AreEqual(clone.Person, _target.Person);
            Assert.AreEqual(clone.Scenario, _target.Scenario);
        }

        [Test]
        public void VerifyBelongsToScenario()
        {
            _target.BelongsToScenario(_scenario);
        }

        [Test]
        public void VerifyCorrectApplicationFunctionPathIsReturned()
        {
            Assert.AreEqual(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, _target.FunctionPath);
        }

        [Test]
        public void VerifyCreateTransient()
        {
            Guid guid = Guid.NewGuid();
            _target.SetId(guid);

            Assert.AreEqual(guid, _target.Id);

            var data = _target.CreateTransient();

            Assert.IsNull(data.Id);
        }
        [Test]
        public void VerifyAppendingWorksUpTo255()
        {
            _target.ClearScheduleNote();
            string text = string.Empty;
            text = text.PadRight(255, 'x');
            _target.AppendScheduleNote(text);
        }
        [Test]
        public void VerifyExceptionIsThrownIfTextIsLongerThan255WhenAppending()
        {
            const string text = "z";
            string currentText = string.Empty;
            currentText = currentText.PadRight(255, 'x');
            _target = new PublicNote(_person, _noteDate, _scenario, currentText);
			Assert.Throws<ArgumentException>(() => _target.AppendScheduleNote(text));
        }

        [Test]
        public void VerifyExceptionIsThrownIfTextIsLongerThan255WhenCreatingNote()
        {
            string text = string.Empty;
            text = text.PadRight(256, 'x');
			Assert.Throws<ArgumentException>(() => _target = new PublicNote(_person, _noteDate, _scenario, text));
        }
        
        [Test]
        public void VerifyTextCanBe255WhenAppending()
        {
            string text = string.Empty;
            _target = new PublicNote(_person, _noteDate, _scenario, text);
            text = text.PadRight(255, 'x');
            _target.AppendScheduleNote(text);
			Assert.AreEqual(255, _target.GetScheduleNote(new NoFormatting()).Length);
        }

        [Test]
        public void VerifyCloneAndChangeParameters()
        {
            var newScenario = new Scenario("new scenario");
			string text = _target.GetScheduleNote(new NoFormatting());

            var moveToTheseParameters = new PublicNote(_target.Person, _target.NoteDate, newScenario, text);

            var newNote = _target.CloneAndChangeParameters(moveToTheseParameters);
            var castedNote = ((IPublicNote) newNote);
            Assert.IsNull(newNote.Id);
            Assert.AreSame(_target.Person, newNote.Person);
            Assert.AreNotSame(_target.Scenario, newNote.Scenario);
            Assert.AreSame(newScenario, newNote.Scenario);
            Assert.AreEqual(_target.Period, newNote.Period);
			Assert.AreEqual(_target.GetScheduleNote(new NoFormatting()), castedNote.GetScheduleNote(new NoFormatting()));
            Assert.AreEqual(_target.NoteDate, castedNote.NoteDate);
        }
    }
}
