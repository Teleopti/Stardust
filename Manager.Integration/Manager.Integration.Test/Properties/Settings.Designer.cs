﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Manager.Integration.Test.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("../../../Manager.Integration.Tests.Console.Host/bin/")]
        public string ManagerIntegrationConsoleHostLocation {
            get {
                return ((string)(this["ManagerIntegrationConsoleHostLocation"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Manager.IntegrationTest.Console.Host.exe")]
        public string ManagerIntegrationConsoleHostAssemblyName {
            get {
                return ((string)(this["ManagerIntegrationConsoleHostAssemblyName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Manager.config")]
        public string ManagerConfigurationFileName {
            get {
                return ((string)(this["ManagerConfigurationFileName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Manager.IntegrationTest.Console.Host.exe.config")]
        public string ManagerIntegrationConsoleHostConfigurationFile {
            get {
                return ((string)(this["ManagerIntegrationConsoleHostConfigurationFile"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("scripts/CreateLoggingTable.sql")]
        public string CreateLoggingTableSqlScriptLocationAndFileName {
            get {
                return ((string)(this["CreateLoggingTableSqlScriptLocationAndFileName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost:9100/")]
        public string ManagerIntegrationTestControllerBaseAddress {
            get {
                return ((string)(this["ManagerIntegrationTestControllerBaseAddress"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost:9000/StardustDashboard/")]
        public string ManagerLocationUri {
            get {
                return ((string)(this["ManagerLocationUri"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost:9100/")]
        public string IntegrationControllerBaseAddress {
            get {
                return ((string)(this["IntegrationControllerBaseAddress"]));
            }
        }
    }
}
