using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{

	[TestFixture]
	public class EntityContainerTest
	{
		
		#region Variables
		
		// Variable to hold object to be tested for reuse by init functions
        private EntityContainer<EntityConverterTestClass> _target;
        private EntityConverterTestClass _entity;
		
		#endregion
				
		#region SetUp and TearDown
		
		[SetUp]
		public void TestInit()
		{
            _entity = new EntityConverterTestClass("NAME", "DESC");
            _target = new EntityContainer<EntityConverterTestClass>(_entity);
		}
		
		[TearDown]
		public void TestDispose()
		{
			_target = null;
		}
		
		#endregion
		
		#region Constructor Tests
		
		[Test]
		public void VerifyConstructor()
		{		
			// Perform Assert Tests
            _target = new EntityContainer<EntityConverterTestClass>();
			Assert.IsNotNull(_target);
		}

        [Test]
        public void VerifyConstructorOverload1()
        {
            // Perform Assert Tests
            Assert.IsNotNull(_target);
            Assert.AreSame(_entity, _target.ContainedEntity);
        }
	
		#endregion

		#region Property Tests
		
		[Test]
		public void VerifyDomainEntity()
		{

            EntityConverterTestClass setValue = new EntityConverterTestClass("Name", "Desc");

            _target.ContainedEntity = setValue;

			// Declare return variable to hold property get method
            EntityConverterTestClass getValue = null;

			// Test get method
			getValue = _target.ContainedEntity;
			
			// Perform Assert Tests
            Assert.AreSame(setValue, getValue);
		}

        /// <summary>
        /// Determines whether this instance can set its id.
        /// </summary>
        [Test]
        public void VerifySetId()
        {
            Guid newId = Guid.NewGuid();
            _target.SetId(newId);
            Assert.AreEqual(newId, _target.Id);
        }

        /// <summary>
        /// Verifies the equals implementation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyEquals()
        {
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            EntityContainer<EntityConverterTestClass> nullId = new EntityContainer<EntityConverterTestClass>(new EntityConverterTestClass());
            EntityContainer<EntityConverterTestClass> nullId2 = new EntityContainer<EntityConverterTestClass>(new EntityConverterTestClass());
            EntityContainer<EntityConverterTestClass> Id1 = new EntityContainer<EntityConverterTestClass>(new EntityConverterTestClass());
            Id1.SetId(guid1);
            EntityContainer<EntityConverterTestClass> Id1Copy = new EntityContainer<EntityConverterTestClass>(new EntityConverterTestClass());
            Id1Copy.SetId(guid1);
            EntityContainer<EntityConverterTestClass> Id2 = new EntityContainer<EntityConverterTestClass>(new EntityConverterTestClass());
            Id2.SetId(guid2);

            Assert.IsFalse(nullId.Equals(nullId2));
            Assert.IsTrue(Id1.Equals(Id1Copy));
            Assert.IsFalse(Id1Copy.Equals(Id2));
            Assert.IsFalse(nullId.Equals(null));
            Assert.IsFalse(Id1.Equals(nullId2));
            Assert.IsFalse(Id1.Equals(3));
            Assert.IsTrue(nullId.Equals(nullId));
        }


		#endregion
		
	}
}
