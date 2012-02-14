using System;
using NHibernate.Cfg;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
    /// <summary>
    /// Tests for TeleoptiDatabaseNamingStrategyTest
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class TeleoptiDatabaseNamingStrategyTest
    {
        private INamingStrategy target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = TeleoptiDatabaseNamingStrategy.Instance;
        }

        /// <summary>
        /// Verifies ClassToTableName method.
        /// </summary>
        [Test]
        public void VerifyClassToTableName()
        {
            Assert.AreEqual("apa", target.ClassToTableName("apa"));
            Assert.AreEqual("apa", target.ClassToTableName("gris.apa"));
            Assert.AreEqual(string.Empty, target.ClassToTableName(null));
        }

        /// <summary>
        /// Verifies ClassToTableName method.
        /// </summary>
        [Test]
        public void VerifyPropertyToColumnName()
        {
            Assert.AreEqual("apa", target.PropertyToColumnName("apa"));
            Assert.AreEqual("apa", target.PropertyToColumnName("gris.apa"));
            Assert.AreEqual(string.Empty, target.PropertyToColumnName(null));
        }

        /// <summary>
        /// Verifies TableName method.
        /// </summary>
        [Test]
        public void VerifyTableName()
        {
            Assert.AreEqual("apa", target.TableName("apa"));
        }

        /// <summary>
        /// Verifies ColumnName method.
        /// </summary>
        [Test]
        public void VerifyColumnName()
        {
            Assert.AreEqual("apa", target.TableName("apa"));
        }

        /// <summary>
        /// Verifies PropertyToTableName method.
        /// </summary>
        [Test]
        public void VerifyPropertyToTableName()
        {
            Assert.AreEqual("apa", target.PropertyToTableName(string.Empty, "apa"));
            Assert.AreEqual("apa", target.PropertyToTableName(null, "gris.apa"));
            Assert.AreEqual(string.Empty, target.PropertyToTableName("roger", null));
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void VerifyLogicalColumnName()
        {
            target.LogicalColumnName("sdf", "sdfsf");
        }
    }
}