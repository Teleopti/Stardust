using System;

namespace Teleopti.Ccc.DBManager.Library
{
	public interface ILog : IDisposable
	{
		void Write(string message);
	}
}