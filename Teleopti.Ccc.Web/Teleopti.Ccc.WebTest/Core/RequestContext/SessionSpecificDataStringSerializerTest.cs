using System;
using System.Runtime.Serialization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificDataStringSerializerTest
	{
		private ISessionSpecificDataStringSerializer target;

		[SetUp]
		public void Setup()
		{
			target = new SessionSpecificDataStringSerializer();
		}

		[Test]
		public void ShouldSerialize()
		{
			var original = new SessionSpecificData(Guid.NewGuid(), "data source", Guid.NewGuid(), AuthenticationTypeOption.Application);
			var serialized = target.Serialize(original);
			var deserialized = target.Deserialize(serialized);
			deserialized.PersonId.Should().Be.EqualTo(original.PersonId);
			deserialized.DataSourceName.Should().Be.EqualTo(original.DataSourceName);
			deserialized.BusinessUnitId.Should().Be.EqualTo(original.BusinessUnitId);
			deserialized.AuthenticationType.Should().Be.EqualTo(original.AuthenticationType);
		}

		[Test]
		public void FailingDeserializationShouldThrow()
		{
			Assert.Throws<SerializationException>(() => target.Deserialize("totally wrong"));
		}

		[Test]
		public void DeserializeShouldReturnNullIfUserdataIsNull()
		{
			target.Deserialize(null).Should().Be.Null();
		}

		[Test]
		public void DeserializeShouldReturnNullIfUserdataIsEmpty()
		{
			target.Deserialize(string.Empty).Should().Be.Null();
		}
	}
}