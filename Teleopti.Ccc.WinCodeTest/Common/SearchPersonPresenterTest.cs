using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class SearchPersonPresenterTest
    {
        private SearchPersonPresenter _target;
        private ISearchPersonView _view;
        IList<IPerson> _persons;
        ICollection<IPerson> _found;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<ISearchPersonView>();
            _target = new SearchPersonPresenter(_view);
            _persons = new List<IPerson>();
        }

        [Test]
        public void VerifyOnTextBoxTextChanged()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _view.FillGridListControl()).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                _target.OnTextBox1TextChanged();
            }
        }

        [Test]
        public void VerifySearch()
        {
            _persons.Add(PersonFactory.CreatePerson("kalle"));
            _persons.Add(PersonFactory.CreatePerson("olle"));
            _target.SetSearchablePersons(_persons);
            Assert.AreEqual(1, _target.Search("al").Count);
            Assert.AreEqual(2, _target.Search("ll").Count);
        }

        [Test]
        public void VerifySearchFirstNameLastName()
        {
            IPerson person1 = PersonFactory.CreatePerson("Albert", "Persson");
            IPerson person2 = PersonFactory.CreatePerson("Jöns", "Jacobs");

            _persons.Add(person1);
            _persons.Add(person2);

            _target.SetSearchablePersons(_persons);

            _found  = _target.Search("Albert Persson");
            Assert.IsTrue(_found.Count == 1);
            Assert.IsTrue(_found.Contains(person1));

            _found = _target.Search("Jöns Jacobs");
            Assert.IsTrue(_found.Count == 1);
            Assert.IsTrue(_found.Contains(person2));

            _found = _target.Search("Albert Jacobs");
            Assert.IsTrue(_found.Count == 0);
        }

        [Test]
        public void VerifySearchLastNameFirstName()
        {
            IPerson person1 = PersonFactory.CreatePerson("Albert", "Persson");
            IPerson person2 = PersonFactory.CreatePerson("Jöns", "Jacobs");

            _persons.Add(person1);
            _persons.Add(person2);

            _target.SetSearchablePersons(_persons);

            _found = _target.Search("Jacobs Jöns");
            Assert.IsTrue(_found.Count == 1);
            Assert.IsTrue(_found.Contains(person2));

            _found = _target.Search("Albert Persson");
            Assert.IsTrue(_found.Count == 1);
            Assert.IsTrue(_found.Contains(person1));

            _found = _target.Search("Jacobs Albert");
            Assert.IsTrue(_found.Count == 0);
        }
   }
}