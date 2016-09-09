using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class DataSourceState
	{
		[ThreadStatic]
		private static Stack<IDataSource> _threadDataSources;

		public IDisposable SetOnThread(IDataSource dataSource)
		{
			if (_threadDataSources == null)
				_threadDataSources = new Stack<IDataSource>();
			_threadDataSources.Push(dataSource);
			return new GenericDisposable(() =>
			{
				_threadDataSources.Pop();
			});
		}

		public IDataSource Get()
		{
			if (_threadDataSources == null) return null;
			if (_threadDataSources.Count == 0) return null;
			return _threadDataSources.Peek();
		}

	}
}