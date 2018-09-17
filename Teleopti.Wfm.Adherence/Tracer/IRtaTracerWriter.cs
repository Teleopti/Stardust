namespace Teleopti.Wfm.Adherence.Tracer
{
	public interface IRtaTracerWriter
	{
		void Write<T>(RtaTracerLog<T> log);
		void Flush();
		void Clear();
		void Purge();
	}
}