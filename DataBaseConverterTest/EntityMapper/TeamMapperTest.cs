using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for TEamMapper
    /// </summary>
    [TestFixture]
    public class TeamMapperTest : MapperTest<UnitBase>
    {
        /// <summary>
        /// Determines whether this type [can validate number of properties].
        /// </summary>
        [Test]
        public void CanValidateNumberOfProperties()
        {
            Assert.AreEqual(6, PropertyCounter.CountProperties(typeof (UnitSub)));
            Assert.AreEqual(12, PropertyCounter.CountProperties(typeof (Team)));
        }

        /// <summary>
        /// Determines whether this instance [can map activity6x].
        /// </summary>
        /// <remarks>
        /// </remarks>
        [Test]
        public void CanMapTeam6XAndShrinkName()
        {
            string oldName = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";

            Unit oldUnit = new Unit(-1, "oldUnit", false, false, null, null, false);
            Site newSite = new Site("newSite");
            ObjectPairCollection<Unit, ISite> siteList = new ObjectPairCollection<Unit, ISite>();
            siteList.Add(oldUnit, newSite);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Site = siteList;

            TeamMapper teamMap = new TeamMapper(mappedObjectPair);
            

            UnitSub oldTeam = new UnitSub(-1, oldName, -1, false, oldUnit);
            ITeam newTeam = teamMap.Map(oldTeam);
            //do not test dbid
            //name should be truncated to 50 chars
            Assert.AreEqual(50, newTeam.Description.Name.Length);
            //Assert.AreEqual(oldName.Substring(0, 25), newTeam.Name);
        }

        [Test]
        public void VerifySiteIsSet()
        {
            Site site = new Site("ff");
            MappedObjectPair mapped = new MappedObjectPair();
            ObjectPairCollection<Unit, ISite> pair = new ObjectPairCollection<Unit, ISite>();
            UnitSub oldTeam = new UnitSub(3, "ff", 47, false, null);
            Unit oldSite = new Unit(47, "ff",false,false,null,null,false);

            pair.Add(oldSite, site);
            mapped.Site = pair;

            TeamMapper teamMapper = new TeamMapper(mapped);
            ITeam newTeam = teamMapper.Map(oldTeam);
            Assert.AreSame(site, newTeam.Site);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 4; }
        }

        [Test]
        public void VerifyCanHandleDeleted()
        {
            string oldName = "Gamunu Balakaya";
            Unit myOldUnit = new Unit(-1, "myOldUnit", false, false, null, null, false);
            Site myNewSite = new Site("myNewSite");
            ObjectPairCollection<Unit, ISite> siteList = new ObjectPairCollection<Unit, ISite>();
            siteList.Add(myOldUnit, myNewSite);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Site = siteList;

            TeamMapper teamMap = new TeamMapper(mappedObjectPair);


            UnitSub oldTeam = new UnitSub(-1, oldName, -1, true, myOldUnit);
            ITeam newTeam = teamMap.Map(oldTeam);

            Assert.AreEqual(true, ((IDeleteTag)newTeam).IsDeleted);
        }
    }
}