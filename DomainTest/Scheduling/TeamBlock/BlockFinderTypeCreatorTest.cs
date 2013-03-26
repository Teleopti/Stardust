using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class BlockFinderTypeCreatorTest
    {
        [SetUp]
        public void Setup()
        {
           
        }

        [Test]
        public void CheckCorrectNumberOfOptions()
        {
            Assert.AreEqual(BlockFinderTypeCreator.GetBlockFinderTypes.Count, 2);
        }

        [Test]
        public void CheckCorrectContentsAreRecieved()
        {
            var blockType = BlockFinderTypeCreator.GetBlockFinderTypes.OrderBy(x => x.Key).ToList();
            Assert.AreEqual(blockType[0].Key , BlockFinderType.BetweenDayOff.ToString() );
            Assert.AreEqual(blockType[1].Key , BlockFinderType.SchedulePeriod.ToString() );
        }

        
    }
}
