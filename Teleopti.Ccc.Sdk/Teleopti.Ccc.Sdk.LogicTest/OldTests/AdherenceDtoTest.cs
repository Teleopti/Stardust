using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AdherenceDtoTest
    {
        private AdherenceDto _target;

        [SetUp]
        public void Setup()
        {
            _target = new AdherenceDto();
        }

        [Test]
        public void VerifyPropertiesAndConstructor()
        {
            Assert.IsNotNull(_target);
            ICollection<AdherenceDataDto> list = new List<AdherenceDataDto>();
            _target.AdherenceDataDtos = list;
            Assert.AreEqual(list, _target.AdherenceDataDtos);
        }
    }
}
