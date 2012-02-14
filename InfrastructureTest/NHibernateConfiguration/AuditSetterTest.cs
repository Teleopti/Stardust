﻿using System;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class AuditSetterTest
	{
		[Test]
		public void ShouldReturnDelegateValue()
		{
			var aSetting = new AuditSetting();
			Func<ISession, IAuditSetting> deleg = s => aSetting;
			var provider = new AuditSetter(deleg);

			provider.Entity(null).Should().Be.SameInstanceAs(aSetting);
		}

		[Test]
		public void ShouldBeAbleToSetEntity()
		{
			var aSetting = new AuditSetting();
			var provider = new AuditSetter(s => new AuditSetting());
			provider.SetEntity(aSetting);

			provider.Entity(null).Should().Be.SameInstanceAs(aSetting);
		}

		[Test]
		public void ShouldThrowIfNullIsPassedToCtor()
		{
			Assert.Throws<ArgumentNullException>(() => new AuditSetter(null));
		}


		[Test]
		public void ShouldCallDelegateOnlyOnce()
		{
			var wasCalled = false;
			var provider = new AuditSetter(s =>
			                                       {
			                                          if (wasCalled)
			                                             Assert.Fail("Should call delegate only once!");
			                                          wasCalled = true;
			                                          return new AuditSetting();
			                                       });
			provider.Entity(null);
			provider.Entity(null);
		}
	}
}