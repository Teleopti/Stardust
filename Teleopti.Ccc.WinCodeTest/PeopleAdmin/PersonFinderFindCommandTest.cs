using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class PersonFinderFindCommandTest
    {
        private PersonFinderFindCommand _target;
        private IPersonFinderModel _model;
        private MockRepository _mocks;
        private IPersonFinderSearchCriteria _personFinderSearchCritera;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _model = _mocks.StrictMock<IPersonFinderModel>();
            _personFinderSearchCritera = _mocks.StrictMock<IPersonFinderSearchCriteria>();
            _target = new PersonFinderFindCommand(_model);
        }

        [Test]
        public void ShouldNotBeAbleToExecuteWhenNoSearchValue()
        {
            using(_mocks.Record())
            {
                Expect.Call(_model.SearchCriteria).Return(_personFinderSearchCritera);
                Expect.Call(_personFinderSearchCritera.SearchCriterias).Return(new Dictionary<PersonFinderField, string>());
            }

            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.CanExecute());
            }
        }

        [Test]
        public void ShouldBeAbleToExecuteWhenSearchValue()
        {
            using (_mocks.Record())
            {
                Expect.Call(_model.SearchCriteria).Return(_personFinderSearchCritera);
				Expect.Call(_personFinderSearchCritera.SearchCriterias).Return(new Dictionary<PersonFinderField, string> { { PersonFinderField.All, "aa" } });
            }

            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.CanExecute());
            }
        }

        [Test]
        public void ShouldFind()
        {
            using(_mocks.Record())
            {
                Expect.Call(_model.Find);
            }

            using(_mocks.Playback())
            {
                _target.Execute();   
            }
        }
    }
}
