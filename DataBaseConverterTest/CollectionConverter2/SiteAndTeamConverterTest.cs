using System;
using System.Collections.Generic;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter2;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter2
{
    /// <summary>
    /// Tests for the SiteAndTeamConverter
    /// </summary>
    [TestFixture]
    public class SiteAndTeamConverterTest
    {
        /// <summary>
        /// Determines whether this instance can create.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            SiteAndTeamConverter converter;
            converter = new SiteAndTeamConverter(new BusinessUnit("Default"), new List<Unit>());
            Assert.IsNotNull(converter);
        }

        /// <summary>
        /// Determines whether this instance [can convert site and team].
        /// </summary>
        [Test]
        public void CanConvertSiteAndTeam()
        {
            Unit oldUnit;
            UnitSub oldTeam;
            oldUnit = new Unit(1, "test", false, false, null, new List<UnitSub>(), false);
            oldTeam = new UnitSub(7, "testTeam", 1, false, oldUnit);
            oldUnit.ChildUnits.Add(oldTeam);
            List<Unit> theList = new List<Unit>();
            theList.Add(oldUnit);

            SiteMapper siteMapper = new SiteMapper(new MappedObjectPair(), TimeZoneInfo.Utc);
            BusinessUnit bu = new BusinessUnit("Default");
            SiteAndTeamConverter converter = new SiteAndTeamConverter(bu, theList);
            converter.Convert(siteMapper, new TeamMapper(new MappedObjectPair()));
            Assert.AreEqual(1, bu.SiteCollection.Count);
            Assert.AreEqual(1, bu.SiteCollection[0].TeamCollection.Count);
        }

        /// <summary>
        /// Objects pair lists are filled.
        /// </summary>
        [Test]
        public void ObjectPairListsAreFilled()
        {
            Unit oldUnit;
            UnitSub oldTeam;
            oldUnit = new Unit(1, "test", false, false, null, new List<UnitSub>(), false);
            oldTeam = new UnitSub(7, "testTeam", 1, false, oldUnit);
            oldUnit.ChildUnits.Add(oldTeam);
            List<Unit> theList = new List<Unit>();
            theList.Add(oldUnit);

            SiteMapper siteMapper = new SiteMapper(new MappedObjectPair(), TimeZoneInfo.Utc);
            BusinessUnit bu = new BusinessUnit("Default");
            SiteAndTeamConverter converter = new SiteAndTeamConverter(bu, theList);
            converter.Convert(siteMapper, new TeamMapper(new MappedObjectPair()));
            Assert.IsNotNull(converter.PairListUnitSite.GetPaired(oldUnit));
            Assert.IsNotNull(converter.PairListUnitSubTeam.GetPaired(oldTeam));
        }
    }
}