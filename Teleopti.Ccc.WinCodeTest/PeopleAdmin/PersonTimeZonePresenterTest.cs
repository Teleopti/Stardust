using System;
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
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IPersonTimeZoneView>();
			_person = _mock.StrictMock<IPerson>();
			_persons = new List<IPerson>{_person};
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
				_view.SetPersonsTimeZone(_persons, TimeZoneInfo.Utc);
			}

			using (_mock.Playback())
			{
				_presenter.OnButtonAdvOkClick(TimeZoneInfo.Utc);
			}
		}
	}
}
