using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class AddressBookPresenterTest
    {
        private ContactPersonViewModel _person1;
        private ContactPersonViewModel _person2;
        private AddressBookPresenter _target;
        private AddressBookViewModel _addressBookViewModel;
        private DateOnly _startDate;
        private MockRepository _mocks;
        private IAddressBookView _addressBookView;
	    private Guid _guid1;
	    private Guid _guid2;

        [SetUp]
        public void Setup()
        {
	        _guid1 = Guid.NewGuid();
	        _guid2 = Guid.NewGuid();
            _mocks = new MockRepository();
            _addressBookView = _mocks.StrictMock<IAddressBookView>();
            _person1 = new ContactPersonViewModel(PersonFactory.CreatePerson("required","1"));
            _person2 = new ContactPersonViewModel(PersonFactory.CreatePerson("optional","2"));
			_person1.SetId(_guid1);
			_person2.SetId(_guid2);
            _startDate = new DateOnly(2009, 10, 15);
            _addressBookViewModel = new AddressBookViewModel(new List<ContactPersonViewModel> {_person1, _person2},
                                                             new List<ContactPersonViewModel> {_person1},
                                                             new List<ContactPersonViewModel> {_person2});
            _target = new AddressBookPresenter(_addressBookView, _addressBookViewModel, _startDate);
        }

        [Test]
        public void VerifyInitialize()
        {
            Assert.AreEqual(_addressBookViewModel, _target.AddressBookViewModel);
            _addressBookView.SetCurrentDate(_startDate);
            _addressBookView.SetRequiredParticipants(_addressBookViewModel.RequiredParticipants);
            _addressBookView.SetOptionalParticipants(_addressBookViewModel.OptionalParticipants);
            _addressBookView.PrepareGridView(_addressBookViewModel.PersonModels);
            _mocks.ReplayAll();
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifySetCurrentDate()
        {
            _addressBookView.PerformSearch();
            _mocks.ReplayAll();
            _target.SetCurrentDate(_startDate.AddDays(2));
            Assert.AreEqual(_startDate.AddDays(2), _addressBookViewModel.PersonModels[0].CurrentDate);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyIsVisible()
        {
            _person1.ContainedEntity.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(_startDate));
            _person1.CurrentDate = _startDate.AddDays(1);
            Assert.IsFalse(_target.IsVisible(_person1)); //Because application function is null
            Assert.IsFalse(_target.IsVisible(_person2));
        }

        [Test]
        public void VerifyCanAddRequiredParticipants()
        {
            _addressBookView.SetRequiredParticipants("required 1");
            _addressBookView.SetRequiredParticipants("required 1; optional 2");

            _mocks.ReplayAll();
            _target.AddRequiredParticipants(new List<ContactPersonViewModel> { _person1 });
            Assert.AreEqual("required 1", _target.AddressBookViewModel.RequiredParticipants);
			_target.AddRequiredParticipants(new List<ContactPersonViewModel> { _person2 });
			Assert.AreEqual("required 1; optional 2", _target.AddressBookViewModel.RequiredParticipants);
             _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanAddOptionalParticipants()
        {
            _addressBookView.SetOptionalParticipants("optional 2");
            _addressBookView.SetOptionalParticipants("optional 2; required 1");

            _mocks.ReplayAll();
            _target.AddOptionalParticipants(new List<ContactPersonViewModel> { _person2 });
            Assert.AreEqual("optional 2", _target.AddressBookViewModel.OptionalParticipants);
            _target.AddOptionalParticipants(new List<ContactPersonViewModel> { _person1 });
            Assert.AreEqual("optional 2; required 1", _target.AddressBookViewModel.OptionalParticipants);
             _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotAddNullsToAddressBookViewModel()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _addressBookView.SetRequiredParticipants(_person1.FullName));
                Expect.Call(() => _addressBookView.SetOptionalParticipants(_person2.FullName));
            }

            using (_mocks.Playback())
            {
                _target.AddRequiredParticipants(new List<ContactPersonViewModel> {null});
                _target.AddOptionalParticipants(new List<ContactPersonViewModel>{null});
                Assert.AreEqual(1, _target.AddressBookViewModel.RequiredParticipantList.Count);
                Assert.AreEqual(1, _target.AddressBookViewModel.OptionalParticipantList.Count);
            }
        }

		[Test]
		public void ShouldHandleRequiredAgentsWithSameName()
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();

			_person1 = new ContactPersonViewModel(PersonFactory.CreatePerson("firstName", "lastName"));
			_person1.SetId(guid1);
			_person2 = new ContactPersonViewModel(PersonFactory.CreatePerson("firstName", "lastName"));
			_person2.SetId(guid2);
			_addressBookViewModel.RequiredParticipantList.Clear();

			using (_mocks.Record())
			{
				Expect.Call(() => _addressBookView.SetRequiredParticipants("firstName lastName"));
				Expect.Call(() => _addressBookView.SetRequiredParticipants("firstName lastName; firstName lastName"));	
			}

			using (_mocks.Playback())
			{
				_target.AddRequiredParticipants(new List<ContactPersonViewModel> { _person1 });
				_target.AddRequiredParticipants(new List<ContactPersonViewModel> { _person2 });

				Assert.AreEqual(2, _target.AddressBookViewModel.RequiredParticipantList.Count);
				Assert.AreEqual(guid1, _target.AddressBookViewModel.RequiredParticipantList[0].Id);
				Assert.AreEqual(guid2, _target.AddressBookViewModel.RequiredParticipantList[1].Id);
			}
		}

		[Test]
		public void ShouldHandleOptionalAgentsWithSameName()
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();

			_person1 = new ContactPersonViewModel(PersonFactory.CreatePerson("firstName", "lastName"));
			_person1.SetId(guid1);
			_person2 = new ContactPersonViewModel(PersonFactory.CreatePerson("firstName", "lastName"));
			_person2.SetId(guid2);
			_addressBookViewModel.OptionalParticipantList.Clear();

			using (_mocks.Record())
			{
				Expect.Call(() => _addressBookView.SetOptionalParticipants("firstName lastName"));
				Expect.Call(() => _addressBookView.SetOptionalParticipants("firstName lastName; firstName lastName"));
			}

			using (_mocks.Playback())
			{
				_target.AddOptionalParticipants(new List<ContactPersonViewModel> { _person1 });
				_target.AddOptionalParticipants(new List<ContactPersonViewModel> { _person2 });

				Assert.AreEqual(2, _target.AddressBookViewModel.OptionalParticipantList.Count);
				Assert.AreEqual(guid1, _target.AddressBookViewModel.OptionalParticipantList[0].Id);
				Assert.AreEqual(guid2, _target.AddressBookViewModel.OptionalParticipantList[1].Id);
			}
		}

		[Test]
		public void ShouldRemoveRequiredFromIndex()
		{
			var allSelectedIndex = new List<int> {0};

			using (_mocks.Record())
			{
				Expect.Call(() => _addressBookView.SetRequiredParticipants(""));
			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(1, _target.AddressBookViewModel.RequiredParticipantList.Count);
				_target.RemoveIndexesRequired(allSelectedIndex);
				Assert.AreEqual(0, _target.AddressBookViewModel.RequiredParticipantList.Count);
			}
		}

		[Test]
		public void ShouldRemoveOptionalFromIndex()
		{
			var allSelectedIndex = new List<int> {0};

			using (_mocks.Record())
			{
				Expect.Call(() => _addressBookView.SetOptionalParticipants(""));
			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(1, _target.AddressBookViewModel.OptionalParticipantList.Count);
				_target.RemoveIndexesOptional(allSelectedIndex);
				Assert.AreEqual(0, _target.AddressBookViewModel.OptionalParticipantList.Count);
			}
		}
    }
}
