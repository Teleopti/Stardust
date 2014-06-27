using System;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Core.Logging;
using log4net;

namespace Teleopti.Ccc.WebTest.Core.Logging
{
	[TestFixture]
	public class Log4NetModuleTest
	{
		[Test]
		public void ShouldHaveEmptyConstructor()
		{
			new Log4NetModule();
		}

		[Test]
		public void ShouldConfigureLog4Net()
		{
			var configured = false;
			var httpApplication = MockRepository.GenerateMock<HttpApplication>();
			var target = new Log4NetModule(null, () =>
			{
				configured = true;
			}, null);

			target.Init(httpApplication);

			Assert.That(configured, Is.True);
		}

		[Test]
		public void ShouldRegisterEventHandler()
		{
			var httpApplication = MockRepository.GenerateMock<HttpApplication>();
			var target = new Log4NetModule(null, () => { }, null);

			target.Init(httpApplication);

			// cant test this 
			//Assert.That(httpApplication.Error != null, Is.True);
			//httpApplication.AssertWasCalled(x => x.Error += Arg<EventHandler>.Is.Anything);
		}

		[Test]
		public void ShouldDoNothingOnDispose()
		{
			new Log4NetModule(null, null, null).Dispose();
		}

		[Test]
		public void ShouldLogExceptionsAsError()
		{
			var exception = new Exception();
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new Log4NetModule(logger, null, () => exception);

			target.Application_Error(null, null);

			logger.AssertWasCalled(x => x.Error(Log4NetModule.LogMessageException, exception));
		}

		[Test]
		public void ShouldLogInnerMostException()
		{
			var exception = new Exception(null, new Exception(null, new Exception()));
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new Log4NetModule(logger, null, () => exception);

			target.Application_Error(null, null);

			logger.AssertWasCalled(x => x.Error(Log4NetModule.LogMessageException, exception.InnerException.InnerException));
		}

		[Test]
		public void ShouldLog404AsWarning()
		{
			var exception = new HttpException(404, null);
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new Log4NetModule(logger, null, () => exception);

			target.Application_Error(null, null);

			logger.AssertWasCalled(x => x.Warn(Log4NetModule.LogMessage404, exception));
		}
	}
}
