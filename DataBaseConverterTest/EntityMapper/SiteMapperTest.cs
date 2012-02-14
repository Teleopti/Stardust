using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for SiteMapper
    /// </summary>
    [TestFixture]
    public class SiteMapperTest : MapperTest<global::Domain.Unit>
    {
        private string _oldName;
        /// <summary>
        /// Determines whether this instance [can map activity6x].
        /// </summary>
        /// <remarks>
        /// </remarks>
        [Test]
        public void CanMapSite6XAndShrinkName()
        {
            string oldName = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
            SiteMapper map = new SiteMapper(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Utc));
            global::Domain.Unit old = new global::Domain.Unit(-1, oldName, false, false, null, null, false);
            ISite newSite = map.Map(old);
            Assert.AreEqual(50, newSite.Description.Name.Length);
        }

        [Test]
        public void CanMapWithZeroLengthName()
        {
            string oldName = "";
            SiteMapper map = new SiteMapper(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Utc));
            global::Domain.Unit old = new global::Domain.Unit(-1, oldName, false, false, null, null, false);
            ISite newSite = map.Map(old);
            Assert.Greater(newSite.Description.Name.Length,0);
        }

        [Test]
        public void DoNotMapUnitWithAllUnitProperty()
        {
            _oldName = "abc";
            SiteMapper map = new SiteMapper(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Utc));
            global::Domain.Unit old = new global::Domain.Unit(-1, _oldName, true, false, null, null, false);
            ISite newSite = map.Map(old);
            Assert.IsNull(newSite);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 8; }
        }
    }
}