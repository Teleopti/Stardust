using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{

    [TestFixture]
    public class AvailableDataEntryTest
    {

        #region Variables

        private AvailableDataEntry _target;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _target = new AvailableDataEntry();
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void VerifyConstructorOverload1()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructorOverload2()
        {
            IAuthorizationEntity authorizationEntity = new AuthorizationEntity("Key", "Name", "Description", "Value");
            _target = new AvailableDataEntry(authorizationEntity);

            Assert.AreEqual(authorizationEntity.AuthorizationName, _target.AuthorizationName);
            Assert.AreEqual(authorizationEntity.AuthorizationDescription, _target.AuthorizationDescription);
            Assert.AreEqual(authorizationEntity.AuthorizationValue, _target.AuthorizationValue);
        }

        #endregion

        #region Property Tests


        [Test]
        public void VerifyAvailableDataHolderName()
        {
            // Declare variable to hold property set method
            System.String setValue = "TestName";

            // Test set method
            _target.AvailableDataHolderName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.AvailableDataHolderName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyAvailableDataHolderDescription()
        {
            // Declare variable to hold property set method
            System.String setValue = "TestDescription";

            // Test set method
            _target.AvailableDataHolderDescription = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.AvailableDataHolderDescription;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyAvailableDataHolderValue()
        {
            // Declare variable to hold property set method
            System.String setValue = "TestValue";

            // Test set method
            _target.AvailableDataHolderValue = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.AvailableDataHolderValue;

            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyAvailableDataHolderKey()
        {
            const string setValue = "TestValue";
            _target.AvailableDataHolderKey = setValue;

            string getValue = _target.AvailableDataHolderKey;
            Assert.AreEqual(setValue, getValue);

            getValue = _target.AuthorizationKey;
            Assert.AreEqual(setValue, getValue);

        }

        [Test]
        public void VerifyAuthorizationName()
        {
            // Declare variable to hold property set method
            System.String setValue = "TestName";

            // Test set method
            _target.AvailableDataHolderName = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.AuthorizationName;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyAuthorizationDescription()
        {
            // Declare variable to hold property set method
            System.String setValue = "TestDescription";

            // Test set method
            _target.AvailableDataHolderDescription = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.AuthorizationDescription;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyAuthorizationValue()
        {
            // Declare variable to hold property set method
            System.String setValue = "TestValue";

            // Test set method
            _target.AvailableDataHolderValue = setValue;

            // Declare return variable to hold property get method
            System.String getValue = String.Empty;

            // Test get method
            getValue = _target.AuthorizationValue;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }



        #endregion


    }

}

