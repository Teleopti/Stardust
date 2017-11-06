using System;
using NHibernate.SqlTypes;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class CompressedStringTest
	{
		private CompressedString target;
		private string string1;

		[SetUp]
		public void Setup()
		{
			string1 = "shift".ToCompressedBase64String();
			target = new CompressedString();
		}

		[Test]
		public void VerifyCreateNewInstance()
		{
			target = new CompressedString();
			Assert.IsNotNull(target);
		}

		[Test]
		public void VerifyEquals()
		{
			Assert.IsTrue(target.Equals("shift1", "shift1"));
		}

		[Test]
		public void VerifyGetHashCode()
		{
			Assert.AreEqual("shift1".GetHashCode(), target.GetHashCode("shift1"));
		}

		[Test]
		public void VerifyGetHashCodeFailsWithNullArgument()
		{
			Assert.Throws<ArgumentNullException>(() => target.GetHashCode(null));
		}

		[Test]
		public void VerifyNullSafeGetExceptionWithTooManyNames()
		{
			Assert.Throws<InvalidOperationException>(() => target.NullSafeGet(null, new[] { "name1", "name2" }, null, null));
		}

		[Test]
		public void VerifyDeepCopy()
		{
			Assert.IsNull(target.DeepCopy(null));
			var document = (string)target.DeepCopy(string1);
			Assert.AreEqual(string1, document);
		}

		[Test]
		public void VerifyProperties()
		{
			Assert.IsInstanceOf<SqlType>(target.SqlTypes[0]);
			Assert.IsTrue(target.IsMutable);
			Assert.AreEqual(typeof(string), target.ReturnedType);
		}
	}
}