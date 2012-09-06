﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.17929
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Teleopti.Ccc.WebBehaviorTest
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Preferences Extended")]
    public partial class PreferencesExtendedFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "PreferencesExtended.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Preferences Extended", "In order to view and submit when I prefer to work in more detail\r\nAs an agent\r\nI " +
                    "want to view and submit extended preferences", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 8
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table1.AddRow(new string[] {
                        "Name",
                        "Access to extended preferences"});
            table1.AddRow(new string[] {
                        "Access to extended preferences",
                        "true"});
#line 9
 testRunner.Given("there is a role with", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "No access to extended preferences"});
            table2.AddRow(new string[] {
                        "Access to extended preferences",
                        "false"});
#line 13
 testRunner.And("there is a role with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Name",
                        "Late"});
#line 17
 testRunner.And("there is a shift category with", ((string)(null)), table3);
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Name",
                        "Lunch"});
#line 20
 testRunner.And("there is an activity with", ((string)(null)), table4);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Name",
                        "Dayoff"});
#line 23
 testRunner.And("there is a dayoff with", ((string)(null)), table5);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Name",
                        "Illness"});
#line 26
 testRunner.And("there is an absence with", ((string)(null)), table6);
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table7.AddRow(new string[] {
                        "Schedule published to date",
                        "2012-06-24"});
            table7.AddRow(new string[] {
                        "Available shift category",
                        "Late"});
            table7.AddRow(new string[] {
                        "Available dayoff",
                        "Dayoff"});
            table7.AddRow(new string[] {
                        "Available absence",
                        "Illness"});
            table7.AddRow(new string[] {
                        "Available activity",
                        "Lunch"});
#line 29
 testRunner.And("I have a workflow control set with", ((string)(null)), table7);
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table8.AddRow(new string[] {
                        "Type",
                        "Week"});
            table8.AddRow(new string[] {
                        "Length",
                        "1"});
#line 37
 testRunner.And("I have a schedule period with", ((string)(null)), table8);
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
#line 42
 testRunner.And("I have a person period with", ((string)(null)), table9);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can see indication of an extended preference")]
        public virtual void CanSeeIndicationOfAnExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can see indication of an extended preference", ((string[])(null)));
#line 46
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 47
 testRunner.Given("I have the role \'Access to extended preferences\'");
#line 48
 testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 49
 testRunner.When("I view preferences for date \'2012-06-20\'");
#line 50
 testRunner.Then("I should see that I have an extended preference on \'2012-06-20\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can see extended preference")]
        public virtual void CanSeeExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can see extended preference", ((string[])(null)));
#line 52
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 53
 testRunner.Given("I have the role \'Access to extended preferences\'");
#line 54
 testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 55
 testRunner.When("I view preferences for date \'2012-06-20\'");
#line 56
 testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line 57
 testRunner.Then("I should see the extended preference on \'2012-06-20\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can see extended preference without permission")]
        public virtual void CanSeeExtendedPreferenceWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can see extended preference without permission", ((string[])(null)));
#line 59
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 60
 testRunner.Given("I have the role \'No access to extended preferences\'");
#line 61
 testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 62
 testRunner.When("I view preferences for date \'2012-06-20\'");
#line 63
 testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line 64
 testRunner.Then("I should see the extended preference on \'2012-06-20\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cannot see extended preference button without permission")]
        public virtual void CannotSeeExtendedPreferenceButtonWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cannot see extended preference button without permission", ((string[])(null)));
#line 74
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 75
 testRunner.Given("I have the role \'No access to extended preferences\'");
#line 76
 testRunner.When("I am viewing preferences");
#line 77
 testRunner.Then("I should not see the extended preference button");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add standard preference")]
        public virtual void AddStandardPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add standard preference", ((string[])(null)));
#line 79
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 80
 testRunner.Given("I have the role \'Access to extended preferences\'");
#line 81
 testRunner.And("I am viewing preferences for date \'2012-06-20\'");
#line 82
 testRunner.When("I select day \'2012-06-20\'");
#line 83
 testRunner.And("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "Preference",
                        "Late"});
#line 84
 testRunner.And("I input extended preference fields with", ((string)(null)), table10);
#line 87
 testRunner.And("I click the apply extended preferences button");
#line 88
 testRunner.Then("I should not see an extended preference indication on \'2012-06-20\'");
#line 89
 testRunner.And("I should see the preference \'Late\' on \'2012-06-20\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add extended preference")]
        public virtual void AddExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add extended preference", ((string[])(null)));
#line 91
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 92
 testRunner.Given("I have the role \'Access to extended preferences\'");
