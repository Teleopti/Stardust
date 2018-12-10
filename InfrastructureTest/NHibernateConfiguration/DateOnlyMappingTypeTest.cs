using System;
using System.Data;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;


namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	[Category("BucketB")]
	public class DateOnlyMappingTypeTest
	{
		private DateOnlyMappingType _target;

		[SetUp]
		public void Setup()
		{
			_target = new DateOnlyMappingType();
		}

		/// <summary>
		/// Determines whether this instance [can create instance].
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void CanCreateInstance()
		{
			Assert.IsNotNull(_target);
		}

		/// <summary>
		/// Determines whether this instance [can deep copy].
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void CanDeepCopy()
		{
			DateOnly firstDateOnly = DateOnly.Today;
			DateOnly secondDateOnly = (DateOnly)_target.DeepCopy(firstDateOnly);

			Assert.AreNotSame(secondDateOnly, firstDateOnly);
			Assert.IsTrue(firstDateOnly.Equals(secondDateOnly));

			Assert.IsNull(_target.DeepCopy(null));
		}

		/// <summary>
		/// Verifies the SQL types.
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void VerifySqlTypes()
		{
			Assert.AreEqual(1, _target.SqlTypes.Length);
			SqlType dateSqlType = new SqlType(DbType.Date);
			Assert.IsAssignableFrom(dateSqlType.GetType(), _target.SqlTypes[0]);
		}

		/// <summary>
		/// Verifies the equals.
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void VerifyEquals()
		{
			IUserType iUserType = _target;
			DateOnly firstDateOnly = DateOnly.Today;
			DateOnly secondDateOnly = firstDateOnly;
			Assert.IsTrue(iUserType.Equals(firstDateOnly, secondDateOnly));

			secondDateOnly = (DateOnly)_target.DeepCopy(firstDateOnly);
			Assert.IsTrue(iUserType.Equals(firstDateOnly, secondDateOnly));

			Assert.IsTrue(iUserType.Equals(null, null));
			Assert.IsFalse(iUserType.Equals(firstDateOnly, null));

		}

		/// <summary>
		/// Determines whether this instance can assemble.
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void CanAssemble()
		{
			DateOnly dateOnly1 = new DateOnly(DateTime.MinValue);
			DateOnly dateOnly2 = (DateOnly)_target.Assemble(dateOnly1, null);
			Assert.AreEqual(dateOnly1, dateOnly2);

		}

		/// <summary>
		/// Determines whether this instance can disassemble.
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void CanDisassemble()
		{
			DateOnly dateOnly1 = new DateOnly(DateTime.MinValue);
			DateOnly dateOnly2 = (DateOnly)_target.Disassemble(dateOnly1);
			Assert.AreEqual(dateOnly1, dateOnly2);
		}

		[Test]
		public void CanGetHashCode()
		{
			DateOnly dateOnly1 = new DateOnly(DateTime.MinValue.AddYears(2007));
			DateOnly dateOnly2 = new DateOnly(DateTime.MinValue.AddYears(2007));
			DateOnly dateOnly3 = new DateOnly(DateTime.MinValue.AddYears(2007).AddDays(1));

			int hash1 = _target.GetHashCode(dateOnly1);
			int hash2 = _target.GetHashCode(dateOnly2);
			int hash3 = _target.GetHashCode(dateOnly3);

			Assert.AreEqual(hash1, hash2);
			Assert.AreNotEqual(hash1, hash3);
			Assert.AreEqual(hash1, _target.GetHashCode(new DateOnly(DateTime.MinValue.AddYears(2007))));

		}

		/// <summary>
		/// Verifies the is mutable.
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void VerifyIsMutable()
		{
			Assert.IsTrue(_target.IsMutable);
		}

		/// <summary>
		/// Verifies the type of the returned.
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void VerifyReturnedType()
		{
			Assert.AreEqual(typeof(DateOnly), _target.ReturnedType);

		}

		/// <summary>
		/// Determines whether this instance can replace.
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void CanReplace()
		{
			Assert.AreEqual(null, _target.Replace(null, null, null));
		}
	}
}