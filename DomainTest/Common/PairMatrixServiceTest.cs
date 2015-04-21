using System;
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
            var pairList = createPairList();

            var result = target.CreateDependencies(pairList, new List<int> {4});
            IEnumerable<int> left = result.FirstDependencies;
            IEnumerable<int> right = result.SecondDependencies;

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
            var result = target.CreateDependencies(createPairList(), new List<int> { 11, 1 });
            CollectionAssert.IsEmpty(result.FirstDependencies);
            CollectionAssert.IsEmpty(result.SecondDependencies);
        }

        private static IEnumerable<Tuple<int, int>> createPairList()
        {
					return new List<Tuple<int, int>>()
                       {
                           Tuple.Create(2,1),
                           Tuple.Create(3,7),
                           Tuple.Create(4,8),
                           Tuple.Create(4,6),
                           Tuple.Create(9,6),
                           Tuple.Create(9,7)
                       };
        }
    }
}
