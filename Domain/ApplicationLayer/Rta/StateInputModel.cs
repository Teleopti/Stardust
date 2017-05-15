using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IValidatable
	{
		string AuthenticationKey { get; set; }
		string SourceId { get; set; }
	}

	public class CloseSnapshotInputModel : IValidatable
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public DateTime SnapshotId { get; set; }

		// for logging
		public override string ToString()
		{
			return $"AuthenticationKey: {AuthenticationKey}, SourceId: {SourceId}, SnapshotId: {SnapshotId}";
		}
	}

	public class BatchInputModel : IValidatable
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public DateTime? SnapshotId { get; set; }
		public IEnumerable<BatchStateInputModel> States { get; set; }

		// for logging
		public override string ToString()
		{
			var states = States.EmptyIfNull()
				.Select(x => x.ToString())
				.Aggregate((current, next) => current + ", " + next);
			return $"AuthenticationKey: {AuthenticationKey}, SourceId: {SourceId}, SnapshotId: {SnapshotId}, States: {states}";
		}

	}

	public class BatchStateInputModel
	{
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }

		// for logging
		public override string ToString()
		{
			return $"UserCode: {UserCode}, StateCode: {StateCode}, StateDescription: {StateDescription}";
		}

	}

}