using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class PersonFinderPreviousCommandTest
    {
        private PersonFinderPreviousCommand _target;
        private IPersonFinderModel _model;
        private MockRepository _mocks;
		  private IPeoplePersonFinderSearchCriteria _personFinderSearchCritera;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _model = _mocks.StrictMock<IPersonFinderModel>();
				_personFinderSearchCritera = _mocks.StrictMock<IPeoplePersonFinderSearchCriteria>();
            _target = new PersonFinderPreviousCommand(_model);
        }

        [Test]
        public void ShouldNotBeAbleToExecuteWhenNoPreviousPages()
        {
            using(_mocks.Record())
            {
                Expect.Call(_model.SearchCriteria).Return(_personFinderSearchCritera);
                Expect.Call(_personFinderSearchCritera.CurrentPage).Return(1);
            }

            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.CanExecute());
            }
        }

        [Test]
        public void ShouldBeAbleToExecuteWhenPreviousPages()
        {
            using(_mocks.Record())
            {
                Expect.Call(_model.SearchCriteria).Return(_personFinderSearchCritera);
                Expect.Call(_personFinderSearchCritera.CurrentPage).Return(2);
            }

            using(_mocks.Playback())
            {
                Assert.IsTrue(_target.CanExecute());
            }
        }

        [Test]
        public void ShouldFind()
        {
            using (_mocks.Record())
            {
                Expect.Call(_model.SearchCriteria).Return(_personFinderSearchCritera);
                Expect.Call(_personFinderSearchCritera.CurrentPage).Return(2);
                Expect.Call(() => _personFinderSearchCritera.CurrentPage = 1);
                Expect.Call(_model.Find);
            }

            using (_mocks.Playback())
            {
                _target.Execute();
            }
        }
    }
}
