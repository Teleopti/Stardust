using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
	[TestFixture]
	public class PersonFinderFindCommandTest
	{
		private PersonFinderFindCommand _target;
		private IPersonFinderModel _model;
		private MockRepository _mocks;
		private IPeoplePersonFinderSearchCriteria _personFinderSearchCritera;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_model = _mocks.StrictMock<IPersonFinderModel>();
			_personFinderSearchCritera = _mocks.StrictMock<IPeoplePersonFinderSearchCriteria>();
			_target = new PersonFinderFindCommand(_model);
		}

		[Test]
		public void ShouldNotBeAbleToExecuteWhenNoSearchValue()
		{
			using (_mocks.Record())
			{
				Expect.Call(_model.SearchCriteria).Return(_personFinderSearchCritera);
				Expect.Call(_personFinderSearchCritera.SearchValue).Return(string.Empty);
			}

			using (_mocks.Playback())
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
				Expect.Call(_personFinderSearchCritera.SearchValue).Return("searchValue");
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.CanExecute());
			}
		}

		[Test]
		public void ShouldFind()
		{
			using (_mocks.Record())
			{
				Expect.Call(_model.Find);
			}

			using (_mocks.Playback())
			{
				_target.Execute();
			}
		}
	}
}
