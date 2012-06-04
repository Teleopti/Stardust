using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Entity;

namespace Teleopti.Analytics.Etl.CommonTest.Entity
{
	[TestFixture]
	public class BusinessUnitItemTest
	{
		private BusinessUnitItem _target;
		private Guid _newId;

		[SetUp]
		public void Setup()
		{
			_newId = Guid.NewGuid();
			_target = new BusinessUnitItem { Id = _newId, Name = "name" };
		}

		[Test]
		public void ShouldBeAbleToSetProperties()
		{
			_target.Id.Should().Be.EqualTo(_newId);
			_target.Name.Should().Be.EqualTo("name");
		}
	}
}
