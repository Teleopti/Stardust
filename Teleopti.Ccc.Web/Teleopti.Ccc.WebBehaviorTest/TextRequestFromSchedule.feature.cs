// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.6.1.0
//      SpecFlow Generator Version:1.6.0.0
//      Runtime Version:4.0.30319.239
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
namespace Teleopti.Ccc.WebBehaviorTest
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.6.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Text request from schedule")]
    public partial class TextRequestFromScheduleFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "TextRequestFromSchedule.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Text request from schedule", "In order to make requests to my superior\r\nAs an agent\r\nI want to be able to submi" +
                    "t requests as text", GenerationTargetLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add text request from week schedule view")]
        public virtual void AddTextRequestFromWeekScheduleView()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add text request from week schedule view", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.And("I am viewing week schedule");
#line 9
 testRunner.When("I click add text request button in the toolbar");
#line 10
 testRunner.And("I input text request values");
#line 11
 testRunner.And("I click the OK button");
#line 12
 testRunner.Then("I should see a symbol at the top of the schedule");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not add text request if no permission")]
        public virtual void CanNotAddTextRequestIfNoPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not add text request if no permission", ((string[])(null)));
#line 15
this.ScenarioSetup(scenarioInfo);
#line 16
 testRunner.Given("I am an agent without access to text requests");
#line 17
 testRunner.When("I am viewing week schedule");
#line 18
 testRunner.Then("I should not see the add text request button");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default text request values from week schedule")]
        public virtual void DefaultTextRequestValuesFromWeekSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default text request values from week schedule", ((string[])(null)));
#line 20
this.ScenarioSetup(scenarioInfo);
#line 21
 testRunner.Given("I am an agent");
#line 22
 testRunner.And("I am viewing week schedule");
#line 23
 testRunner.When("I click add text request button in the toolbar");
#line 24
 testRunner.Then("I should see the text request form with the first day of week as default");
#line 25
 testRunner.And("I should see 8:00 - 17:00 as the default times");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cancel adding text request")]
        public virtual void CancelAddingTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cancel adding text request", ((string[])(null)));
#line 27
this.ScenarioSetup(scenarioInfo);
#line 28
 testRunner.Given("I am an agent");
#line 29
 testRunner.And("I am viewing week schedule");
#line 30
 testRunner.When("I click add text request button in the toolbar");
#line 31
 testRunner.And("I input text request values");
#line 32
 testRunner.And("I click the Cancel button");
#line 33
 testRunner.Then("I should not see a symbol at the top of the schedule");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding invalid text request values")]
        public virtual void AddingInvalidTextRequestValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding invalid text request values", ((string[])(null)));
#line 35
this.ScenarioSetup(scenarioInfo);
#line 36
 testRunner.Given("I am an agent");
#line 37
 testRunner.And("I am viewing week schedule");
#line 38
 testRunner.When("I click add text request button in the toolbar");
#line 39
 testRunner.And("I input empty subject");
#line 40
 testRunner.And("I input later start time than end time");
#line 41
 testRunner.And("I click the OK button");
#line 42
 testRunner.Then("I should see texts describing my errors");
#line 43
 testRunner.And("I should not see a symbol at the top of the schedule");
#line hidden
            testRunner.CollectScenarioErrors();
        }
    }
}
#endregion
