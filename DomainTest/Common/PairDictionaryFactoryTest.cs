using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

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
	        var pairList = new List<Tuple<int, int>>
		        {
			        Tuple.Create(2, 1),
			        Tuple.Create(3, 7),
			        Tuple.Create(9, 7),
			        Tuple.Create(4, 8),
			        Tuple.Create(4, 6),
			        Tuple.Create(9, 6),
			        Tuple.Create(9, 9)
		        };
            var result = target.CreateDictionaries(pairList);
            IDictionary<int, ICollection<int>> firstDic = result.FirstDictionary;
            IDictionary<int, ICollection<int>> secondDic = result.SecondDictionary;
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
