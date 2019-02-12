using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class OptionalColumnCollectionChangedEvent: EventWithInfrastructureContext
	{
		private string _serializedOptionalColumn;

		[NonSerialized]
		private IEnumerable<Guid> _optionalColumnIdCollection;

		public IEnumerable<Guid> OptionalColumnIdCollection
		{
			get
			{
				if (_optionalColumnIdCollection != null) return _optionalColumnIdCollection;
				_optionalColumnIdCollection = string.IsNullOrEmpty(_serializedOptionalColumn)
					? Enumerable.Empty<Guid>()
					: _serializedOptionalColumn.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).Distinct().ToArray();
				return _optionalColumnIdCollection;
			}
			set => _optionalColumnIdCollection = value;
		}

		public void SetOptionalColumnIdCollection(ICollection<Guid> optionalColumnIdCollection)
		{
			if (optionalColumnIdCollection != null)
			{
				_serializedOptionalColumn = string.Join(",", new HashSet<Guid>(optionalColumnIdCollection).Select(p => p.ToString()));
			}
		}

		public string SerializedOptionalColumn
		{
			get
			{
				return _serializedOptionalColumn;
			}
			set
			{
				_serializedOptionalColumn = value;
			}
		}
	}
}