using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Teleopti.Support.Library.Config
{
    public interface IMachineKeyChecker
    {
        bool CheckForMachineKey(string filePath);
    }

    public class MachineKeyChecker : IMachineKeyChecker
    {
        public bool CheckForMachineKey(string filePath)
        {
            if (!filePath.ToLower(CultureInfo.InvariantCulture).EndsWith("web.config"))
                return false;
            if (filePath.Contains(@"\Areas\"))
                return false;
            XDocument xDocument = XDocument.Load(filePath);
            var xElement = xDocument.Descendants("machineKey").FirstOrDefault();
            if (xElement == null)
            {
                var xElement2 = xDocument.Descendants("system.web").FirstOrDefault();
                if (xElement2 == null)
                {
                    //Console.WriteLine("No system.web element found. Probably not a web.config file.");
                    return false ;
                }
                xElement = new XElement("machineKey");
                xElement2.Add(xElement);
            }
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "validation.key");
            string text2 = File.Exists(path) ? File.ReadAllText(path) : CryptoCreator.GetCryptoBytes(64);
            File.WriteAllText(path, text2);
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "decryption.key");
            string text3 = File.Exists(path) ? File.ReadAllText(path) : CryptoCreator.GetCryptoBytes(24);
            File.WriteAllText(path, text3);
            xElement.SetAttributeValue("validationKey", text2);
            xElement.SetAttributeValue("decryptionKey", text3);
            xElement.SetAttributeValue("decryption", "AES");
            xElement.SetAttributeValue("validation", "SHA1");
            xDocument.Save(filePath);
           
            return true;
        }
    }
}