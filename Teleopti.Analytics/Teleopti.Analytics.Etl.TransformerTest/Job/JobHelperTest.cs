using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.Job
{
	[TestFixture]
	public class JobHelperTest : IDisposable
	{
		private MockRepository _mocks;
		private ILogOnHelper _logOnHelper;
		private JobHelper _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_logOnHelper = _mocks.StrictMock<ILogOnHelper>();
			_target = new JobHelper(null, null, null, _logOnHelper);
		}

		[Test]
		public void VerifyGetBusinessUnitCollection()
		{
			using(_mocks.Record())
			{
				Expect.Call(_logOnHelper.GetBusinessUnitCollection()).Return(new List<IBusinessUnit>());
			}
			using (_mocks.Playback())
			{
				Assert.AreEqual(0,_target.BusinessUnitCollection.Count);
			}
		}

		[Test]
		public void VerifyLogOnRaptor()
		{
			using (_mocks.Record())
			{
				Expect.Call(_logOnHelper.LogOn(null)).Return(true);
			}
			using (_mocks.Playback())
			{
				Assert.IsNull(_target.Repository);
				Assert.IsTrue(_target.LogOnTeleoptiCccDomain(null));
				Assert.IsNotNull(_target.Repository);
			}
		}

		[Test]
		public void VerifyLogOnRaptorFailure()
		{
			using (_mocks.Record())
			{
				Expect.Call(_logOnHelper.LogOn(null)).Return(false);
			}
			using (_mocks.Playback())
			{
				Assert.IsNull(_target.Repository);
				Assert.IsFalse(_target.LogOnTeleoptiCccDomain(null));
				Assert.IsNull(_target.Repository);
			}
		}

		[Test]
		public void VerifyLogOffRaptorAndDispose()
		{
			using (_mocks.Record())
			{
				_logOnHelper.LogOff();
				_logOnHelper.Dispose();
			}
			using (_mocks.Playback())
			{
				_target.LogOffTeleoptiCccDomain();
				_target.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				ReleaseManagedResources();
			}
			ReleaseUnmanagedResources();
		}

		protected virtual void ReleaseUnmanagedResources()
		{

		}

		protected virtual void ReleaseManagedResources()
		{
			_target = new JobHelper(null, null, null, null);
			_target.Dispose();
		}
	}
}
