using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Entity;

namespace Teleopti.Analytics.Etl.CommonTest.Entity
{
	[TestFixture]
	public class BusinessUnitItemTest
	{
		[Test]
		public void ShouldBeAbleToSetProperties()
		{
			Guid newId = Guid.NewGuid();
			var target = new BusinessUnitItem { Id = newId, Name = "name" };

			target.Id.Should().Be.EqualTo(newId);
			target.Name.Should().Be.EqualTo("name");
		}
	}
}
