using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class BlockFinderResultTest
    {
        private IBlockFinderResult _target;

        [SetUp]
        public void Setup()
        {
            IShiftCategory cat = null;
            _target = new BlockFinderResult(cat, new List<DateOnly>(), new Dictionary<string, IWorkShiftFinderResult>());
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNull(_target.ShiftCategory);
            Assert.AreEqual(0, _target.BlockDays.Count);
            Assert.AreEqual(0, _target.WorkShiftFinderResult.Count);
        }
    }
}