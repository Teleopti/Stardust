using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
    /// <summary>
    /// MessageBrokerSupport attribute that signals 
    /// that the attribute will be picked up
    /// by Message Brokern.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-22
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class MessageBrokerSupportAttribute : Attribute
    {
    }
}
