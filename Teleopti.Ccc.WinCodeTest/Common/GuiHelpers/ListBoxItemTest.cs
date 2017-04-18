using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
    [TestFixture]
    public class ListBoxItemTest
    {
        #region Variables

        private ListBoxItem<object> target;
        private string _displayText;
        private object _value;

        #endregion

        #region Test Preparation Methods

        /// <summary>
        /// Setup tests.
        /// </summary>
        [SetUp]
        public void TestInit()
        {
            _value = new object();
            _displayText = "Displayed text";
            target = new ListBoxItem<object>(_value, _displayText);
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Tests constructor.
        /// </summary>
        [Test]
        public void ListBoxItem()
        {
            // Perform Assert Tests
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Tests constructor.
        /// </summary
        [Test]
        public void ListBoxItemOverload1()
        {
            // Declare return type to hold constructor result
            ListBoxItem<object> _returnValue;

            // Instantiate object
            _returnValue = new ListBoxItem<object>(_value, _displayText);

            // Perform Assert Tests
            Assert.IsNotNull(_returnValue);
        }

        #endregion

        #region Method Tests

        /// <summary>
        /// Tests ToString method.
        /// </summary
        [Test]
        public void ToStringTest()
        {
            // Declare variables to pass to method call
            //TODO: Set values for variables

            // Declare return type to hold method result
            String _returnValue;

            // Make method call
            _returnValue = target.ToString();

            // Perform Assert Tests
            Assert.AreEqual(_displayText, _returnValue);
        }

        #endregion

        #region Property Tests

        /// <summary>
        /// Tests Value property.
        /// </summary
        [Test]
        public void VerifyValue()
        {
            // Declare return variable to hold property get method
            object _getValue;
            object _setValue;

            _getValue = target.Value;

            Assert.AreSame(_value, _getValue);

            _setValue = new object();

            // Test set method
            target.Value = _setValue;

            _getValue = target.Value;

            Assert.AreSame(_setValue, _getValue);
        }

        /// <summary>
        /// Tests DisplayText property.
        /// </summary
        [Test]
        public void VerifyDisplayText()
        {
            // Declare variables
            string _getValue;
            string _setValue;

            // Test get method
            _getValue = target.DisplayText;

            Assert.AreEqual(_displayText, _getValue);

            // Declare variable to hold property set method
            _setValue = "New Display item";

            // Test set method
            target.DisplayText = _setValue;

            _getValue = target.DisplayText;

            Assert.AreEqual(_setValue, _getValue);
        }

        #endregion
    }
}