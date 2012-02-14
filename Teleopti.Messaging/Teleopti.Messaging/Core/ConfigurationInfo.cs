using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.Core
{
    [Serializable]
    public class ConfigurationInfo : IConfigurationInfo
    {
        private Int32 _configurationId;
        private string _configurationType;
        private string _configurationName;
        private string _configurationValue;
        private string _configurationDataType;
        private string _changedBy;
        private DateTime _changedDateTime;

        public ConfigurationInfo()
        {
        }

        public ConfigurationInfo(Int32 configurationId,
                                 string configurationType, 
                                 string configurationName, 
                                 string configurationValue, 
                                 string configurationDataType, 
                                 string changedBy, 
                                 DateTime changedDateTime)
        {
            _configurationId = configurationId;
            _configurationType = configurationType;
            _configurationName = configurationName;
            _configurationValue = configurationValue;
            _configurationDataType = configurationDataType;
            _changedBy = changedBy;
            _changedDateTime = changedDateTime;
        }

        protected ConfigurationInfo(SerializationInfo info, StreamingContext context)
        {
            _configurationId = info.GetInt32("ConfigurationId");
            _configurationType = info.GetString("ConfigurationType");
            _configurationName = info.GetString("ConfigurationName");
            _configurationValue = info.GetString("ConfigurationValue");
            _configurationDataType = info.GetString("ConfigurationDataType");
            _changedBy = info.GetString("ChangedBy");
            _changedDateTime = info.GetDateTime("ChangedDateTime");
        }

        public Int32 ConfigurationId
        {
            get { return _configurationId;  }
            set { _configurationId = value;  }
        }

        public string ConfigurationType
        {
            get { return _configurationType;  }
            set { _configurationType = value; }
        }

        public string ConfigurationName
        {
            get { return _configurationName;  }
            set { _configurationName = value; }
        }

        public string ConfigurationValue
        {
            get { return _configurationValue;  }
            set { _configurationValue = value; }
        }

        public string ConfigurationDataType
        {
            get { return _configurationDataType;  }
            set { _configurationDataType = value; }
        }

        public string ChangedBy
        {
            get { return _changedBy;  }
            set { _changedBy = value; }
        }

        public DateTime ChangedDateTime
        {
            get { return _changedDateTime;  }
            set { _changedDateTime = value; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ConfigurationId", _configurationId, _configurationId.GetType());
            info.AddValue("ConfigurationType", _configurationType, _configurationType.GetType());
            info.AddValue("ConfigurationName", _configurationName, _configurationName.GetType());
            info.AddValue("ConfigurationValue", _configurationValue, _configurationValue.GetType());
            info.AddValue("ConfigurationDataType", _configurationDataType, _configurationDataType.GetType());
            info.AddValue("ChangedBy", _changedBy, _changedBy.GetType());
            info.AddValue("ChangedDateTime", _changedDateTime, _changedDateTime.GetType());  
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(reader.ReadOuterXml());
            XmlNodeList nodes = doc.SelectNodes("./ConfigurationInfo");
            if (nodes != null)
                foreach (XmlNode node in nodes)
                {
                    _configurationId = Convert.ToInt32(node.ChildNodes.Item(0).InnerText, CultureInfo.InvariantCulture);
                    _configurationType = node.ChildNodes.Item(1).InnerText;
                    _configurationName = node.ChildNodes.Item(2).InnerText;
                    _configurationValue = node.ChildNodes.Item(3).InnerText;
                    _configurationDataType = node.ChildNodes.Item(4).InnerText;
                    _changedBy = node.ChildNodes.Item(5).InnerText;
                    _changedDateTime = Convert.ToDateTime(node.ChildNodes.Item(6).InnerText, CultureInfo.InvariantCulture);
                }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("ConfigurationId", _configurationId.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ConfigurationType", _configurationType);
            writer.WriteElementString("ConfigurationName", _configurationName);
            writer.WriteElementString("ConfigurationValue", _configurationValue);
            writer.WriteElementString("ConfigurationDataType", _configurationDataType);
            writer.WriteElementString("ChangedBy", _changedBy);
            writer.WriteElementString("ChangedDateTime", _changedDateTime.ToString(CultureInfo.InvariantCulture));
        }

    }
}
