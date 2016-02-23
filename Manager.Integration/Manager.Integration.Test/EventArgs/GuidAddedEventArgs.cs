using System;

namespace Manager.Integration.Test.EventArgs
{
	public class GuidAddedEventArgs
	{
		public GuidAddedEventArgs(Guid guid)
		{
			Guid = guid;
		}

		public Guid Guid { get; private set; }
	}
}