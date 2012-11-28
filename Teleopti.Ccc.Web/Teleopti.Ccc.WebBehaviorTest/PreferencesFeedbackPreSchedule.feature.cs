﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.296
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Preferences feedback pre scheduled")]
    public partial class PreferencesFeedbackPreScheduledFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "PreferencesFeedbackPreSchedule.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Preferences feedback pre scheduled", "In order to clearly see preferences that collide with the pre scheduled personal " +
                    "shift or meeting.\r\nAs an agent\r\nI want good feedback about personal shifts, meet" +
                    "ings and the the preferences in collision", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 6
#line 7
 testRunner.Given("I have a role named \'Agent\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table1.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table1.AddRow(new string[] {
                        "Schedule published to date",
                        "2012-10-14"});
#line 8
 testRunner.And("I have a workflow control set with", ((string)(null)), table1, "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Start date",
                        "2012-10-01"});
            table2.AddRow(new string[] {
                        "Type",
                        "Week"});
            table2.AddRow(new string[] {
                        "Length",
                        "1"});
#line 12
 testRunner.And("I have a schedule period with", ((string)(null)), table2, "And ");
#line 17
 testRunner.And("there is a shift category named \'Day\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Name",
                        "Phone"});
            table3.AddRow(new string[] {
                        "AllowMeeting",
                        "true"});
#line 18
 testRunner.And("there is an activity with", ((string)(null)), table3, "And ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Name",
                        "Administration"});
            table4.AddRow(new string[] {
                        "AllowMeeting",
                        "true"});
#line 22
 testRunner.And("there is an activity with", ((string)(null)), table4, "And ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Name",
                        "Common"});
            table5.AddRow(new string[] {
                        "Activity",
                        "Phone"});
            table5.AddRow(new string[] {
                        "Shift category",
                        "Day"});
            table5.AddRow(new string[] {
                        "Start boundry",
                        "8:00-9:00"});
            table5.AddRow(new string[] {
                        "End boundry",
                        "17:00-18:00"});
#line 26
 testRunner.And("there is a rule set with", ((string)(null)), table5, "And ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Name",
                        "Common"});
            table6.AddRow(new string[] {
                        "Sets",
                        "Common"});
#line 33
 testRunner.And("there is a rule set bag with", ((string)(null)), table6, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See indication of a pre-scheduled meeting")]
        public virtual void SeeIndicationOfAPre_ScheduledMeeting()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See indication of a pre-scheduled meeting", ((string[])(null)));
#line 39
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 40
 testRunner.Given("I have a person period that starts on \'2012-10-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "StartTime",
                        "2012-10-19 9:00"});
            table7.AddRow(new string[] {
                        "EndTime",
                        "2012-10-19 10:00"});
            table7.AddRow(new string[] {
                        "Subject",
                        "Meeting subject"});
            table7.AddRow(new string[] {
                        "Location",
                        "Meeting location"});
#line 41
 testRunner.And("I have a pre-scheduled meeting with", ((string)(null)), table7, "And ");
#line 47
 testRunner.When("I view preferences for date \'2012-10-19\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 48
 testRunner.Then("I should see that I have a pre-scheduled meeting on \'2012-10-19\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Tooltip of a pre-scheduled meeting")]
        public virtual void TooltipOfAPre_ScheduledMeeting()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Tooltip of a pre-scheduled meeting", ((string[])(null)));
#line 50
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 51
 testRunner.Given("I have a person period that starts on \'2012-10-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "StartTime",
                        "2012-10-19 9:00"});
            table8.AddRow(new string[] {
                        "EndTime",
                        "2012-10-19 10:00"});
            table8.AddRow(new string[] {
                        "Subject",
                        "Meeting subject"});
            table8.AddRow(new string[] {
                        "Location",
                        "Meeting location"});
#line 52
 testRunner.And("I have a pre-scheduled meeting with", ((string)(null)), table8, "And ");
#line 58
 testRunner.When("I view preferences for date \'2012-10-19\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "StartTime",
                        "2012-10-19 9:00"});
            table9.AddRow(new string[] {
                        "EndTime",
                        "2012-10-19 10:00"});
            table9.AddRow(new string[] {
                        "Subject",
                        "Meeting subject"});
#line 59
 testRunner.Then("I should have a tooltip for meeting details with", ((string)(null)), table9, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See indication of a pre-scheduled personal shift")]
        public virtual void SeeIndicationOfAPre_ScheduledPersonalShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See indication of a pre-scheduled personal shift", ((string[])(null)));
#line 65
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 66
 testRunner.Given("I have a person period that starts on \'2012-10-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "StartTime",
                        "2012-10-19 9:00"});
            table10.AddRow(new string[] {
                        "EndTime",
                        "2012-10-19 10:00"});
            table10.AddRow(new string[] {
                        "Activity",
                        "Administration"});
#line 67
 testRunner.And("I have a pre-scheduled personal shift with", ((string)(null)), table10, "And ");
#line 72
 testRunner.When("I view preferences for date \'2012-10-19\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 73
 testRunner.Then("I should see that I have a pre-scheduled personal shift on \'2012-10-19\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Tooltip of a pre-scheduled personal shift")]
        public virtual void TooltipOfAPre_ScheduledPersonalShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Tooltip of a pre-scheduled personal shift", ((string[])(null)));
#line 75
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 76
 testRunner.Given("I have a person period that starts on \'2012-10-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "StartTime",
                        "2012-10-19 9:00"});
            table11.AddRow(new string[] {
                        "EndTime",
                        "2012-10-19 10:00"});
            table11.AddRow(new string[] {
                        "Activity",
                        "Administration"});
#line 77
 testRunner.And("I have a pre-scheduled personal shift with", ((string)(null)), table11, "And ");
#line 82
 testRunner.When("I view preferences for date \'2012-10-19\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "StartTime",
                        "2012-10-19 9:00"});
            table12.AddRow(new string[] {
                        "EndTime",
                        "2012-10-19 10:00"});
            table12.AddRow(new string[] {
                        "Activity",
                        "Administration"});
#line 83
 testRunner.Then("I should have a tooltip for personal shift details with", ((string)(null)), table12, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
