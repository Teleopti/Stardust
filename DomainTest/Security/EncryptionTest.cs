#region Imports

using System;
using System.Security.Cryptography;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;

#endregion

namespace Teleopti.Ccc.DomainTest.Security
{
    [TestFixture]
    public class EncryptionTest : IDisposable
    {
        private RijndaelManaged _myRijndael;
        private string _original;

        [SetUp]
        public void Setup()
        {
            _myRijndael = new RijndaelManaged();
            _original = typicalConfiguration();
        }

        [Test]
        public void VerifyEncryptionDecryption()
        {
            // Encrypt the string to an array of bytes.
            byte[] encrypted = Encryption.EncryptStringToBytes(_original, _myRijndael.Key, _myRijndael.IV);
            string encryptedString = Convert.ToBase64String(encrypted);

            // Decrypt the bytes to a string.
            string roundtrip = Encryption.DecryptStringFromBytes(Convert.FromBase64String(encryptedString),
                                                                 _myRijndael.Key, _myRijndael.IV);

            Assert.AreEqual(_original, roundtrip);
        }

        [Test]
        public void VerifyBase64EncryptionDecryption()
        {
            // Encrypt the string to an array of bytes.
            string encrypted = Encryption.EncryptStringToBase64(_original, _myRijndael.Key, _myRijndael.IV);

            // Decrypt the bytes to a string.
            string roundtrip = Encryption.DecryptStringFromBase64(encrypted,
                                                                  _myRijndael.Key, _myRijndael.IV);

            Assert.AreEqual(_original, roundtrip);
        }
		
        private static string typicalConfiguration()
        {
            return string.Concat(
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                   <datasource>
                    <hibernate-configuration  xmlns=""urn:nhibernate-configuration-2.2"" >
                      <session-factory name=""foo"">
                        <!-- properties -->
                        <property name=""connection.provider"">NHibernate.Connection.DriverConnectionProvider</property>
                        <property name=""connection.driver_class"">NHibernate.Driver.SqlClientDriver</property>
                        <property name=""connection.connection_string"">very secret connection string</property>
                        <property name=""show_sql"">false</property> 
                        <property name=""dialect"">NHibernate.Dialect.MsSql2008Dialect</property>
                        <property name=""use_outer_join"">true</property>
                        <property name=""default_schema"">nhtest2.dbo</property>
                      </session-factory >
                    </hibernate-configuration>
                  </datasource>
                ");
        }

        #region IDispose

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        /// <remarks>
        /// So far only managed code. No need implementing destructor.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
                if (disposing)
                {
                    ReleaseManagedResources();
                }
                ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            if (_myRijndael!=null)
                ((IDisposable)_myRijndael).Dispose();
        }

        #endregion
    }
}