namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceMapper
	{
		public AdherenceState GetNeutralIfUnknownAdherence(AdherenceState adherence)
		{
			return adherence == AdherenceState.Unknown ? AdherenceState.Neutral : adherence;
		}
	}
}