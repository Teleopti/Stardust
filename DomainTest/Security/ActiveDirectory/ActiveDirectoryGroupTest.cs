using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.DomainTest.Security.ActiveDirectory
{

    [TestFixture]
    public class ActiveDirectoryGroupTest
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private ActiveDirectoryGroupTestClass _target;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _target = new ActiveDirectoryGroupTestClass();
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
            ActiveDirectoryGroup returnValue;

            // Instantiate object
            returnValue = new ActiveDirectoryGroup();

            // Perform Assert Tests
            Assert.IsNotNull(returnValue);

        }

        #endregion

        #region Property Tests

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
        }

        [Test]
        public void VerifyMemberOf()
        {
            // Test set method
            _target.MemberOf.Add("A");
            _target.MemberOf.Add("B");

            // Declare return variable to hold property get method
            List<string> getValue;

            // Test get method
            getValue = _target.MemberOf;

            // Perform Assert Tests
            Assert.AreEqual(2, getValue.Count);
        }

        [Test]
        public void VerifyMembers()
        {
            // Test set method
            _target.MemberActions.Add("A", MemberAction.Add);
            _target.MemberActions.Add("B", MemberAction.Add);

            List<string> getValue = null;
            // Test get method
            getValue = _target.Members;

            // Perform Assert Tests
            Assert.AreEqual(getValue.Count, 2);
        }

        [Test]
        public void VerifyMemberActions()
        {
           // Test set method
            _target.MemberActions.Add("A", MemberAction.Add);
            _target.MemberActions.Add("B", MemberAction.Add);

            // Declare return variable to hold property get method
            SortedDictionary<System.String,  MemberAction> getValue;

            // Test get method
            getValue = _target.MemberActions;

            // Perform Assert Tests
            Assert.AreEqual(2, getValue.Count);
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
            System.String setValue = String.Empty;

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
            byte[] setValue = new byte[28] { 1, 5, 0, 0, 0, 0, 0, 5, 21, 0, 0, 0, 238, 200, 27, 18, 29, 211, 127, 17, 176, 145, 206, 59, 85, 13, 0, 0 };

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM"), Test]
        public void VerifySAMAccountName()
        {
            // Declare variable to hold property set method
            System.String setValue ="Test";

            // Test set method
            _target.SAMAccountName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.SAMAccountName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }


        #endregion

    }

}

