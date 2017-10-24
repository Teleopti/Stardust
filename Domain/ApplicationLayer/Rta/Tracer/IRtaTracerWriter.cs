namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public interface IRtaTracerWriter
	{
		void Write<T>(RtaTracerLog<T> log);
		void Flush();
		void Clear();
		void Purge();
	}
}