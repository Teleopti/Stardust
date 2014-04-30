using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors"), TestFixture]
    public class BlockFinderTypeCreatorTest
    {
		[Test]
        public static void CheckCorrectNumberOfOptions()
        {
            Assert.AreEqual(3, BlockFinderTypeCreator.GetBlockFinderTypes.Count);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public static void CheckCorrectContentsAreReceived()
        {
            var blockType = BlockFinderTypeCreator.GetBlockFinderTypes.OrderBy(x => x.Key).ToList();
            Assert.AreEqual(blockType[0].Key , BlockFinderType.BetweenDayOff.ToString() );
            Assert.AreEqual(blockType[1].Key , BlockFinderType.SchedulePeriod.ToString() );
        }

        
    }
}
