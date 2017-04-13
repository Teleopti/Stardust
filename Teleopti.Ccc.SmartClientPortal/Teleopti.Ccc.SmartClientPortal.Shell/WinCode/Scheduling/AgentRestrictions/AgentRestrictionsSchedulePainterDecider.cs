using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	public class AgentRestrictionsSchedulePainterDecider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaint(IPreferenceCellData preferenceCellData)
		{
			return (preferenceCellData.SchedulingOption.UseScheduling && (preferenceCellData.HasDayOff || preferenceCellData.HasShift || preferenceCellData.HasFullDayAbsence));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintFullDayAbsence(IPreferenceCellData preferenceCellData)
		{
			return (preferenceCellData.SchedulingOption.UseScheduling && preferenceCellData.HasFullDayAbsence);
			//return (preferenceCellData.SchedulingOption.UseScheduling && preferenceCellData.HasFullDayAbsence && !preferenceCellData.HasDayOff);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintFullDayAbsenceOnContractDayOff(IPreferenceCellData preferenceCellData)
		{
			return (preferenceCellData.SchedulingOption.UseScheduling && preferenceCellData.HasAbsenceOnContractDayOff);
			//return (preferenceCellData.SchedulingOption.UseScheduling && preferenceCellData.HasFullDayAbsence && !preferenceCellData.HasDayOff && preferenceCellData.HasAbsenceOnContractDayOff);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintDayOff(IPreferenceCellData preferenceCellData)
		{
			return (preferenceCellData.SchedulingOption.UseScheduling && preferenceCellData.HasDayOff);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool ShouldPaintMainShift(IPreferenceCellData preferenceCellData)
		{
			if (preferenceCellData.SchedulingOption.UseRotations && preferenceCellData.HasFullDayAbsence) return false;
			return (preferenceCellData.SchedulingOption.UseScheduling && !preferenceCellData.HasFullDayAbsence && !preferenceCellData.HasDayOff && preferenceCellData.HasShift);
		}
	}
}
