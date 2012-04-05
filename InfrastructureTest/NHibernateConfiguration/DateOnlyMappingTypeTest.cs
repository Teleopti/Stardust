using System;
using System.Data;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	[Category("LongRunning")]
	public class DateOnlyMappingTypeTest
	{
		private DateOnlyMappingType _target;
		private MockRepository mocks;

		[SetUp]
		public void Setup()
		{
			_target = new DateOnlyMappingType();
			mocks = new MockRepository();
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
		/// Determines whether this instance [can null safe get].
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void CanNullSafeGet()
		{
			IDataReader dataReader = mocks.StrictMock<IDataReader>();
			Expect.Call(dataReader.IsDBNull(0)).Return(false);
			Expect.Call(dataReader.GetOrdinal("Kalle")).Return(0);
			Expect.Call(dataReader[0]).Return(DateTime.MinValue);

			Expect.Call(dataReader.IsDBNull(0)).Return(true);
			Expect.Call(dataReader.GetOrdinal("Kalle")).Return(0);

			mocks.ReplayAll();

			object gotObject = _target.NullSafeGet(dataReader, new[] { "Kalle" }, null);
			Assert.IsNotNull(gotObject);

			DateOnly gotDateOnly = (DateOnly)gotObject;
			Assert.IsTrue(gotDateOnly.Equals(new DateOnly(DateTime.MinValue)));

			Assert.IsNull(_target.NullSafeGet(dataReader, new[] { "Kalle" }, null));

			mocks.VerifyAll();
		}

		/// <summary>
		/// Determines whether this instance [can null safe set].
		/// </summary>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2008-11-24
		/// </remarks>
		[Test]
		public void CanNullSafeSet()
		{
			var minDate = new DateOnly(DateTime.MinValue);

			IDbCommand dbCommand = mocks.StrictMock<IDbCommand>();
			IDataParameter dataParameter = mocks.StrictMock<IDataParameter>();
			IDataParameterCollection dataParameterCollection = mocks.StrictMock<IDataParameterCollection>();
			using (mocks.Record())
			{
				Expect.Call(dataParameterCollection[0]).Return(dataParameter);
				Expect.Call(dbCommand.Parameters)
					.Return(dataParameterCollection);
				Expect.Call(dataParameter.Value).PropertyBehavior();
			}

			_target.NullSafeSet(dbCommand, minDate, 0);

			Assert.IsNotNull(_target);
			Assert.AreEqual(minDate.Date, dataParameter.Value);

			dbCommand = mocks.StrictMock<IDbCommand>();
			dataParameter = mocks.StrictMock<IDataParameter>();
			dataParameterCollection = mocks.StrictMock<IDataParameterCollection>();
			using (mocks.Record())
			{
				Expect.Call(dataParameterCollection[0]).Return(dataParameter);
				Expect.Call(dbCommand.Parameters)
					.Return(dataParameterCollection);
				Expect.Call(dataParameter.Value).PropertyBehavior();
			}
			_target.NullSafeSet(dbCommand, null, 0);
			Assert.AreEqual(DBNull.Value, dataParameter.Value);

			mocks.VerifyAll();
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