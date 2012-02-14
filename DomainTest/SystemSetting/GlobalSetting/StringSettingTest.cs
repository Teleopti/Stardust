using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.DomainTest.SystemSetting.GlobalSetting
{
    [TestFixture]
    public class StringSettingTest
    {
        private StringSetting _stringSetting;
        [SetUp]
        public void Setup()
        {
            _stringSetting = new StringSetting();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(string.Empty, _stringSetting.StringValue);
            string stringToSave = "support@teleopti.com";
            _stringSetting.StringValue = stringToSave;
            Assert.AreEqual(stringToSave, _stringSetting.StringValue);
        }
    }
}
