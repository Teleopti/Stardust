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
    [NUnit.Framework.DescriptionAttribute("Requests")]
    public partial class RequestsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Requests.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Requests", "In order to review my requests made\r\nAs an agent\r\nI want to be able to view my re" +
                    "quests", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View request list")]
        public virtual void ViewRequestList()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View request list", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.And("I have an existing text request");
#line 9
 testRunner.When("I view requests");
#line 10
 testRunner.Then("I should see a requests list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See text request")]
        public virtual void SeeTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See text request", ((string[])(null)));
#line 12
this.ScenarioSetup(scenarioInfo);
#line 13
 testRunner.Given("I am an agent");
#line 14
 testRunner.And("I have an existing text request");
#line 15
 testRunner.When("I view requests");
#line 16
 testRunner.Then("I should see my existing text request");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See absence request")]
        public virtual void SeeAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See absence request", ((string[])(null)));
#line 18
this.ScenarioSetup(scenarioInfo);
#line 19
 testRunner.Given("I am an agent");
#line 20
 testRunner.And("I have an existing absence request");
#line 21
 testRunner.When("I view requests");
#line 22
 testRunner.Then("I should see my existing absence request");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See created shift trade request")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SeeCreatedShiftTradeRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See created shift trade request", new string[] {
                        "ignore"});
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("I am an agent");
#line 27
 testRunner.And("I have created a shift trade request");
#line 28
 testRunner.When("I view requests");
#line 29
 testRunner.Then("I should see my existing shift trade request");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See received shift trade request")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SeeReceivedShiftTradeRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See received shift trade request", new string[] {
                        "ignore"});
#line 32
this.ScenarioSetup(scenarioInfo);
#line 33
 testRunner.Given("I am an agent");
#line 34
 testRunner.And("I have received a shift trade request");
#line 35
 testRunner.When("I view requests");
#line 36
 testRunner.Then("I should see my existing shift trade request");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Requests tab")]
        public virtual void RequestsTab()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Requests tab", ((string[])(null)));
#line 38
this.ScenarioSetup(scenarioInfo);
#line 39
 testRunner.Given("I am an agent");
#line 40
 testRunner.When("I sign in");
#line 41
 testRunner.Then("I should be able to see requests link");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No access to requests tab")]
        public virtual void NoAccessToRequestsTab()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No access to requests tab", ((string[])(null)));
#line 43
this.ScenarioSetup(scenarioInfo);
#line 44
 testRunner.Given("I am an agent without access to any requests");
#line 45
 testRunner.When("I sign in");
#line 46
 testRunner.Then("I should not be able to see requests link");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No access to requests page")]
        public virtual void NoAccessToRequestsPage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No access to requests page", ((string[])(null)));
#line 48
this.ScenarioSetup(scenarioInfo);
#line 49
 testRunner.Given("I am an agent without access to any requests");
#line 50
 testRunner.And("I am signed in");
#line 51
 testRunner.When("I navigate to the requests page");
#line 52
 testRunner.Then("I should see an error message");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No requests")]
        public virtual void NoRequests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No requests", ((string[])(null)));
#line 54
this.ScenarioSetup(scenarioInfo);
#line 55
 testRunner.Given("I am an agent");
#line 56
 testRunner.And("I have no existing requests");
#line 57
 testRunner.When("I view requests");
#line 58
 testRunner.Then("I should see a user-friendly message explaining I dont have anything to view");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default sorting")]
        public virtual void DefaultSorting()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default sorting", ((string[])(null)));
#line 60
this.ScenarioSetup(scenarioInfo);
#line 61
 testRunner.Given("I am an agent");
#line 62
 testRunner.And("I have 2 existing request changed on different times");
#line 63
 testRunner.When("I view requests");
#line 64
 testRunner.Then("I should see that the list is sorted on changed date and time");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show single page")]
        public virtual void ShowSinglePage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show single page", ((string[])(null)));
#line 66
this.ScenarioSetup(scenarioInfo);
#line 67
 testRunner.Given("I am an agent");
#line 68
 testRunner.And("I have more than one page of requests");
#line 69
 testRunner.When("I view requests");
#line 70
 testRunner.Then("I should only see one page of requests");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Paging")]
        public virtual void Paging()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Paging", ((string[])(null)));
#line 72
this.ScenarioSetup(scenarioInfo);
#line 73
 testRunner.Given("I am an agent");
#line 74
 testRunner.And("I have more than one page of requests");
#line 75
 testRunner.When("I view requests");
#line 76
 testRunner.And("I scroll down to the bottom of the page");
#line 77
 testRunner.Then("I should see the page fill with the next page of requests");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
