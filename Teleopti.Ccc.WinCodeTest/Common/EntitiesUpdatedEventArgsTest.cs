using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class EntitiesUpdatedEventArgsTest
	{
		EntitiesUpdatedEventArgs _target;

		[SetUp]
		public void Setup()
		{
			_target = new EntitiesUpdatedEventArgs();
		}

		[Test]
		public void VerifyCanCreate()
		{
			Assert.IsNotNull(_target);
		}
		[Test]
		public void VerifyPropertiesCanBeSet()
		{
			var guid = Guid.NewGuid();

			_target.EntityType = typeof(Person);
			_target.UpdatedIds = new[] { guid };

			Assert.That(_target.EntityType, Is.EqualTo(typeof(Person)));
			Assert.That(_target.UpdatedIds.Single(), Is.EqualTo(guid));
		}
	}
}
