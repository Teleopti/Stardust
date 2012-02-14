using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.Management.BindingLists
{
    public class AddressBindingList : BindingList<IMessageInformation>
    {
        public AddressBindingList(IList<IMessageInformation> addresses) : base(addresses)
        {
            AddingNew += new AddingNewEventHandler(OnAddingNew);
            RaiseListChangedEvents = true;
        }

        // Create a new part from the text in the two text boxes.
#pragma warning disable 1692
        private void OnAddingNew(object sender, AddingNewEventArgs e)
        {
            e.NewObject = new MessageInformation();
        }
#pragma warning restore 1692

        public IList<IMessageInformation> Addresses
        {
            get { return Items; }
        }

    }
}