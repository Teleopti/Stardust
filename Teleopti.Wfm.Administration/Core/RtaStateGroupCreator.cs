using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Administration.Core
{
    public class RtaStateGroupCreator
    {
        private const string DEFAULT_STATE_GROUP_SECTION = "DefaultStateGroup";
        private const string STATE = "state";
        private const string NAME = "name";
        private const string RTA_STATE = "RtaState";
        
        private readonly string _defaultStateGroup;
        private readonly XmlDocument _xmlDocument;
        private readonly IList<IRtaStateGroup> _rtaGroupCollection = new List<IRtaStateGroup>();

        public RtaStateGroupCreator(string rtaSettings)
        {
            _xmlDocument = new XmlDocument();
	        _xmlDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\App_Data\\{rtaSettings}");
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

				foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    if (node.Name.Equals(STATE))
                    {
                        string stateCode = node.InnerText.Trim();
                        string stateName = node.Attributes[NAME].Value.Trim();
                        rtaStateGroup.AddState(stateCode, stateName);
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
                                                   
			foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.Name.Equals(STATE))
                {
                    string stateCode = node.InnerText.Trim();
                    string stateName = node.Attributes[NAME].Value.Trim();
	                rtaStateGroup.AddState(stateCode, stateName);
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
