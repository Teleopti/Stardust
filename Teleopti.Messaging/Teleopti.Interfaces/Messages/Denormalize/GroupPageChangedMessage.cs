﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalize the grouping message.
	/// </summary>
	[Serializable]
	public class GroupPageChangedMessage : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();
		private string _serializedGroupPageId;

		[NonSerialized]
		private ICollection<Guid> _groupPageIdCollection;

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		public ICollection<Guid> GroupPageIdCollection
		{
			get
			{
				if (_groupPageIdCollection == null)
				{
					_groupPageIdCollection = new Collection<Guid>();
					if (!string.IsNullOrEmpty(_serializedGroupPageId))
					{
						var items = _serializedGroupPageId.Split(',');
						foreach (var item in items)
						{
							_groupPageIdCollection.Add(new Guid(item));
						}
					}
				}
				return _groupPageIdCollection;
			}
		}

		public void SetGroupPageIdCollection(ICollection<Guid> groupPageIdCollection)
		{
			var stringCollection = new string[groupPageIdCollection.Count];
			var index = 0;
			foreach (var guid in groupPageIdCollection)
			{
				stringCollection[index] = guid.ToString();
				index++;
			}
			_serializedGroupPageId = string.Join(",", stringCollection);
		}
	}
}
