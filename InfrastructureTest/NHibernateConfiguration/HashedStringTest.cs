using System;
using System.Data;
using NHibernate.SqlTypes;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
    [TestFixture]
    public class HashedStringTest
    {
        private HashedString target;
        private MockRepository mocks;
        private IOneWayEncryption encryption;
        private string string1;
        private string string2;
        private string string3;
        private ISpecification<string> isEncryptedSpecification;

        [SetUp]
        public void Setup()
        {
            string1 = "password";
            string2 = "is_safe";
            string3 = "is_safe";
            mocks = new MockRepository();
            encryption = mocks.StrictMock<IOneWayEncryption>();
            isEncryptedSpecification = mocks.StrictMock<ISpecification<string>>();
            target = new HashedString(encryption,isEncryptedSpecification);
        }

        [Test]
        public void VerifyCreateNewInstance()
        {
            target = new HashedString();
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyEquals()
        {
            Assert.IsTrue(target.Equals(null, null));
            Assert.IsFalse(target.Equals(null, string1));
            Assert.IsFalse(target.Equals(string1, null));
            Assert.IsFalse(target.Equals(string1, string2));
            Assert.IsTrue(target.Equals(string3, string2));
        }

        [Test]
        public void VerifyGetHashCode()
        {
            Assert.AreEqual(string1.GetHashCode(), target.GetHashCode(string1));
        }

        [Test,ExpectedException(typeof(ArgumentNullException))]
        public void VerifyGetHashCodeFailsWithNullArgument()
        {
            target.GetHashCode(null);
        }

        [Test]
        public void VerifyNullSafeGet()
        {
            var names = new[] {"name1"};

            IDataReader dataReader = mocks.StrictMock<IDataReader>();

            Expect.Call(dataReader.GetOrdinal(names[0])).Return(0).Repeat.AtLeastOnce();
            Expect.Call(dataReader.IsDBNull(0)).Return(true);
            Expect.Call(dataReader.IsDBNull(0)).Return(false);
            Expect.Call(dataReader[0]).Return(string1);

            mocks.ReplayAll();

            Assert.IsNull(target.NullSafeGet(dataReader,names,null));

            string document = (string)target.NullSafeGet(dataReader, names, null);
            Assert.AreEqual(string1, document);

            mocks.VerifyAll();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void VerifyNullSafeGetExceptionWithTooManyNames()
        {
            target.NullSafeGet(null, new[] {"name1", "name2"},null);
        }

        [Test]
        public void VerifyNullSafeSet()
        {
            IDbCommand dbCommand = mocks.StrictMock<IDbCommand>();
            IDataParameterCollection dataParameterCollection = mocks.StrictMock<IDataParameterCollection>();
            IDataParameter dataParameter = mocks.StrictMock<IDataParameter>();
            const string encryptedString2 = "###encrypted_is_safe###";

            Expect.Call(dbCommand.Parameters).Return(dataParameterCollection).Repeat.Twice();
            Expect.Call(dataParameterCollection[1]).Return(dataParameter).Repeat.Twice();
            Expect.Call(isEncryptedSpecification.IsSatisfiedBy(string2)).Return(false);
            Expect.Call(encryption.EncryptString(string2)).Return(encryptedString2);
            Expect.Call(dataParameter.Value = DBNull.Value);
            Expect.Call(dataParameter.Value = encryptedString2);

            mocks.ReplayAll();

            target.NullSafeSet(dbCommand, null, 1);
            target.NullSafeSet(dbCommand, string2, 1);

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyPasswordEncryptedOnlyOnce()
        {
            IDbCommand dbCommand = mocks.StrictMock<IDbCommand>();
            IDataParameterCollection dataParameterCollection = mocks.StrictMock<IDataParameterCollection>();
            IDataParameter dataParameter = mocks.StrictMock<IDataParameter>();
            const string encryptedString2 = "###encrypted_is_safe###";

            Expect.Call(dbCommand.Parameters).Return(dataParameterCollection);
            Expect.Call(dataParameterCollection[1]).Return(dataParameter);
            Expect.Call(isEncryptedSpecification.IsSatisfiedBy(encryptedString2)).Return(true);
            Expect.Call(dataParameter.Value = encryptedString2);

            mocks.ReplayAll();

            target.NullSafeSet(dbCommand, encryptedString2, 1);

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDeepCopy()
        {
            Assert.IsNull(target.DeepCopy(null));
            string document = (string)target.DeepCopy(string1);
            Assert.AreEqual(string1, document);
        }

        [Test]
        public void VerifyReplaceNotImplemented()
        {
            Assert.AreEqual(string1, target.Replace(string1, null, null));
        }

        [Test]
        public void VerifyAssemble()
        {
            Assert.IsNull(target.Assemble(null,null));
            string document = (string)target.Assemble(string2, null);
            Assert.AreEqual(string2,document);
        }

        [Test]
        public void VerifyDisassemble()
        {
            Assert.IsNull(target.Disassemble(null));
            string xmlString = (string)target.Disassemble(string2);
            Assert.AreEqual(string2, xmlString);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsInstanceOf<SqlType>(target.SqlTypes[0]);
            Assert.IsFalse(target.IsMutable);
            Assert.AreEqual(typeof(string), target.ReturnedType);
        }
    }
}