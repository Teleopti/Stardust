
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalize the person period message.
	/// </summary>
	[Serializable]
	public class PersonPeriodChangedMessage : MessageWithLogOnContext
	{
		private readonly Guid _messageId = Guid.NewGuid();
		private string _serializedPersonPeriod;

		[NonSerialized]
		private ICollection<Guid> _personIdCollection;

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

        /// <summary>
        /// This property will return the person id collection
        /// </summary>
		public ICollection<Guid> PersonIdCollection
		{
			get
			{
				if (_personIdCollection == null)
				{
					_personIdCollection = new Collection<Guid>();
					if (!string.IsNullOrEmpty(_serializedPersonPeriod))
					{
						var items = _serializedPersonPeriod.Split(',');
						foreach (var item in items)
						{
							_personIdCollection.Add(new Guid(item));
						}
					}
				}
				return _personIdCollection;
			}
		}

        /// <summary>
        /// set the person id collection
        /// </summary>
        /// <param name="personIdCollection"></param>
		public void SetPersonIdCollection(ICollection<Guid> personIdCollection)
		{
			if(personIdCollection != null )
			{
                var stringCollection = new string[personIdCollection.Count];
                var index = 0;
                foreach (var guid in personIdCollection)
                {
                    stringCollection[index] = guid.ToString();
                    index++;
                }
                _serializedPersonPeriod = string.Join(",", stringCollection);
			}
            
		}

        /// <summary>
        /// Return the serialized person period
        /// </summary>
        public string SerializedPersonPeriod
        {
            get
            {
                return _serializedPersonPeriod;
            }
            set
            {
                _serializedPersonPeriod = value;
            }
        }
	}
}
