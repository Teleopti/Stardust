using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class LocalizedUpdateInfoTest
    {
        private LocalizedUpdateInfo target;

        [SetUp]
        public void Setup()
        {
            target = new LocalizedUpdateInfo();
        }

        [Test]
        public void VerifyUpdatedText()
        {
            AggregateRootTest.CreatedAndChangedTest testRoot = new AggregateRootTest.CreatedAndChangedTest();
            string updated = target.UpdatedByText(testRoot, "Updated by:");
            Assert.IsTrue(updated.Length > 0);
        }

        [Test]
        public void VerifyUpdatedTextWhenValuesAreNull()
        {
            var rootCreatedTest = new AggregateRootTest.AggRootWithNoBusinessUnit();
            string updated = target.UpdatedByText(rootCreatedTest, "Updated by:");
            Assert.IsTrue(updated.Length == 0);
        }
        [Test]
        public void VerifyCanGetUpdateTimeTextWhenNull()
        {
            var rootCreatedTest = new AggregateRootTest.AggRootWithNoBusinessUnit();
            Assert.AreEqual(string.Empty, target.UpdatedTimeInUserPerspective(rootCreatedTest));
        }
        [Test]
        public void VerifyCanGetUpdateTimeText()
        {
            var testRoot = new AggregateRootTest.CreatedAndChangedTest();
            Assert.IsTrue(!string.IsNullOrEmpty(target.UpdatedTimeInUserPerspective(testRoot)));
        }
    }
}
