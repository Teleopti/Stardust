using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for Mapper
    /// </summary>
    public abstract class MapperTest<TOld>
    {
        [Test]
        public void VerifyPropertiesCanBeRead()
        {
            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            ICccTimeZoneInfo timeZone = new CccTimeZoneInfo(TimeZoneInfo.Local);
            testMapper target = new testMapper(mappedObjectPair, timeZone);
            Assert.AreSame(mappedObjectPair, target.MappedObjectPair);
            Assert.AreSame(timeZone, target.TimeZone);
        }


        [Test]
        public void VerifyNoPropertyHasBeenAddedOrRemovedInOldDomain()
        {
            Assert.AreEqual(NumberOfPropertiesToConvert, PropertyCounter.CountProperties(typeof(TOld)));
        }

        protected abstract int NumberOfPropertiesToConvert { get;}


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyMappedObjectPairIsNotNull()
        {
            new testMapper(null, new CccTimeZoneInfo(TimeZoneInfo.Local));
        }

        private class testMapper : Mapper<int, string>
        {
            public testMapper(MappedObjectPair mappedObjectPair,
                                ICccTimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
            {
            }

            public override int Map(string oldEntity)
            {
                return 11;
            }
        }
    }
}
