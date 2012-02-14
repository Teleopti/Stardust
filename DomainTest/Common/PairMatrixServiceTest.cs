using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PairMatrixServiceTest
    {
        private PairMatrixService<int> target;
        private IPairDictionaryFactory<int> factory;

        [SetUp]
        public void Setup()
        {
            factory = new PairDictionaryFactory<int>();
            target = new PairMatrixService<int>(factory);
        }

        [Test]
        public void VerifyCreateDependencies()
        {
            ICollection<IPair<int>> pairList = createPairList();


            target.CreateDependencies(pairList, new List<int> {4});
            IEnumerable<int> left = target.FirstDependencies;
            IEnumerable<int> right = target.SecondDependencies;

            Assert.AreEqual(3, left.Count());
            Assert.AreEqual(3, right.Count());

            CollectionAssert.Contains(left, 3);
            CollectionAssert.Contains(left, 4);
            CollectionAssert.Contains(left, 9);
            
            CollectionAssert.Contains(right, 6);
            CollectionAssert.Contains(right, 7);
            CollectionAssert.Contains(right, 8);
        }

        [Test]
        public void VerifyNoHits()
        {
            target.CreateDependencies(createPairList(), new List<int> { 11, 1 });
            CollectionAssert.IsEmpty(target.FirstDependencies);
            CollectionAssert.IsEmpty(target.SecondDependencies);
        }

        private static ICollection<IPair<int>> createPairList()
        {
            return new List<IPair<int>>()
                       {
                           new Pair<int>(2,1),
                           new Pair<int>(3,7),
                           new Pair<int>(4,8),
                           new Pair<int>(4,6),
                           new Pair<int>(9,6),
                           new Pair<int>(9,7)
                       };
        }
    }
}
