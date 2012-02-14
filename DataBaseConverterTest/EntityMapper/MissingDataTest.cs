using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class MissingDataTest
    {
        [Test]
        public void VerifyNoName()
        {
            Assert.AreEqual("(No name)", MissingData.Name);
        }
    }
}
