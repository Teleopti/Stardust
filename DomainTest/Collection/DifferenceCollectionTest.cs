using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class DifferenceCollectionTest
    {
        private DifferenceCollection<IPerson> target;

        [SetUp]
        public void Setup()
        {
            target = new DifferenceCollection<IPerson>();
        }

        [Test]
        public void VerifyTryGetOriginalItemWithGiveHit()
        {
            var exists = new Person();
            ((IEntity)exists).SetId(Guid.NewGuid());
            var item = new DifferenceCollectionItem<IPerson>(exists, null); 
            target.Add(item);

            Assert.AreEqual(item, target.FindItemByOriginalId(exists.Id.Value));
            Assert.IsNull(target.FindItemByOriginalId(Guid.NewGuid()));
        }

        [Test]
        public void VerifyTryGetOriginalItemWithGiveNoHit()
        {
            var notExists = new Person();
            ((IEntity)notExists).SetId(Guid.NewGuid());
            target.Add(new DifferenceCollectionItem<IPerson>(null, notExists));
            Assert.IsNull(target.FindItemByOriginalId(notExists.Id.Value));
        }
    }
}
