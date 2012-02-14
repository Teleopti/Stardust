using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.WinCode.PersonAdmin;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WinCodeTest.PersonAdmin
{
	
	[TestFixture]
	public class PersonGridDataTest
	{
		
		#region Variables
		
		// Variable to hold object to be tested for reuse by init functions
		private PersonGridData _target;
        private Person _base;
		
		#endregion
				
		#region SetUp and TearDown
		
		[SetUp]
		public void TestInit()
		{
            _base = new Person();
			_target = new PersonGridData();
            _target.ContainedEntity = _base;
		}
		
		[TearDown]
		public void TestDispose()
		{
			_target = null;
            _base = null;
		}
		
		#endregion
		
		#region Constructor Tests
		
		[Test]
		public void VerifyConstructor()
		{
						
			// Perform Assert Tests
			Assert.IsNotNull(_target);

		}
				
		#endregion
		
		#region Property Tests
		
		[Test]
		public void VerifyFirstName()
		{
			// Declare variable to hold property set method
			System.String setValue = "FirstName";

			// Test set method
			_target.FirstName = setValue;

			// Declare return variable to hold property get method
			System.String getValue = String.Empty;

			// Test get method
			getValue = _target.FirstName;
			
			// Perform Assert Tests
			Assert.AreSame(setValue, getValue);
		}
		
		[Test]
		public void VerifyLastName()
		{
			// Declare variable to hold property set method
			System.String setValue = "LastName";

			// Test set method
			_target.LastName = setValue;

			// Declare return variable to hold property get method
			System.String getValue = String.Empty;

			// Test get method
			getValue = _target.LastName;
			
			// Perform Assert Tests
			Assert.AreSame(setValue, getValue);
		}
		
		[Test]
		public void VerifyEmail()
		{
			// Declare variable to hold property set method
			System.String setValue = "Mail";

			// Test set method
			_target.Email = setValue;

			// Declare return variable to hold property get method
			System.String getValue = String.Empty;

			// Test get method
			getValue = _target.Email;
			
			// Perform Assert Tests
			//TODO: Write Assert Tests for Email()
			Assert.AreSame(setValue, getValue);
		}
		
		[Test]
		public void VerifyIsAgent()
		{
			// Declare return variable to hold property get method
			System.Boolean getValue = false;

            bool expectedValue = (_base.Period(DateTime.UtcNow) != null);  

			// Test get method
			getValue = _target.IsAgent;
			
			// Perform Assert Tests
			//TODO: Write Assert Tests for IsAgent()
            Assert.AreEqual(expectedValue, getValue);
		}
		
		[Test]
		public void VerifyIsUser()
		{
            // Declare return variable to hold property get method
            Boolean getValue;

            PermissionInformation info = _base.PermissionInformation;

            bool expectedValue = (info != null);

            // Test get method
            getValue = _target.IsUser;

            // Perform Assert Tests
            //TODO: Write Assert Tests for IsAgent()
            Assert.AreEqual(expectedValue, getValue);
		}
		

		#endregion
		
		
	}

}