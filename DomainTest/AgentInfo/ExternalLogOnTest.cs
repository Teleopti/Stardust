using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    [TestFixture]
    public class ExternalLogOnTest
    {
        private ExternalLogOn target;

        [SetUp]
        public void Setup()
        {
            target = new ExternalLogOn(-1,-2,"ABCD","Ab Cd",true);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(target is IBelongsToBusinessUnit);
            Assert.AreEqual(-1,target.AcdLogOnMartId);
            Assert.AreEqual(-2,target.AcdLogOnAggId);
            Assert.AreEqual("ABCD",target.AcdLogOnOriginalId);
            Assert.AreEqual("Ab Cd", target.AcdLogOnName);
            Assert.AreEqual(-1,target.DataSourceId);
            Assert.IsTrue(target.Active);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(),true));
        }

        [Test]
        public void VerifyCanSetProperties()
        {
            target.AcdLogOnMartId = 1;
            target.AcdLogOnAggId = 2;
            target.AcdLogOnOriginalId = "DCBA";
            target.AcdLogOnName = "Dc Ba";
            target.Active = false;
            target.DataSourceId = 3;

            Assert.AreEqual(1,target.AcdLogOnMartId);
            Assert.AreEqual(2, target.AcdLogOnAggId);
            Assert.AreEqual("DCBA", target.AcdLogOnOriginalId);
            Assert.AreEqual("Dc Ba", target.AcdLogOnName);
            Assert.IsFalse(target.Active);
            Assert.AreEqual(3,target.DataSourceId);
        }

        [Test]
        public void VerifyCloneWorks()
        {
            ExternalLogOn clonedEntity = (ExternalLogOn)target.Clone();
            Assert.AreNotEqual(clonedEntity, target);
            Assert.AreNotSame(clonedEntity, target);
        }
    }
}
