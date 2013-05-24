using System;
using System.Data;
using NHibernate.SqlTypes;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class CompressedStringTest
	{
		private CompressedString target;
		private MockRepository mocks;
		private string string1;

		[SetUp]
		public void Setup()
		{
			string1 = "shift".ToCompressedBase64String();
			mocks = new MockRepository();
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

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void VerifyGetHashCodeFailsWithNullArgument()
		{
			target.GetHashCode(null);
		}

		[Test]
		public void VerifyNullSafeGet()
		{
			var names = new[] { "Shift" };

			var dataReader = mocks.StrictMock<IDataReader>();

			Expect.Call(dataReader.GetOrdinal(names[0])).Return(0).Repeat.AtLeastOnce();
			Expect.Call(dataReader.IsDBNull(0)).Return(true);
			Expect.Call(dataReader.IsDBNull(0)).Return(false);
			Expect.Call(dataReader[0]).Return(string1);

			mocks.ReplayAll();

			Assert.IsNull(target.NullSafeGet(dataReader, names, null));

			var document = (string)target.NullSafeGet(dataReader, names, null);
			Assert.AreEqual("shift", document);

			mocks.VerifyAll();
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void VerifyNullSafeGetExceptionWithTooManyNames()
		{
			target.NullSafeGet(null, new[] { "name1", "name2" }, null);
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