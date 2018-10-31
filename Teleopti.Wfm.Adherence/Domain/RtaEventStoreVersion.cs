namespace Teleopti.Wfm.Adherence.Domain
{
	public class RtaEventStoreVersion
	{
		public const int WithoutBelongsToDate = 1;
		public const int WithBelongsToDate = 2;
		public const int StoreVersion = WithBelongsToDate;
	}
}