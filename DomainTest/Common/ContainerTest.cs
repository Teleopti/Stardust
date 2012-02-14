using System.Linq;
using NUnit.Framework;

namespace Teleopti.Ccc.DomainTest.Common
{

	[TestFixture]
	public class ContainerTest
	{
		
		#region Variables
		
		// Variable to hold object to be tested for reuse by init functions
		private ContainerTestClass<object> _target;
        private object _content;
		
		#endregion
				
		#region SetUp and TearDown
		
		[SetUp]
		public void TestInit()
		{
            _content = new object();
            _target = new ContainerTestClass<object>(_content);
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
            _target = new ContainerTestClass<object>();
			Assert.IsNotNull(_target);
		}

        [Test]
        public void VerifyConstructorOverload1()
        {
            // Perform Assert Tests
            Assert.IsNotNull(_target);
            Assert.AreSame(_content, _target.Content);
        }
	
		#endregion

		#region Property Tests
		
		[Test]
		public void VerifyDomainEntity()
		{

            object setValue = new object();

            _target.Content = setValue;

			// Declare return variable to hold property get method
			object getValue;

			// Test get method
            getValue = _target.Content;
			
			// Perform Assert Tests
            Assert.AreSame(setValue, getValue);
		}
		

		#endregion
		
	}
}
