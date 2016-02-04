using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalize the grouping message.
	/// </summary>
	[Serializable]
	public class GroupPageChangedMessage : MessageWithLogOnContext
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

        /// <summary>
        /// This property will return the group page id collection
        /// </summary>
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

        /// <summary>
        /// Set the group id collection
        /// </summary>
        /// <param name="groupPageIdCollection"></param>
		public void SetGroupPageIdCollection(ICollection<Guid> groupPageIdCollection)
		{
			if (groupPageIdCollection != null)
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

        /// <summary>
        /// Return the serialized group page
        /// </summary>
        public string SerializedGroupPage
        {
            get
            {
                return _serializedGroupPageId;
            }
            set
            {
                _serializedGroupPageId = value;
            }
        }
	}
}
