using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTransactionHook : ITransactionHook
	{
		private bool _fails;

		public IEnumerable<IRootChangeInfo> AfterFlushInvokedWith;
		public IEnumerable<object> ModifiedRoots;

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			AfterFlushInvokedWith = modifiedRoots;
			ModifiedRoots = AfterFlushInvokedWith.Select(x => x.Root).ToArray();
			if (_fails)
				throw new Exception("Ok, lets fail");
		}

		public void Fails()
		{
			_fails = true;
		}

		public void Clear()
		{
			AfterFlushInvokedWith = null;
			ModifiedRoots = null;
			_fails = false;
		}
	}
}