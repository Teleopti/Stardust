using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using log4net;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificDataStringSerializerTest
	{
		private ISessionSpecificDataStringSerializer target;

		[SetUp]
		public void Setup()
		{
			target = new SessionSpecificDataStringSerializer(MockRepository.GenerateStub<ILog>());
		}

		[Test]
		public void ShouldSerialize()
		{
			var original = new SessionSpecificData(Guid.NewGuid(), "data source", Guid.NewGuid(), RandomName.Make());
			var serialized = target.Serialize(original);
			var deserialized = target.Deserialize(serialized);
			deserialized.PersonId.Should().Be.EqualTo(original.PersonId);
			deserialized.DataSourceName.Should().Be.EqualTo(original.DataSourceName);
			deserialized.BusinessUnitId.Should().Be.EqualTo(original.BusinessUnitId);
			deserialized.TenantPassword.Should().Be.EqualTo(original.TenantPassword);
		}

		[Test]
		public void FailingDeserializationShouldReturnNull()
		{
			target.Deserialize("Totally wrong")
				.Should().Be.Null();
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

		[Test]
		public void ShouldNotSerializeTooBig()
		{
			//just to make sure we don't make serialization bigger in the future
			//however - if we need to add stuff to the cookie for some reason, this limit needs to be increased.
			const int bytesLimit = 224;
			var testData = new SessionSpecificData(Guid.NewGuid(), "data sourceasdfasdfa", Guid.NewGuid(), "tenantPassword");
			(target.Serialize(testData).ToCharArray().Length * 2)
				.Should().Be.LessThanOrEqualTo(bytesLimit);
		}
	}
}