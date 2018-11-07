using System;

namespace Teleopti.Wfm.Api.Query.Response
{
	public class AbsenceRequestDto
	{
		public Guid Id;
		public bool IsNew;
		public bool IsApproved;
		public bool IsPending;
		public bool IsWaitlisted;
		public bool IsAlreadyAbsent;
		public bool IsAutoAproved;
		public bool IsAutoDenied;
		public bool IsCancelled;
		public bool IsDeleted;
		public bool IsDenied;
		public bool IsExpired;
	}
}