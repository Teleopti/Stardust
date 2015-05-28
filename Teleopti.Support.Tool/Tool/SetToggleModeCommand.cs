using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace Teleopti.Support.Code.Tool
{
    public class SetToggleModeCommand : ISupportCommand
    {
        private string toggleMode;
        private string _installDir;
        private string _webConfigUri;
        private const string WebConfigSubUri = @"TeleoptiCCC\Web\web.config";

        public SetToggleModeCommand(string toggleMode)
        {
            this.toggleMode = toggleMode;
        }

        public void Execute(ModeFile mode)
        {
            _installDir = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings", "INSTALLDIR", @"");
            _webConfigUri = _installDir + WebConfigSubUri;
            System.Configuration.ConfigXmlDocument cxd = new System.Configuration.ConfigXmlDocument();
            cxd.Load(_webConfigUri);

            XmlNode appSettingsNode = cxd.DocumentElement.SelectSingleNode("//appSettings");
            XmlElement toggleModeElement = appSettingsNode.SelectSingleNode(string.Format("//add[@key='{0}']", "ToggleMode")) as XmlElement;
            if (toggleModeElement == null)
            {
                toggleModeElement = cxd.CreateElement("add");
                toggleModeElement.SetAttribute("key", "ToggleMode");
                appSettingsNode.AppendChild(toggleModeElement);
            }
            toggleModeElement.SetAttribute("value", this.toggleMode);
            cxd.Save(_webConfigUri);
            Console.Out.WriteLine("ToggleMode was set to " + this.toggleMode);
            Console.Out.WriteLine("Please restart your Teleopti WFM server by running ResetSystem.bat");
        }
    }
}
