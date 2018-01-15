﻿using System;
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
			var target = new Log4NetModule(() =>
			{
				configured = true;
			}, null, null);

			target.Init(httpApplication);

			Assert.That(configured, Is.True);
		}

		[Test]
		public void ShouldRegisterEventHandler()
		{
			var httpApplication = MockRepository.GenerateMock<HttpApplication>();
			var target = new Log4NetModule(() => { }, null, null);

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
			var target = new Log4NetModule(null, () => exception, new Log4NetLogger(logger));

			target.Application_Error(null, null);

			logger.AssertWasCalled(x => x.Error(Log4NetModule.LogMessageException, exception));
		}

		[Test]
		public void ShouldLog404AsWarning()
		{
			var exception = new HttpException(404, null);
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new Log4NetModule(null, () => exception, new Log4NetLogger(logger));

			target.Application_Error(null, null);

			logger.AssertWasCalled(x => x.Warn(Log4NetModule.LogMessage404, exception));
		}

		[Test]
		public void ShouldIgnoreRemoteHostClosedConnection80070057()
		{
			var exception = new HttpException(500, null, unchecked((int)0x80070057));
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new Log4NetModule(null, () => exception, new Log4NetLogger(logger));

			target.Application_Error(null, null);

			logger.AssertWasNotCalled(x => x.Warn(Arg<string>.Is.Anything, Arg<HttpException>.Is.Anything));
			logger.AssertWasNotCalled(x => x.Error(Arg<string>.Is.Anything, Arg<HttpException>.Is.Anything));
		}

		[Test]
		public void ShouldIgnoreRemoteHostClosedConnection800703E3()
		{
			var exception = new HttpException(500, null, unchecked((int)0x800703E3));
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new Log4NetModule(null, () => exception, new Log4NetLogger(logger));

			target.Application_Error(null, null);

			logger.AssertWasNotCalled(x => x.Warn(Arg<string>.Is.Anything, Arg<HttpException>.Is.Anything));
			logger.AssertWasNotCalled(x => x.Error(Arg<string>.Is.Anything, Arg<HttpException>.Is.Anything));
		}
		[Test]
		public void ShouldIgnoreInnerMostRemoteHostClosedConnection80070057()
		{
			var httpException = new HttpException(500, null, unchecked((int)0x80070057));
			var exception = new HttpUnhandledException("", httpException);
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new Log4NetModule(null, () => exception, new Log4NetLogger(logger));

			target.Application_Error(null, null);

			logger.AssertWasNotCalled(x => x.Warn(Arg<string>.Is.Anything, Arg<HttpException>.Is.Anything));
			logger.AssertWasNotCalled(x => x.Error(Arg<string>.Is.Anything, Arg<HttpException>.Is.Anything));
		}
	}
}
