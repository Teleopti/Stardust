using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.IO;


namespace Teleopti.Support.Tool.DataLayer
{
    public sealed class XmlHandler
    {
        private XmlHandler() { }


        /// <summary>
        /// Reads all nhib files in dir and creates a list of Nhib objects that contains information about all databases in this Nhib file
        /// </summary>
        /// <param name="dir">The directory to look in for all Nhib files</param>
        /// <returns>A IEnumerable lsit with Nhib objects</returns>
        public static IEnumerable<Nhib> GetNhibSettings(string dir)
        {
            IList<Nhib> nhibList = new List<Nhib>();
            XmlDocument xmlDocument = new XmlDocument();
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDocument.NameTable);
            manager.AddNamespace("Nhib", "urn:nhibernate-configuration-2.2");

            string[] filePaths = Directory.GetFiles(dir, "*.nhib.xml");
            //Loop throw all files in the file dir
            foreach (var file in filePaths)
            {
                xmlDocument.Load(file);
                string sessionfactory = xmlDocument.SelectSingleNode("//Nhib:session-factory", manager).Attributes["name"].Value;
                XmlNode xmlNode = xmlDocument.SelectSingleNode("//Nhib:property[@name='connection.connection_string']", manager);
                string cccConnection = xmlNode.InnerText;
                
                string analyticConnection = xmlDocument.SelectSingleNode("//connectionString").InnerText;
                Nhib nhib = new Nhib(cccConnection, analyticConnection, string.Empty, sessionfactory, file);
                nhibList.Add(nhib);
            }

            return nhibList;
        }




