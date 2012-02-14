using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common.Presentation;

namespace Teleopti.Ccc.WinCodeTest.Common.Presentation
{

	[TestFixture]
	public class DomainEntityPresenterTest
	{
		
		#region Variables
		
		// Variable to hold object to be tested for reuse by init functions
        private DomainEntityPresenter<DomainEntityPresenterTestClass> _target;
        private DomainEntityPresenterTestClass _entity;
		
		#endregion
				
		#region SetUp and TearDown
		
		[SetUp]
		public void TestInit()
		{
            _entity = new DomainEntityPresenterTestClass("NAME", "DESC");
            _target = new DomainEntityPresenter<DomainEntityPresenterTestClass>(_entity);
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
            _target = new DomainEntityPresenter<DomainEntityPresenterTestClass>();
			Assert.IsNotNull(_target);
		}

        [Test]
        public void VerifyConstructorOverload1()
        {
            // Perform Assert Tests
            Assert.IsNotNull(_target);
            Assert.AreSame(_entity, _target.DomainEntity);
        }
	
		#endregion

		#region Property Tests
		
		[Test]
		public void VerifyDomainEntity()
		{

            DomainEntityPresenterTestClass setValue = new DomainEntityPresenterTestClass("Name", "Desc");

            _target.DomainEntity = setValue;

			// Declare return variable to hold property get method
            DomainEntityPresenterTestClass getValue = null;

			// Test get method
			getValue = _target.DomainEntity;
			
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
            DomainEntityPresenter<DomainEntityPresenterTestClass> nullId = new DomainEntityPresenter<DomainEntityPresenterTestClass>(new DomainEntityPresenterTestClass());
            DomainEntityPresenter<DomainEntityPresenterTestClass> nullId2 = new DomainEntityPresenter<DomainEntityPresenterTestClass>(new DomainEntityPresenterTestClass());
            DomainEntityPresenter<DomainEntityPresenterTestClass> Id1 = new DomainEntityPresenter<DomainEntityPresenterTestClass>(new DomainEntityPresenterTestClass());
            Id1.SetId(guid1);
            DomainEntityPresenter<DomainEntityPresenterTestClass> Id1Copy = new DomainEntityPresenter<DomainEntityPresenterTestClass>(new DomainEntityPresenterTestClass());
            Id1Copy.SetId(guid1);
            DomainEntityPresenter<DomainEntityPresenterTestClass> Id2 = new DomainEntityPresenter<DomainEntityPresenterTestClass>(new DomainEntityPresenterTestClass());
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
