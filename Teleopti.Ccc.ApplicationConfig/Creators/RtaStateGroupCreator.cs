using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class RtaStateGroupCreator
    {
        private const string DEFAULT_STATE_GROUP_SECTION = "DefaultStateGroup";
        private const string PLATFORM_TYPE_ID = "platformTypeId";
        private const string STATE = "state";
        private const string NAME = "name";
        private const string RTA_STATE = "RtaState";
        
        private readonly string _defaultStateGroup;
        private readonly XmlDocument _xmlDocument;
        private readonly IList<IRtaStateGroup> _rtaGroupCollection = new List<IRtaStateGroup>();

        public RtaStateGroupCreator(string rtaSettings)
        {
            _xmlDocument = new XmlDocument();
            _xmlDocument.Load(rtaSettings);
            XmlElement root = _xmlDocument.DocumentElement;
            XmlNode xmlNode = root.SelectSingleNode(DEFAULT_STATE_GROUP_SECTION);
            _defaultStateGroup = xmlNode.InnerText.Trim(); 
            //("/df:Report/df:DataSources",

            createDefaultStateGroup();
            createStateGroups();
        }

        private void createStateGroups()
        {
            XmlElement root = _xmlDocument.DocumentElement;
            string xpath = string.Format(CultureInfo.CurrentCulture, "{0}[@{1}!='{2}']", RTA_STATE, NAME, _defaultStateGroup);
            XmlNodeList xmlNodeList = root.SelectNodes(xpath);

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                XmlAttribute xmlAttribute = xmlNode.Attributes[NAME];
                string stateGroupName = xmlAttribute.InnerText.Trim();
                IRtaStateGroup rtaStateGroup = new RtaStateGroup(stateGroupName, false, true);
                xmlAttribute = xmlNode.Attributes[PLATFORM_TYPE_ID];
                string platformTypeId = xmlAttribute.InnerText.Trim();

                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    if (node.Name.Equals(STATE))
                    {
                        string stateCode = node.InnerText.Trim();
                        string stateName = node.Attributes[NAME].Value.Trim();
                        rtaStateGroup.AddState(stateName, stateCode, new Guid(platformTypeId));
                    }
                }
                _rtaGroupCollection.Add(rtaStateGroup);
            }
        }

        private void createDefaultStateGroup()
        {
            IRtaStateGroup rtaStateGroup = new RtaStateGroup(_defaultStateGroup, true, true);
            XmlElement root = _xmlDocument.DocumentElement;
            string xpath = string.Format(CultureInfo.CurrentCulture, "{0}[@{1}='{2}']", RTA_STATE, NAME, _defaultStateGroup);
            XmlNode xmlNode = root.SelectSingleNode(xpath);
                                                   
            XmlAttribute xmlAttribute = xmlNode.Attributes[PLATFORM_TYPE_ID];
            string platformTypeId = xmlAttribute.InnerText.Trim();

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.Name.Equals(STATE))
                {
                    string stateCode = node.InnerText.Trim();
                    string stateName = node.Attributes[NAME].Value.Trim();
                    rtaStateGroup.AddState(stateName, stateCode, new Guid(platformTypeId));
                }
            }
            _rtaGroupCollection.Add(rtaStateGroup);
        }

        public IList<IRtaStateGroup> RtaGroupCollection
        {
            get { return _rtaGroupCollection; }
        }
    }
}