#line 93
 testRunner.And("I am viewing preferences for date \'2012-06-20\'");
#line 94
 testRunner.When("I select day \'2012-06-20\'");
#line 95
 testRunner.And("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Preference",
                        "Late"});
            table11.AddRow(new string[] {
                        "Start time minimum",
                        "10:30"});
            table11.AddRow(new string[] {
                        "Start time maximum",
                        "11:00"});
            table11.AddRow(new string[] {
                        "End time minimum",
                        "19:00"});
            table11.AddRow(new string[] {
                        "End time maximum",
                        "20:30"});
            table11.AddRow(new string[] {
                        "Work time minimum",
                        "08:00"});
            table11.AddRow(new string[] {
                        "Work time maximum",
                        "08:30"});
            table11.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
            table11.AddRow(new string[] {
                        "Activity Start time minimum",
                        "11:30"});
            table11.AddRow(new string[] {
                        "Activity Start time maximum",
                        "11:45"});
            table11.AddRow(new string[] {
                        "Activity End time minimum",
                        "12:00"});
            table11.AddRow(new string[] {
                        "Activity End time maximum",
                        "12:15"});
            table11.AddRow(new string[] {
                        "Activity time minimum",
                        "00:15"});
            table11.AddRow(new string[] {
                        "Activity time maximum",
                        "00:45"});
#line 96
 testRunner.And("I input extended preference fields with", ((string)(null)), table11);
#line 112
 testRunner.And("I click the apply extended preferences button");
#line 113
 testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "Date",
                        "2012-06-20"});
            table12.AddRow(new string[] {
                        "Preference",
                        "Late"});
            table12.AddRow(new string[] {
                        "Start time minimum",
                        "10:30"});
            table12.AddRow(new string[] {
                        "Start time maximum",
                        "11:00"});
            table12.AddRow(new string[] {
                        "End time minimum",
                        "19:00"});
            table12.AddRow(new string[] {
                        "End time maximum",
                        "20:30"});
            table12.AddRow(new string[] {
                        "Work time minimum",
                        "8:00"});
            table12.AddRow(new string[] {
                        "Work time maximum",
                        "8:30"});
            table12.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
            table12.AddRow(new string[] {
                        "Activity Start time minimum",
                        "11:30"});
            table12.AddRow(new string[] {
                        "Activity Start time maximum",
                        "11:45"});
            table12.AddRow(new string[] {
                        "Activity End time minimum",
                        "12:00"});
            table12.AddRow(new string[] {
                        "Activity End time maximum",
                        "12:15"});
            table12.AddRow(new string[] {
                        "Activity time minimum",
                        "0:15"});
            table12.AddRow(new string[] {
                        "Activity time maximum",
                        "0:45"});
#line 114
 testRunner.Then("I should see extended preference with", ((string)(null)), table12);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Preference list contains available preferences when adding extended preference")]
        public virtual void PreferenceListContainsAvailablePreferencesWhenAddingExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Preference list contains available preferences when adding extended preference", ((string[])(null)));
#line 132
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 133
 testRunner.Given("I have the role \'Access to extended preferences\'");
#line 134
 testRunner.And("I am viewing preferences");
#line 135
 testRunner.When("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Value"});
            table13.AddRow(new string[] {
                        ""});
            table13.AddRow(new string[] {
                        "-"});
            table13.AddRow(new string[] {
                        "Late"});
            table13.AddRow(new string[] {
                        "-"});
            table13.AddRow(new string[] {
                        "Dayoff"});
            table13.AddRow(new string[] {
                        "-"});
            table13.AddRow(new string[] {
                        "Illness"});
#line 136
 testRunner.Then("I should see these available preferences", ((string)(null)), table13);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Activity list contains available activities when adding extended preference")]
        public virtual void ActivityListContainsAvailableActivitiesWhenAddingExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Activity list contains available activities when adding extended preference", ((string[])(null)));
#line 146
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 147
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 148
    testRunner.And("I am viewing preferences");
#line 149
    testRunner.When("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Value"});
            table14.AddRow(new string[] {
                        ""});
            table14.AddRow(new string[] {
                        "-"});
            table14.AddRow(new string[] {
                        "Lunch"});
#line 150
    testRunner.Then("I should see these available activities", ((string)(null)), table14);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Replace extended preference")]
        public virtual void ReplaceExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Replace extended preference", ((string[])(null)));
