using System;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class LogWriterTest
    {
        [Test]
        public void VerifyTwoLogWriterSharesSameLogObject()
        {
            LogWriter<object> writer1 = new LogWriter<object>();
            LogWriter<object> writer2 = new LogWriter<object>();
            Assert.AreSame(writer1.Log, writer2.Log);
        }
		
        [Test]
        public void VerifyWriterOfDifferentTypeHaveDifferentLogObject()
        {
            LogWriter<Person> writer1 = new LogWriter<Person>();
            LogWriter<object> writer2 = new LogWriter<object>();
            Assert.AreNotSame(writer1.Log, writer2.Log);
        }
		
        [Test]
        public void VerifyInnerLogCalled()
        {
            ILog innerLogger = MockRepository.GenerateMock<ILog>();
            LogWriter<object> writer1 = new LogWriter<object>();
            LogWriter<object>.SetExplicitLog(innerLogger);

	        innerLogger.Stub(x => x.IsInfoEnabled).Return(true);

	        writer1.LogInfo(()=>$"Log Message");

	        innerLogger.AssertWasCalled(x => x.Info("Log Message"));
		}

	    [Test]
	    public void ShouldNotEvaluateWhenInfoDisabled()
	    {
		    ILog innerLogger = MockRepository.GenerateMock<ILog>();
		    LogWriter<object> writer1 = new LogWriter<object>();
		    LogWriter<object>.SetExplicitLog(innerLogger);
		    int increase = 0;
		    Func<int> action = () => {
				increase++;
			    return increase;
		    };

		    innerLogger.Stub(x => x.IsInfoEnabled).Return(false);

		    writer1.LogInfo(()=>$"Log Message {action()}");

		    increase.Should().Be.EqualTo(0);
	    }
	}
}
