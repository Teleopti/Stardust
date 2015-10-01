using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain
{
	// http://stackoverflow.com/questions/57383/in-c-how-can-i-rethrow-innerexception-without-losing-stack-trace
	public static class PreserveStack
	{
		public static void ForInnerOf(Exception e)
		{
			if (e.InnerException == null)
				return;
			For(e.InnerException);
		}

		public static void For(Exception e)
		{
			var typeOfException = e.GetType();
			if (exceptionIsNonSerializable(typeOfException))
			{
				return;
			}

			var ctx = new StreamingContext(StreamingContextStates.CrossAppDomain);
			var mgr = new ObjectManager(null, ctx);
			var si = new SerializationInfo(typeOfException, new FormatterConverter());

			e.GetObjectData(si, ctx);
			mgr.RegisterObject(e, 1, si); // prepare for SetObjectData
			mgr.DoFixups(); // ObjectManager calls SetObjectData

			// voila, e is unmodified save for _remoteStackTraceString
		}

		private static bool exceptionIsNonSerializable(Type type)
		{
			return type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new[] {typeof (SerializationInfo), typeof (StreamingContext)}, null) == null;
		}
	}
}