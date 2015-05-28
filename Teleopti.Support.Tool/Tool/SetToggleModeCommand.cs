using System;
using System.Xml;
using Microsoft.Win32;

namespace Teleopti.Support.Tool.Tool
{
    public class SetToggleModeCommand : ISupportCommand
    {
        private readonly string toggleMode;
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
            var cxd = new System.Configuration.ConfigXmlDocument();
            cxd.Load(_webConfigUri);

            var appSettingsNode = cxd.DocumentElement.SelectSingleNode("//appSettings");
            var toggleModeElement = appSettingsNode.SelectSingleNode(string.Format("//add[@key='{0}']", "ToggleMode")) as XmlElement;
            if (toggleModeElement == null)
            {
                toggleModeElement = cxd.CreateElement("add");
                toggleModeElement.SetAttribute("key", "ToggleMode");
                appSettingsNode.AppendChild(toggleModeElement);
            }
            toggleModeElement.SetAttribute("value", toggleMode);
            cxd.Save(_webConfigUri);
            Console.Out.WriteLine("ToggleMode was set to " + toggleMode);
            Console.Out.WriteLine("Please restart your Teleopti WFM server by running ResetSystem.bat");
        }
    }
}
