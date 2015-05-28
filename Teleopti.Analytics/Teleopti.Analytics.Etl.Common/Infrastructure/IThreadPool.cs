using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public interface IThreadPool
	{
		event RowsUpdatedEventHandler RowsUpdatedEvent;
		void Load(IEnumerable<ITaskParameters> taskParameters, DoWork doWork);
		void Start();
		Exception ThreadError { get; }
	}
}