using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for AggregateEntityTest. 
    /// Only root is tested here. The rest will indirectly be tested by its concrete impl.
    /// </summary>
    [TestFixture]
    public class AggregateEntityTest
    {
        private MockRepository mocks;
        private AggregateEntity target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            target = mocks.PartialMock<AggregateEntity>();
        }

        [Test]
        public void VerifyCanSetParent()
        {
            IAggregateEntity ent = new dummyEntity();
            IEntity parent = mocks.StrictMock<IEntity>();
            ent.SetParent(parent);
            Assert.AreSame(parent, ent.Parent);
        }

        internal class dummyEntity : AggregateEntity
        {
            
        }

        /// <summary>
        /// Verifies that aggregate root can be found.
        /// </summary>
        [Test]
        public void VerifyCanFindRoot()
        {
            VerifyRoot(mocks, target, mocks.StrictMock<IAggregateEntity>(), mocks.StrictMock<IAggregateEntity>());
        }

        internal static void VerifyRoot(MockRepository mock, IAggregateEntity targ, IAggregateEntity level1,
                                        IAggregateEntity level2)
        {
            IAggregateRoot root = mock.StrictMock<IAggregateRoot>();
            using (mock.Record())
            {
                Expect.On(targ)
                    .Call(targ.Parent)
                    .Return(level1);
                Expect.On(level1)
                    .Call(level1.Parent)
                    .Return(level2);
                Expect.On(level2)
                    .Call(level2.Parent)
                    .Return(root);
            }
            using (mock.Playback())
            {
                Assert.AreSame(root, targ.Root());
            }
        }

        /// <summary>
        /// Verifies that the parent is not null.
        /// </summary>
        [Test]
        [ExpectedException(typeof (AggregateException))]
        public void VerifyParentIsNotNull()
        {
            using (mocks.Record())
            {
                Expect.On(target)
                    .Call(target.Parent)
                    .Return(null);
            }
            using (mocks.Playback())
            {
                ((IAggregateEntity) target).Root();
            }
        }

        /// <summary>
        /// Verifies that the aggregate entity must have aggregate entity as parent.
        /// </summary>
        [Test]
        [ExpectedException(typeof (AggregateException))]
        public void VerifyAggregateEntityMustHaveAggregateEntityAsParent()
        {
            IAggregateEntity level1 = mocks.StrictMock<IAggregateEntity>();
            IEntity level2 = mocks.StrictMock<IEntity>();
            using (mocks.Record())
            {
                Expect.On(target)
                    .Call(target.Parent)
                    .Return(level1);
                Expect.On(level1)
                    .Call(level1.Parent)
                    .Return(level2);
            }
            using (mocks.Playback())
            {
                ((IAggregateEntity) target).Root();
            }
        }
    }
}