        /// <summary>
        /// This method will return the text for an Element, for example a connection string
        /// </summary>
        /// <param name="parentElement">The parent element name</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <param name="xmlDocument">The xmlDocument to find the Element in</param>
        /// <returns>The Element value</returns>
        private static string GetElementValue(string parentElement, string attributeName, XmlDocument xmlDocument)
        {
            string ret = null;
            XmlNodeList elemList = xmlDocument.GetElementsByTagName(parentElement);
            if (String.IsNullOrEmpty(attributeName))
            {
                return elemList[0].InnerText;
            }
            for (int i = 0; i < elemList.Count; i++)
            {
                if (elemList[i].Attributes[0].Value == attributeName)
                {
                    if (String.IsNullOrEmpty(elemList[i].InnerText))
                    {
                        return elemList[i].Attributes[1].Value;

                    }
                    return elemList[i].InnerText;
                }
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement">The name of the parent element</param>
        /// <param name="attributeName">The attribute name of the element to update</param>
        /// <param name="xmlDocument">The XMLDocument to update</param>
        /// <param name="newVal">the new Element value</param>
        /// <returns>The updated XMLDocument</returns>
        private static XmlDocument SetElementValue(string parentElement, string attributeName, XmlDocument xmlDocument, string newVal)
        {

            XmlNodeList elemList = xmlDocument.GetElementsByTagName(parentElement);
            if (String.IsNullOrEmpty(attributeName))
            {
                elemList[0].InnerText = newVal;
            }
            else
            {
                for (int i = 0; i < elemList.Count; i++)
                {
                    if (elemList[i].Attributes[0].Value == attributeName)
                    {
                        if (String.IsNullOrEmpty(elemList[i].InnerText))
                        {
                            elemList[i].Attributes[1].Value = newVal;
                        }
                        else
                        {
                            elemList[i].InnerText = newVal;
                        }

                    }
                }
            }
            return xmlDocument;
        }

     /// <summary>
     /// Updates A string with new values, is used when you want to update a connection string and set new parameters in the connection string
     /// </summary>
     /// <param name="li">A list with things that should be changed</param>
     /// <param name="str">The string that is to be updated</param>
     /// <param name="splitter">What we want to splt the string on</param>
     /// <param name="swapDB">The database name that we want to insert</param>
     /// <param name="newSqlUser">The SQL user that we want to insert</param>
     /// <param name="sqlUserPwd">The password that we want to insert</param>
     /// <param name="DBServer">The database server that we want to insert</param>
     /// <returns>Returns the updates string</returns>
        private static string UpdateString(Dictionary<string, string> li, string str, char[] splitter, string swapDB, string newSqlUser, string sqlUserPwd, string DBServer)
        {
            string ret = "";
            string[] temp = str.Split(splitter);
         for (int x = 0; x < temp.Length; x++)
            {
                bool hit = false;
                foreach (KeyValuePair<string, string> kvp in li)
                {
                    if (temp[x].ToUpper(System.Globalization.CultureInfo.InvariantCulture).Contains(kvp.Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture)))
                    {//Hit on the key that we will change
                        switch (kvp.Key)
                        {//What value should we put in
                            case "dataSourceKey":
                                ret += kvp.Value + DBServer;
                                hit = true;
                                break;
                            case "initialCatalogKey":
                                ret += kvp.Value + swapDB;
                                hit = true;
                                break;
                            case "userKey":
                                ret += kvp.Value + newSqlUser;
                                hit = true;
                                break;
                            case "passwordKey":
                                ret += kvp.Value + sqlUserPwd;
                                hit = true;
                                break;
                        }
                    }
                }//End foreach
                if (!hit)
                {
                    ret += temp[x];
                }
                if (x < temp.Length - 1)
                {
                    ret += ";";
                }
            }//End for
            return ret;
        }


        /// <summary>
        /// If the type is analyticsDB or messageBroker we should apply the analyticsDB
        /// If the type is appDB we should apply the appDB
        /// If the type is cube we should apply the analyticsDB
        /// If the type is aggDB???? Don't think this should be needed to be swapped in any connection string???
        /// </summary>
        /// <param name="xmlFileName"></param>
        /// <param name="confFilesRoot"></param>
        /// <param name="appDB"></param>
        /// <param name="analyticsDB"></param>
        /// <param name="newSqlUser"></param>
        /// <param name="sqlUserPassword"></param>
        /// <param name="dbServer"></param>
        /// <param name="fileKey"></param>
        public static XmlHandlerMessages UpdateAllConfigurationFiles(string xmlFileName, string confFilesRoot, string appDB, string analyticsDB, string newSqlUser, string sqlUserPassword, string dbServer, string fileKey)
        {
            var myXmlMessages = new XmlHandlerMessages();
            XmlDocument xmlDocument = null;
            string fileName = null;
            Dictionary<String, String> connSwapLi; // Contains a list of things that we wan't to swap
            Dictionary<String, String> connAttrLi; // Contains a List of all Attributes in the Element ConnString except them that is in Dic connSwapLi
            bool fileChanged = false; // Indicates whether the file has been updated or not
            try
            {
                using (var xmlReader = new XmlTextReader(xmlFileName))
                {
                    while (xmlReader.Read())
                    {
                        string connString;
                        switch (xmlReader.NodeType)
                        {
                            case XmlNodeType.EndElement:
                                if (xmlReader.Name == "File")
                                {
                                    //We Are at the end of one config file, let's save the file, but only if it has been updated
                                    if (xmlDocument != null)
                                    {
                                        if (fileChanged)
                                        {
                                            try
                                            {
                                                //If the file is ReadOnly, lets set it to Normal
                                                if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                                                    File.SetAttributes(fileName, FileAttributes.Normal);
                                                xmlDocument.Save(fileName);
                                                myXmlMessages.AddMessage(fileName);
                                            }
                                            catch (UnauthorizedAccessException ex)
                                            {
                                                myXmlMessages.AddError("Error in file: " + ex.Message);
                                            }
                                        }
                                    }
                                    xmlDocument = null;
                                    fileName = null;
                                }
                                else if (xmlReader.Name == "ConnString")
                                {
                                    connSwapLi = null;
                                    connAttrLi = null;
                                    connString = null;
                                }
                                break;
                            case XmlNodeType.Element: // The node is an element.
                                if (xmlReader.Name == "File")
                                {
                                    // We have a new file lets load the file into the xmlDocument
                                    xmlReader.MoveToAttribute(fileKey);
                                    //xmlReader.MoveToNextAttribute();
                                    fileName = confFilesRoot + xmlReader.Value;
                                    if (!String.IsNullOrEmpty(xmlReader.Value))
                                    {//No file name entered Move to the end of the element


                                        if (File.Exists(fileName))
                                        {
                                            xmlDocument = new XmlDocument();
                                            xmlDocument.Load(fileName);
                                            fileChanged = false;
                                        }
                                        else
                                        {
                                            myXmlMessages.AddError("File " + fileName + " doesn't exist");
                                            Console.WriteLine("File " + fileName + " doesn't exist");
                                        }
                                    }
                                }
                                else if (xmlReader.Name == "Schema" && xmlDocument != null)
                                {
                                    xmlReader.MoveToNextAttribute();
                                    xmlReader.MoveToNextAttribute();
                                    string elementName = xmlReader.Value;
                                    xmlReader.MoveToNextAttribute();
                                    string parentElement = xmlReader.Value;
                                    xmlDocument = SetElementValue(parentElement, elementName, xmlDocument, "dbo");
                                    fileChanged = true;
                                }
                                else if (xmlReader.Name == "ConnString" && xmlDocument != null)
                                {
                                    // We have a new ConnString lets load all the attributes, load them in to list of string string
                                    connSwapLi = new Dictionary<String, String>();
                                    connAttrLi = new Dictionary<String, String>();
                                    while (xmlReader.MoveToNextAttribute())
                                    {
                                        //Only add things that we want to update
                                        if (xmlReader.Name == "dataSourceKey" || xmlReader.Name == "initialCatalogKey" || xmlReader.Name == "userKey" || xmlReader.Name == "passwordKey")
                                        {
                                            connSwapLi.Add(xmlReader.Name, xmlReader.Value);
                                        }
                                        else
                                        {
                                            connAttrLi.Add(xmlReader.Name, xmlReader.Value);
                                        }
                                    }
                                    connString = GetElementValue(connAttrLi["parentElement"], connAttrLi["elementName"], xmlDocument);
                                    string swapDb; //The databse that we want in out conn string
                                    if (connAttrLi["type"] == "appDB")
                                    {
                                        swapDb = appDB;
                                    }
                                    else
                                    {
                                        swapDb = analyticsDB;
                                    }
                                    // We have the connection string. Lets do some swapping
                                    string newConnString = UpdateString(connSwapLi, connString, new[] { ';' }, swapDb, newSqlUser, sqlUserPassword, dbServer);
                                    // Now we have the updated connection string, lets insert it into the file again
                                    xmlDocument = SetElementValue(connAttrLi["parentElement"], connAttrLi["elementName"], xmlDocument, newConnString);
                                    fileChanged = true;
                                }
                                break;
                        }
                    }
                }
                
            }
            catch (NullReferenceException ex)// Could happen if the file we are reading dosen't have the setup that we have declared in config.xml (xmlFileName)
            {
                myXmlMessages.AddError(ex.Message+ex.InnerException);
            }
            return myXmlMessages;

        }
    }
}