using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
	[TestFixture]
	public class PersonTimeZonePresenterTest
	{
		private PersonTimeZonePresenter _presenter;
		private IPersonTimeZoneView _view;
		private MockRepository _mock;
		private IList<IPerson> _persons;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IPersonTimeZoneView>();
			_persons = new List<IPerson>();
			_presenter = new PersonTimeZonePresenter(_view, _persons);	
		}

		[Test]
		public void ShouldCancel()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _view.Cancel());
			}

			using (_mock.Playback())
			{
				_presenter.OnButtonAdvCancelClick();	
			}				
		}

		[Test]
		public void ShouldSetTimeZone()
		{
			using (_mock.Record())
			{
				_view.SetPersonTimeZone(_persons);
				_view.ResetDateOnly(_persons);
				_view.HideDialog();
			}

			using (_mock.Playback())
			{
				_presenter.OnButtonAdvOkClick();
			}
		}
	}
}
