using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Hql.Ast.ANTLR;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Collection
{
  
    [TestFixture]
    public class ExtensionsTest
    {
        private IList<int> _target;
        private IList<int> _source;

        [SetUp]
        public void Setup()
        {
            _target = new List<int>();
            _source = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                 _source.Insert(i,i);
            }
        }


        [Test]
        public void VerifyCanCallGetRandom()
        { 
            foreach (int integer in _source)
            {
                _target.Add(_source.GetRandom());
            }
            Assert.LessOrEqual(Math.Abs(_source.Average() - _target.Average()), 10);
            Assert.IsTrue(HasDifferentElement(_source,_target));    
        }

        [Test]
        public void VerifyCanGetRandomWithDuplicatesRandomList()
        { 
           
            _target = new List<int>(_source.GetRandom(_source.Count(),false));
            
            if (!HasDuplicatedElements(_target))
                {
                 _target = (List<int>) _source.GetRandom(_source.Count(),false);
                }
            Assert.IsTrue(HasDuplicatedElements(_target));
            Assert.AreEqual(_source.Count(), _target.Count());

        }

        [Test]
        public void VerifyBatch()
        {
            var list = new List<int>{1,2,3,4,5,6,7,8,9,10};

            var batched = list.Batch(3);

            Assert.AreEqual(4, batched.Count());
            Assert.AreEqual(10, batched[3].First());
        }

        [Test]
        public void VerifyBatchSizeMustBePositive()
        {
            var list = new List<int>{1};
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Batch(-1));
        }

        [Test]
        public void VerifyCanCallGetRandomWithoutDuplicates()
        { 
            //If same length, no duplicates, same SUM but not in the same order, it works
            Assert.AreNotEqual(0, _source.Count);
            _target = new List<int>(_source.GetRandom(_source.Count(), true));
            Assert.AreNotEqual(0, _source.Count);
            Assert.IsFalse(HasDuplicatedElements(_target),"Element is duplicated");
            Assert.AreEqual(_target.Sum(), _source.Sum(),"The sum of  elements are different");
            Assert.AreEqual(_target.Count(), _source.Count(),"Number of elements are different");
            Assert.IsTrue(HasDifferentElement(_source, _target),"All the elements are in the same order");

            
        }

        [Test]
        public void VerifyNotSameCollectionTwice()
        {
            IList<int> listOne = new List<int>(_source.GetRandom(_source.Count(), true));
            IList<int> listTwo = new List<int>(_source.GetRandom(_source.Count(), true));
            Assert.IsTrue(HasDifferentElement(listOne, listTwo));
        }

        [Test]
        public void VerifyRandomCollectionCountIsSameAsSource()
        {
            _target = new List<int>(_source.GetRandom(200, true));
            Assert.AreEqual(_source.Count(), _target.Count());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "testInt")]
        public void VerifyThrowsNullException()
        {
            IList<int> test = null;
            int testInt = test.GetRandom();
        }

        private bool HasDifferentElement(IList<int> sourceList, IList<int> compareList)
        {
            for (int i = 0; i < Math.Min(sourceList.Count, compareList.Count); i++)
            {
                if (sourceList[i] != compareList[i]) return true;
            }
            return false;
        }

        private bool HasDuplicatedElements(IList<int> sourceList)
        {
            var numberGroups =
                from n in sourceList
                group n by n into g
                select new { Quantity = g.Count() };


            foreach (var g in numberGroups)
            {
                if (g.Quantity > 1)
                {
                    return true;
                }
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]  
        public void VerifyCloneCollection()
        {
            // check for ICloneableEntity implemented, not empty values.
            IList<IWorkShiftLimiter> originalCollection = new List<IWorkShiftLimiter>();

            IWorkShiftLimiter atl1 = new ActivityTimeLimiter(new Activity("act"), TimeSpan.FromHours(2), OperatorLimiter.Equals);
            originalCollection.Add(atl1);

            IWorkShiftLimiter atl2 = new ActivityTimeLimiter(new Activity("act1"), TimeSpan.FromHours(2), OperatorLimiter.Equals);
            originalCollection.Add(atl2);

            IWorkShiftLimiter atl3 = new ActivityTimeLimiter(new Activity("act2"), TimeSpan.FromHours(2), OperatorLimiter.Equals);
            originalCollection.Add(atl3);

            IList<IWorkShiftLimiter> cloneCollection = Extensions.CloneCollection(originalCollection);
            
            Assert.AreEqual(cloneCollection.Count, originalCollection.Count);


            // check for ICloneableEntity not implemented, empty values.
            IList<ActivityTimeLimiter> originalCollection1 = new List<ActivityTimeLimiter>();
            ActivityTimeLimiter atl4 = new ActivityTimeLimiter(new Activity("act4"), TimeSpan.FromHours(2), OperatorLimiter.Equals);
            originalCollection1.Add(atl4);

            IList<ActivityTimeLimiter> cloneCollection1 = Extensions.CloneCollection(originalCollection1);
                        
            Assert.AreEqual(cloneCollection1.Count, 0);

        }

        [Test]
        public void VerifyIsEmpty()
        {
            Assert.IsTrue(_target.IsEmpty());
            Assert.IsFalse(_source.IsEmpty());
        }

        [Test]
        public void VerifyIsAny()
        {
            Assert.IsTrue(_target.IsNullOrEmpty());
			Assert.IsFalse(_source.IsNullOrEmpty());

	        _source = null;
	        Assert.IsTrue(_source.IsNullOrEmpty());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldCopyEnumerable()
        {
            IList nonGenericList = new ArrayList {"string", 1};

            var actual = nonGenericList.CopyEnumerable<object>();

            Assert.That(actual, Is.Not.SameAs(nonGenericList));
            Assert.That(actual.Count(), Is.EqualTo(nonGenericList.Count));
            Assert.That(actual.ElementAt(0), Is.EqualTo(nonGenericList[0]));
            Assert.That(actual.ElementAt(1), Is.EqualTo(nonGenericList[1]));
        }

		[Test]
		public void ShouldBeRandomized()
		{
			var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
			var newList = list.Randomize();

			Assert.That(newList, Is.Not.EqualTo(list));
		}
    }
}
 