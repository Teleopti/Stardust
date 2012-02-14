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
    public class QueueSourceMapperTest : MapperTest<int>
    {
        [Test]
        public void VerifyMapping()
        {
            int org = 3;
            string orgSting = org.ToString(CultureInfo.CurrentCulture);
            QueueSourceMapper mapper = new QueueSourceMapper(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Local));
            IQueueSource theNew = mapper.Map(org);
            Assert.AreEqual(orgSting, theNew.Name);
            Assert.AreEqual(orgSting, theNew.Description);
            Assert.AreEqual(org, theNew.QueueAggId);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 0; }
        }
    }
}