#line 156
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 157
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table15.AddRow(new string[] {
                        "Date",
                        "2012-09-05"});
            table15.AddRow(new string[] {
                        "IsExtended",
                        "true"});
            table15.AddRow(new string[] {
                        "Shift Category",
                        "Late"});
#line 158
    testRunner.And("I have an extended preference with", ((string)(null)), table15);
#line 163
    testRunner.And("I am viewing preferences for date \'2012-09-05\'");
#line 164
    testRunner.When("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table16.AddRow(new string[] {
                        "Preference",
                        "Late"});
            table16.AddRow(new string[] {
                        "Start time minimum",
                        "10:30"});
            table16.AddRow(new string[] {
                        "Start time maximum",
                        "11:00"});
            table16.AddRow(new string[] {
                        "End time minimum",
                        "19:00"});
            table16.AddRow(new string[] {
                        "End time maximum",
                        "20:30"});
            table16.AddRow(new string[] {
                        "Work time minimum",
                        "08:00"});
            table16.AddRow(new string[] {
                        "Work time maximum",
                        "08:30"});
            table16.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
            table16.AddRow(new string[] {
                        "Activity Start time minimum",
                        "12:00"});
            table16.AddRow(new string[] {
                        "Activity Start time maximum",
                        "12:15"});
            table16.AddRow(new string[] {
                        "Activity End time minimum",
                        "12:30"});
            table16.AddRow(new string[] {
                        "Activity End time maximum",
                        "12:45"});
            table16.AddRow(new string[] {
                        "Activity time minimum",
                        "00:15"});
            table16.AddRow(new string[] {
                        "Activity time maximum",
                        "00:45"});
#line 165
    testRunner.And("I input extended preference fields with", ((string)(null)), table16);
#line 181
    testRunner.And("I click the apply extended preferences button");
#line 182
    testRunner.And("I click the extended preference indication on \'2012-09-05\'");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table17.AddRow(new string[] {
                        "Date",
                        "2012-09-05"});
            table17.AddRow(new string[] {
                        "Preference",
                        "Late"});
            table17.AddRow(new string[] {
                        "Start time minimum",
                        "10:30"});
            table17.AddRow(new string[] {
                        "Start time maximum",
                        "11:00"});
            table17.AddRow(new string[] {
                        "End time minimum",
                        "19:30"});
            table17.AddRow(new string[] {
                        "End time maximum",
                        "20:00"});
            table17.AddRow(new string[] {
                        "Work time minimum",
                        "08:00"});
            table17.AddRow(new string[] {
                        "Work time maximum",
                        "08:30"});
            table17.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
            table17.AddRow(new string[] {
                        "Activity Start time minimum",
                        "12:00"});
            table17.AddRow(new string[] {
                        "Activity Start time maximum",
                        "12:15"});
            table17.AddRow(new string[] {
                        "Activity End time minimum",
                        "12:30"});
            table17.AddRow(new string[] {
                        "Activity End time maximum",
                        "12:45"});
            table17.AddRow(new string[] {
                        "Activity time minimum",
                        "00:15"});
            table17.AddRow(new string[] {
                        "Activity time maximum",
                        "00:45"});
#line 183
    testRunner.Then("I should see extended preference with", ((string)(null)), table17);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cannot edit extended preference without permission")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void CannotEditExtendedPreferenceWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cannot edit extended preference without permission", new string[] {
                        "ignore"});
#line 203
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 204
    testRunner.Given("I have the role \'No access to extended preferences\'");
#line 205
    testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 206
    testRunner.When("I view preferences for date \'2012-06-20\'");
#line 207
    testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line 208
    testRunner.Then("I should not be able to edit extended preference on \'2012-06-20\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can only select available preferences when editing an existing extended preferenc" +
            "e")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void CanOnlySelectAvailablePreferencesWhenEditingAnExistingExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can only select available preferences when editing an existing extended preferenc" +
                    "e", new string[] {
                        "ignore"});
#line 211
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 212
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 213
    testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 214
    testRunner.When("I view preferences for date \'2012-06-20\'");
#line 215
    testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "Value"});
            table18.AddRow(new string[] {
                        "Late"});
            table18.AddRow(new string[] {
                        "Dayoff"});
            table18.AddRow(new string[] {
                        "Illness"});
#line 216
    testRunner.Then("I should see these available preferences", ((string)(null)), table18);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can only select available activities when editing an existing extended preference" +
            "")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void CanOnlySelectAvailableActivitiesWhenEditingAnExistingExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can only select available activities when editing an existing extended preference" +
                    "", new string[] {
                        "ignore"});
#line 222
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 223
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 224
    testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 225
    testRunner.When("I view preferences for date \'2012-06-20\'");
