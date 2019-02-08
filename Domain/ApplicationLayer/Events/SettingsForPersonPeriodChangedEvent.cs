using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class SettingsForPersonPeriodChangedEvent : EventWithInfrastructureContext
	{
		private string _serializedIds;

		[NonSerialized]
		private IEnumerable<Guid> _idCollection;

		public IEnumerable<Guid> IdCollection
		{
			get
			{
				if (_idCollection != null) return _idCollection;
				_idCollection = string.IsNullOrEmpty(_serializedIds)
					? Enumerable.Empty<Guid>()
					: _serializedIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).Distinct().ToArray();
				return _idCollection;
			}
			set => _idCollection = value;
		}

		public void SetIdCollection(ICollection<Guid> idCollection)
		{
			if (idCollection != null)
			{
				_serializedIds = string.Join(",", new HashSet<Guid>(idCollection).Select(p => p.ToString()));
			}
		}

		public string SerializedIds
		{
			get
			{
				return _serializedIds;
			}
			set
			{
				_serializedIds = value;
			}
		}
	}
}