using System;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class ExternalLogOnMapperTest : MapperTest<int>
    {
        [Test]
        public void VerifyMapping()
        {
            int org = 3;
            string orgSting = org.ToString(CultureInfo.CurrentCulture);
            ExternalLogOnMapper mapper = new ExternalLogOnMapper(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Local));
            IExternalLogOn theNew = mapper.Map(org);
            Assert.AreEqual(orgSting, theNew.AcdLogOnOriginalId);
            Assert.AreEqual(orgSting, theNew.AcdLogOnName);
            Assert.AreEqual(org, theNew.AcdLogOnAggId);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 0; }
        }
    }
}
