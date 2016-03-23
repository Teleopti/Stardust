using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class SettingsForPersonPeriodChangedEvent : EventWithInfrastructureContext
	{
		private string _serializedIds;

		[NonSerialized]
		private ICollection<Guid> _idCollection;

		public ICollection<Guid> IdCollection
		{
			get
			{
				if (_idCollection == null)
				{
					_idCollection = new List<Guid>();
					if (!string.IsNullOrEmpty(_serializedIds))
					{
						var items = _serializedIds.Split(',');
						foreach (var item in items)
						{
							_idCollection.Add(new Guid(item));
						}
					}
				}
				return _idCollection;
			}
		}

		public void SetIdCollection(ICollection<Guid> idCollection)
		{
			if (idCollection != null)
			{
				var stringCollection = new string[idCollection.Count];
				var index = 0;
				foreach (var guid in idCollection)
				{
					stringCollection[index] = guid.ToString();
					index++;
				}
				_serializedIds = string.Join(",", stringCollection);
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