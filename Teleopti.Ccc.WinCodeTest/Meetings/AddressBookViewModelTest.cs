using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class AddressBookViewModelTest
    {
        private AddressBookViewModel _target;
        private CommonNameDescriptionSetting _commonNameSetting;
        private ContactPersonViewModel _requiredPerson1;
        private ContactPersonViewModel _optionalPerson1;
        private ContactPersonViewModel _requiredPerson2;
        private ContactPersonViewModel _optionalPerson2;
        
        [SetUp]
        public void Setup()
        {
            _commonNameSetting = new CommonNameDescriptionSetting("{LastName}, {FirstName}");
            _requiredPerson1 = new ContactPersonViewModel(PersonFactory.CreatePerson("required", "1"), _commonNameSetting);
            _requiredPerson2 = new ContactPersonViewModel(PersonFactory.CreatePerson("required", "2"), _commonNameSetting);
            _optionalPerson1 = new ContactPersonViewModel(PersonFactory.CreatePerson("optional", "3"), _commonNameSetting);
            _optionalPerson2 = new ContactPersonViewModel(PersonFactory.CreatePerson("optional", "4"), _commonNameSetting);
            
            _target = new AddressBookViewModel(new List<ContactPersonViewModel> {_requiredPerson1, _optionalPerson1, _requiredPerson2, _optionalPerson2},
                                               new List<ContactPersonViewModel> {_requiredPerson1},
                                               new List<ContactPersonViewModel> {_optionalPerson1});
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(4,_target.PersonModels.Count);
            Assert.AreEqual("1, required", _target.RequiredParticipants);
            Assert.AreEqual(1, _target.RequiredParticipantList.Count);
            Assert.AreEqual("3, optional", _target.OptionalParticipants);
            Assert.AreEqual(1, _target.OptionalParticipantList.Count);
        }

		[Test]
		public void ShouldNotBePossibleToAddAlreadyExistingPersonToRequired()
		{
			var guid = Guid.NewGuid();
			var person1 = PersonFactory.CreatePerson("firstName", "lastName");
			person1.SetId(guid);
			var contactPerson1 = new ContactPersonViewModel(person1);
			var contactPerson2 = new ContactPersonViewModel(person1);
			
			_target.RequiredParticipantList.Clear();
			_target.AddRequiredParticipant(contactPerson1);
			_target.AddRequiredParticipant(contactPerson2);

			Assert.AreEqual(1, _target.RequiredParticipantList.Count);
		}

		[Test]
		public void ShouldNotBePossibleToAddAlreadyExistingPersonToOptional()
		{
			var guid = Guid.NewGuid();
			var person1 = PersonFactory.CreatePerson("firstName", "lastName");
			person1.SetId(guid);
			var contactPerson1 = new ContactPersonViewModel(person1);
			var contactPerson2 = new ContactPersonViewModel(person1);

			_target.OptionalParticipantList.Clear();
			_target.AddOptionalParticipant(contactPerson1);
			_target.AddOptionalParticipant(contactPerson2);

			Assert.AreEqual(1, _target.OptionalParticipantList.Count);
		}
    }
}
