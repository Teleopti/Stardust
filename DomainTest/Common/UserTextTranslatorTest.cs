using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture, SetUICulture("en-US")]
    public class UserTextTranslatorTest
    {
        private UserTextTranslator _target;


        [SetUp]
        public void Setup()
        {
            _target = new UserTextTranslator();
        }

        [Test]
        public void VerifyTranslateString()
        {
            string toTranslate;
            string expected;
            string result;

            toTranslate = "textNotInDictionary";
            result = _target.TranslateText(toTranslate);
            expected = toTranslate;
            Assert.AreEqual(expected, result);

            toTranslate = "AbandonedCalls";
            result = _target.TranslateText(toTranslate);
            expected = "Abandoned calls";
            Assert.AreEqual(expected, result);

            toTranslate = "xxAbandonedCalls";
            result = _target.TranslateText(toTranslate);
            expected = "Abandoned calls";
            Assert.AreEqual(expected, result);

            toTranslate = "xxxAbandonedCalls";
            result = _target.TranslateText(toTranslate);
            expected = "Abandoned calls";
            Assert.AreEqual(expected, result);

            // test that it does not fail for string shorter than two chars
            toTranslate = "x";
            result = _target.TranslateText(toTranslate);
            expected = toTranslate;
            Assert.AreEqual(expected, result);

            // test that it does not fail for empty string
            toTranslate = string.Empty;
            result = _target.TranslateText(toTranslate);
            expected = toTranslate;
            Assert.AreEqual(expected, result);

            // test that it does not fail for null string
            toTranslate = null;
            result = _target.TranslateText(toTranslate);
            expected = toTranslate;
            Assert.AreEqual(expected, result);
        }
    }
}
