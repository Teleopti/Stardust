using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Events
{
    /// <summary>
    /// Teleopti's event user is one - to - one 
    /// related to the users in Raptor's
    /// user table, or at least that is the general
    /// idea. There are two approaches possible,
    /// either to use the Raptor's user table
    /// straight off or to update a user table 
    /// upon start up.
    /// </summary>
    [Serializable]
    public class EventUser : IEventUser
    {

        #region Private Member Variables

        /// <summary>
        /// The user id, a unique identifier.
        /// </summary>
        private Int32 _userId;
        /// <summary>
        /// The domain the user is on.
        /// </summary>
        private string _domain;
        /// <summary>
        /// The user name, i.e. the Windows log on.
        /// </summary>
        private string _userName;
        /// <summary>
        /// Who changed/created the object of this class.
        /// </summary>
        private string _changedBy;
        /// <summary>
        /// When was this object changed/created?
        /// </summary>
        private DateTime _changedDateTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public EventUser()
        {

        }

        /// <summary>
        /// A specific constructor used to assign values to the private
        /// properties upon construction.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="domain"></param>
        /// <param name="userName"></param>
        /// <param name="changedBy"></param>
        /// <param name="changedDateTime"></param>
        public EventUser(Int32 userId, string domain, string userName, string changedBy, DateTime changedDateTime)
        {
            _userId = userId;
            _domain = domain;
            _userName = userName;
            _changedBy = changedBy;
            _changedDateTime = changedDateTime;
        }
        
        /// <summary>
        /// Proteced construction used when Event User is deserialised.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EventUser(SerializationInfo info, StreamingContext context)
        {
            _userId = info.GetInt32("UserId");
            _domain = info.GetString("Domain");
            _userName = info.GetString("UserName");
            _changedBy = info.GetString("ChangedBy");
            _changedDateTime = info.GetDateTime("ChangedDateTime");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The user id who is subscribing to the events.
        /// This will be the hibernate user id.
        /// </summary>
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        /// <summary>
        /// The domain to which the user belongs.
        /// </summary>
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        /// <summary>
        /// The name of the user. 
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        /// <summary>
        /// The user or program that created/changed the event.
        /// </summary>
        public string ChangedBy
        {
            get { return _changedBy; }
            set { _changedBy = value; }
        }

        /// <summary>
        /// The Date Time for which the event was created/changed.
        /// </summary>
        public DateTime ChangedDateTime
        {
            get { return _changedDateTime; }
            set { _changedDateTime = value; }
        }

        #endregion

        /// <summary>
        /// The GetObjectData is a method needed in order to implement
        /// the ISerializable interface. This will give us the SerializationInfo 
        /// object containing a name value pair list of the properties of the object.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UserId", _userId, _userId.GetType());
            info.AddValue("Domain", _domain, _domain.GetType());
            info.AddValue("UserName", _userName, _userName.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
            info.AddValue("ChangedDateTime", _changedDateTime, _changedDateTime.GetType());     
        }

        /// <summary>
        /// No xml schema is implemented so this method will always return null.
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// This particular method is used for xml deserialization 
        /// and it is needed in order to implement IXmlSerializable.
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(reader.ReadOuterXml());
            XmlNodeList nodes = doc.SelectNodes("./EventUser");
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    _userId = Convert.ToInt32(node.ChildNodes.Item(0).InnerText, CultureInfo.InvariantCulture);
                    _domain = node.ChildNodes.Item(1).InnerText;
                    _userName = node.ChildNodes.Item(2).InnerText;
                    _changedBy = node.ChildNodes.Item(3).InnerText;
                    _changedDateTime = Convert.ToDateTime(node.ChildNodes.Item(4).InnerText, CultureInfo.InvariantCulture);
                }
            }
        }

        /// <summary>
        /// This particular method is used for xml serialization 
        /// and it is needed in order to implement IXmlSerializable.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("UserId", _userId.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Domain", _domain);
            writer.WriteElementString("UserName", _userName);
            writer.WriteElementString("ChangedBy", _changedBy);
            writer.WriteElementString("ChangedDateTime", _changedDateTime.ToString(CultureInfo.InvariantCulture));
        }

    }
}