#line 226
    testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table19.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
#line 227
    testRunner.Then("I should see these available activities", ((string)(null)), table19);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Verify time validation for preference start and end time")]
        public virtual void VerifyTimeValidationForPreferenceStartAndEndTime()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Verify time validation for preference start and end time", ((string[])(null)));
#line 234
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 235
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 236
    testRunner.And("I am viewing preferences for date \'2012-06-20\'");
#line 237
    testRunner.When("I select day \'2012-06-20\'");
#line 238
    testRunner.And("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table20.AddRow(new string[] {
                        "Preference",
                        "Late"});
            table20.AddRow(new string[] {
                        "Start time minimum",
                        "10:30"});
            table20.AddRow(new string[] {
                        "Start time maximum",
                        "10:00"});
#line 239
    testRunner.And("I input extended preference fields with", ((string)(null)), table20);
#line 244
    testRunner.And("I click the apply extended preferences button");
#line 245
    testRunner.Then("I should see add extended preferences panel with error \'Invalid time startTime\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Disable all time fields when absence preference is selected")]
        public virtual void DisableAllTimeFieldsWhenAbsencePreferenceIsSelected()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Disable all time fields when absence preference is selected", ((string[])(null)));
#line 247
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 248
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 249
    testRunner.And("I am viewing preferences");
#line 250
    testRunner.When("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table21.AddRow(new string[] {
                        "Preference",
                        "Illness"});
#line 251
    testRunner.And("I input extended preference fields with", ((string)(null)), table21);
#line 254
    testRunner.Then("I should not be able to edit time fields");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Disable all time fields, when day off is selected")]
        public virtual void DisableAllTimeFieldsWhenDayOffIsSelected()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Disable all time fields, when day off is selected", ((string[])(null)));
#line 256
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 257
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 258
    testRunner.And("I am viewing preferences");
#line 259
    testRunner.When("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table22 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table22.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
#line 260
    testRunner.And("I input extended preference fields with", ((string)(null)), table22);
#line hidden
            TechTalk.SpecFlow.Table table23 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table23.AddRow(new string[] {
                        "Preference",
                        "Dayoff"});
#line 263
    testRunner.And("I input extended preference fields with", ((string)(null)), table23);
#line 266
    testRunner.Then("I should not be able to edit activity time fields");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Reset activity field when day off is selected")]
        public virtual void ResetActivityFieldWhenDayOffIsSelected()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Reset activity field when day off is selected", ((string[])(null)));
#line 268
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 269
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 270
    testRunner.And("I am viewing preferences");
#line 271
    testRunner.When("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table24 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table24.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
#line 272
    testRunner.And("I input extended preference fields with", ((string)(null)), table24);
#line hidden
            TechTalk.SpecFlow.Table table25 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table25.AddRow(new string[] {
                        "Preference",
                        "Dayoff"});
#line 275
    testRunner.And("I input extended preference fields with", ((string)(null)), table25);
#line 278
    testRunner.Then("I should see activity dropdown list selected to \"none\"");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Reset activity field when absence is selected")]
        public virtual void ResetActivityFieldWhenAbsenceIsSelected()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Reset activity field when absence is selected", ((string[])(null)));
#line 280
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 281
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 282
    testRunner.And("I am viewing preferences");
#line hidden
            TechTalk.SpecFlow.Table table26 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table26.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
#line 283
    testRunner.And("I input extended preference fields with", ((string)(null)), table26);
#line 286
    testRunner.When("I click the extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table27 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table27.AddRow(new string[] {
                        "Preference",
                        "Illness"});
#line 287
    testRunner.And("I input extended preference fields with", ((string)(null)), table27);
#line 290
    testRunner.And("I should see activity dropdown list selected to \"none\"");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Enable activity time fields when activity is selected")]
        public virtual void EnableActivityTimeFieldsWhenActivityIsSelected()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Enable activity time fields when activity is selected", ((string[])(null)));
#line 292
this.ScenarioSetup(scenarioInfo);
#line 8
this.FeatureBackground();
#line 293
    testRunner.Given("I have the role \'Access to extended preferences\'");
#line 294
    testRunner.And("I am viewing preferences");
#line 295
    testRunner.When("I click the add extended preference button");
#line hidden
            TechTalk.SpecFlow.Table table28 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table28.AddRow(new string[] {
                        "Activity",
                        "Lunch"});
#line 296
    testRunner.And("I input extended preference fields with", ((string)(null)), table28);
#line 299
    testRunner.Then("I should be able to edit activity minimum and maximum fields");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
