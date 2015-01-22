using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain
{
	// http://stackoverflow.com/questions/57383/in-c-how-can-i-rethrow-innerexception-without-losing-stack-trace
	public static class PreserveStack
	{
		public static void ForInnerOf(Exception x)
		{
			var e = x.InnerException;
			if (e == null)
				return;

			var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var mgr = new ObjectManager(null, ctx);
			var si = new SerializationInfo(e.GetType(), new FormatterConverter());

			e.GetObjectData(si, ctx);
			mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
			mgr.DoFixups(); // ObjectManager calls SetObjectData

			// voila, e is unmodified save for _remoteStackTraceString
		}
	}
}