using System.Data;
using System.Data.Common;
using System.Reflection;
using NHibernate.Connection;
using NHibernate.Driver;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
    [TestFixture]
    public class TeleoptiDriverConnectionProviderTest
    {
        private TeleoptiDriverConnectionProvider target;
        private MockRepository mocks;
        private IDriver nhibDriver;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            nhibDriver = mocks.StrictMock<IDriver>();
            target = new TeleoptiDriverConnectionProvider();
	        target.SetDelayAction(() => { });
            setDriverToTargetByReflection();
        }

        [Test]
        public void VerifyNumberOfRetries()
        {
            Assert.AreEqual(3, TeleoptiDriverConnectionProvider.NumberOfRetries);
        }

        [Test]
        public void WorksFirstTime()
        {
            var retConn = mocks.DynamicMock<IDbConnection>();
            using(mocks.Record())
            {
                expectSuccessfullCallToOpenConnection(retConn);
            }
            using(mocks.Playback())
            {
                var conn = target.GetConnection();
                Assert.AreSame(retConn, conn);
            }
        }

        [Test]
        public void WorksLastTime()
        {
            var retConn = mocks.DynamicMock<IDbConnection>();
            using (mocks.Record())
            {
                expectFailedCallToOpenConnection(TeleoptiDriverConnectionProvider.NumberOfRetries - 1);
                expectSuccessfullCallToOpenConnection(retConn);
            }
            using (mocks.Playback())
            {
                var conn = target.GetConnection();
                Assert.AreSame(retConn, conn);
            }
        }

        [Test]
        public void FailsAfterNumberOfRetriesIsDone()
        {
            using (mocks.Record())
            {
                expectFailedCallToOpenConnection(TeleoptiDriverConnectionProvider.NumberOfRetries);
            }
            using (mocks.Playback())
            {
                Assert.Throws<DataSourceException>(() => target.GetConnection());
            }
        }

        private void expectFailedCallToOpenConnection(int numberOfTimes)
        {
            for (var i = 0; i < numberOfTimes; i++)
            {
                Expect.Call(nhibDriver.CreateConnection())
                    .Throw(new testException());                
            }
        }

        private void expectSuccessfullCallToOpenConnection(IDbConnection connection)
        {
            Expect.Call(nhibDriver.CreateConnection())
                .Return(connection);
            connection.ConnectionString = "doesnt matter";
            LastCall.IgnoreArguments();
            connection.Open();
        }

        private void setDriverToTargetByReflection()
        {
            typeof(ConnectionProvider)
                .GetField("driver", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, nhibDriver);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
        private class testException : DbException{}
    }
}