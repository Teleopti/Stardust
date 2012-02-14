using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PairDictionaryFactoryTest
    {
        private PairDictionaryFactory<int> target;

        [SetUp]
        public void Setup()
        {
            target = new PairDictionaryFactory<int>();
        }

        [Test]
        public void VerifyResult()
        {
            IEnumerable<IPair<int>> pairList = new List<IPair<int>>
                           {
                               {new Pair<int>(2, 1)},
                               {new Pair<int>(3, 7)},
                               {new Pair<int>(9, 7)},
                               {new Pair<int>(4, 8)},
                               {new Pair<int>(4, 6)},
                               {new Pair<int>(9, 6)},
                               {new Pair<int>(9, 9)}

                           };
            target.CreateDictionaries(pairList);
            IDictionary<int, ICollection<int>> firstDic = target.FirstDictionary;
            IDictionary<int, ICollection<int>> secondDic = target.SecondDictionary;
            //firstdic test
            Assert.AreEqual(4, firstDic.Count);
            CollectionAssert.AreEqual(new List<int> { 1 }, firstDic[2]);
            CollectionAssert.AreEqual(new List<int> { 7 }, firstDic[3]);
            CollectionAssert.AreEqual(new List<int> { 7,6,9 }, firstDic[9]);
            CollectionAssert.AreEqual(new List<int> { 8,6 }, firstDic[4]);
            //seconddic test
            Assert.AreEqual(5, secondDic.Count);
            CollectionAssert.AreEqual(new List<int> { 2 }, secondDic[1]);
            CollectionAssert.AreEqual(new List<int> { 4, 9 }, secondDic[6]);
            CollectionAssert.AreEqual(new List<int> { 3, 9 }, secondDic[7]);
            CollectionAssert.AreEqual(new List<int> { 4 }, secondDic[8]);
            CollectionAssert.AreEqual(new List<int> { 9 }, secondDic[9]);
        }

    }
}
