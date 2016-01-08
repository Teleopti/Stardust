using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job
{
	[TestFixture]
	public class JobHelperTest : IDisposable
	{
		private ILogOnHelper _logOnHelper;
		private JobHelper _target;

		[SetUp]
		public void Setup()
		{
			_logOnHelper = MockRepository.GenerateMock<ILogOnHelper>();
			_target = new JobHelper(null, null, _logOnHelper);
		}

		[Test]
		public void VerifyGetBusinessUnitCollection()
		{
			_logOnHelper.Stub(x => x.GetBusinessUnitCollection()).Return(new List<IBusinessUnit>());
			Assert.AreEqual(0,_target.BusinessUnitCollection.Count);
		}

		[Test]
		public void VerifyLogOnRaptor()
		{
			var container = MockRepository.GenerateMock<IDataSourceContainer>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();

			_logOnHelper.Stub(x => x.SetBusinessUnit(null)).Return(true);
			_logOnHelper.Stub(x => x.SelectedDataSourceContainer).Return(container);
			container.Stub(x => x.DataSource).Return(dataSource);
			dataSource.Stub(x => x.Statistic).Return(uowFactory);
			uowFactory.Stub(x => x.ConnectionString).Return("asortofconnectionstring");
			Assert.IsNull(_target.Repository);
			Assert.IsTrue(_target.SetBusinessUnit(null));
			Assert.IsNotNull(_target.Repository);
			
		}

		[Test]
		public void VerifyLogOnRaptorFailure()
		{
			_logOnHelper.Stub(x => x.SetBusinessUnit(null)).Return(false);
			Assert.IsNull(_target.Repository);
			Assert.IsFalse(_target.SetBusinessUnit(null));
			Assert.IsNull(_target.Repository);
		}

		[Test]
		public void VerifyLogOffRaptorAndDispose()
		{
			_logOnHelper.Stub(x => x.Dispose());
			
			_target.LogOffTeleoptiCccDomain();
			_target.Dispose();
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
			_target = new JobHelper(null, null, _logOnHelper);
			_target.Dispose();
		}
	}
}
