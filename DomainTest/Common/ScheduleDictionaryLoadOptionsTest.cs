using Teleopti.Ccc.Domain.Common;
using NUnit.Framework;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class ScheduleDictionaryLoadOptionsTest
    {
        private ScheduleDictionaryLoadOptions _target;

        [Test]
        public void ShouldSetProperties()
        {
            _target = new ScheduleDictionaryLoadOptions(false, false);
            Assert.IsFalse(_target.LoadRestrictions);
            Assert.IsFalse(_target.LoadNotes);

            _target = new ScheduleDictionaryLoadOptions(true, true);
            Assert.IsTrue(_target.LoadRestrictions);
            Assert.IsTrue(_target.LoadNotes);
        }
    }
}
