using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.DomainTest.Security.ActiveDirectory
{
    [TestFixture]
    public class ActiveDirectoryUserTest
    {

        #region Variables

        private ActiveDirectoryUserTestClass _target;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _target = new ActiveDirectoryUserTestClass();
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void VerifyConstructor()
        {
            // Declare return type to hold constructor result
            ActiveDirectoryUser returnValue;

            // Instantiate object
            returnValue = new ActiveDirectoryUser();

            // Perform Assert Tests
            Assert.IsNotNull(returnValue);

        }

        #endregion

        #region Property Tests

        [Test]
        public void VerifyAccountControl()
        {
            // Declare variable to hold property set method
            int setValue = 20;

            // Test set method
            _target.AccountControl = setValue;

            // Declare return variable to hold property get method
            int getValue = 0;

            // Test get method
            getValue = _target.AccountControl;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyAssistant()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.Assistant = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.Assistant;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyCellPhone()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.CellPhone = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.CellPhone;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyChanged()
        {
            // Declare variable to hold property set method
            System.DateTime setValue = DateTime.MaxValue;

            // Test set method
            _target.Changed = setValue;

            // Declare return variable to hold property get method
            System.DateTime getValue = DateTime.MinValue;

            // Test get method
            getValue = _target.Changed;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyCity()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.City = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.City;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyCommonName()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.CommonName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.CommonName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyCompany()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.Company = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.Company;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyCreated()
        {
            // Declare variable to hold property set method
            System.DateTime setValue = DateTime.MaxValue;

            // Test set method
            _target.Created = setValue;

            // Declare return variable to hold property get method
            System.DateTime getValue = DateTime.MinValue;

            // Test get method
            getValue = _target.Created;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyDepartment()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.Department = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.Department;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyDescription()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.Description = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.Description;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyDistinguishedName()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.DistinguishedName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.DistinguishedName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
            Assert.IsFalse(string.IsNullOrEmpty(_target.Path));
        }

        [Test]
        public void VerifyEmailAddress()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.EmailAddress = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.EmailAddress;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID"), Test]
        public void VerifyEmployeeID()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.EmployeeID = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.EmployeeID;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyFaxNumber()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.FaxNumber = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.FaxNumber;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyFirstName()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.FirstName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.FirstName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyFullDisplayName()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.FullDisplayName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.FullDisplayName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyFullName()
        {
            // Declare variables
            System.String firstNameValue = "FirstName";
            System.String lastNameValue = "LastName";
            System.String expectedValue = "FirstName LastName";

            // set values
            _target.FirstName = firstNameValue;
            _target.LastName = lastNameValue;

            // Test get method
            string getValue = _target.FullName;

            // Perform Assert Tests
            Assert.AreEqual(expectedValue, getValue);

            // declare and set values
            System.String middleInitialValue = "I";
            _target.MiddleInitial = middleInitialValue;

            expectedValue = "FirstName I LastName";

            // Test get method
            getValue = _target.FullName;

            // Perform Assert Tests
            Assert.AreEqual(expectedValue, getValue);

        }

        [Test]
        public void VerifyTokenGroups()
        {

            // Test set method
            _target.TokenGroups.Add(new ActiveDirectoryGroup());
            _target.TokenGroups.Add(new ActiveDirectoryGroup());

            // Declare return variable to hold property get method
            List<ActiveDirectoryGroup> getValue;

            // Test get method
            getValue = _target.TokenGroups;

            // Perform Assert Tests
            Assert.AreEqual(2, getValue.Count);
        }

        [Test]
        public void VerifyHomeDirectory()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.HomeDirectory = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.HomeDirectory;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyHomeDrive()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.HomeDrive = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.HomeDrive;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyHomePhone()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.HomePhone = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.HomePhone;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyLastName()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.LastName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.LastName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Logon"), Test]
        public void VerifyLogonCount()
        {
            // Declare variable to hold property set method
            System.Int32 setValue = 10;

            // Test set method
            _target.LogonCount = setValue;

            // Declare return variable to hold property get method
            System.Int32 getValue = 0;

            // Test get method
            getValue = _target.LogonCount;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyMemberOf()
        {
            _target.MemberOf.Add("A");
            _target.MemberOf.Add("B");

            // Test get method
            IList<string> getValue = _target.MemberOf;

            // Perform Assert Tests
            Assert.AreEqual(2, getValue.Count);
        }

        [Test]
        public void VerifyMiddleInitial()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.MiddleInitial = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.MiddleInitial;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyNotes()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.Notes = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.Notes;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GUID"), Test]
        public void VerifyObjectGUID()
        {
            // Declare variable to hold property set method
            System.Byte[] setValue = new byte[16] { 23, 146, 94, 169, 24, 174, 215, 76, 131, 212, 189, 11, 10, 26, 72, 223 };

            // Test set method
            _target.ObjectGUID = setValue;

            // Declare return variable to hold property get method
            System.Byte[] getValue = null;

            // Test get method
            getValue = _target.ObjectGUID;

            // Perform Assert Tests
            Assert.AreSame(setValue, getValue);
            Assert.IsFalse(string.IsNullOrEmpty(_target.ObjectGUIDString));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GUID"), Test]
        public void VerifyObjectGUIDString()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.SetObjectGUIDString(setValue);

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.ObjectGUIDString;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID"), Test]
        public void VerifyObjectSID()
        {
            // Declare variable to hold property set method
            byte[] setValue =  new byte[28] {1,5,0,0,0,0,0,5,21,0,0,0,238,200,27,18,29,211,127,17,176,145,206,59,85,13,0,0};

            // Test set method
            _target.ObjectSID = setValue;

            // Declare return variable to hold property get method
            System.Byte[] getValue = null;

            // Test get method
            getValue = _target.ObjectSID;

            // Perform Assert Tests
            Assert.AreSame(setValue, getValue);
            Assert.IsFalse(string.IsNullOrEmpty(_target.ObjectSIDString));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID"), Test]
        public void VerifyObjectSIDString()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.SetObjectSIDString(setValue);

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.ObjectSIDString;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyOffice()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.Office = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.Office;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyPasswordLastSet()
        {
            // Declare variable to hold property set method
            System.Int32 setValue = 10;

            // Test set method
            _target.PasswordLastSet = setValue;

            // Declare return variable to hold property get method
            System.Int32 getValue = 0;

            // Test get method
            getValue = _target.PasswordLastSet;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyPOBox()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.POBox = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.POBox;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyProfilePath()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.ProfilePath = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.ProfilePath;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM"), Test]
        public void VerifySAMAccountName()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.SAMAccountName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.SAMAccountName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyScriptPath()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.ScriptPath = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.ScriptPath;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyState()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.State = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.State;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyStreetAddress()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.StreetAddress = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.StreetAddress;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyUserPrincipalName()
        {
            // Declare variable to hold property set method
            System.String setValue = String.Empty;

            // Test set method
            _target.UserPrincipalName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.UserPrincipalName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyZipCode()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.ZipCode = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.ZipCode;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyEnabled()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.Enabled = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue = false;

            // Test get method
            getValue = _target.Enabled;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyPasswordNeverExpires()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.PasswordNeverExpires = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue = false;

            // Test get method
            getValue = _target.PasswordNeverExpires;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Logon"), Test]
        public void VerifyMustChangePasswordOnNextLogon()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.MustChangePasswordOnNextLogOn = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue = false;

            // Test get method
            getValue = _target.MustChangePasswordOnNextLogOn;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyPath()
        {
            // Declare variable to hold property set method
            System.String setValue = "Test";

            // Test set method
            _target.SetPath(setValue);

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.Path;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }


        #endregion

    }

}

