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
    [NUnit.Framework.DescriptionAttribute("Student availability")]
    public partial class StudentAvailabilityFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "StudentAvailability.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Student availability", "In order to view and submit when I am available for work\r\nAs a student agent\r\nI w" +
                    "ant to view and submit my availability", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("View student availability")]
        public virtual void ViewStudentAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View student availability", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am a student agent");
#line 8
 testRunner.When("I view student availability");
#line 9
 testRunner.Then("I should see current or first future virtual schedule period +/- 1 week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See student availability")]
        public virtual void SeeStudentAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See student availability", ((string[])(null)));
#line 11
this.ScenarioSetup(scenarioInfo);
#line 12
 testRunner.Given("I am a student agent");
#line 13
 testRunner.And("I have existing student availability");
#line 14
 testRunner.And("My schedule is published");
#line 15
 testRunner.When("I view student availability");
#line 16
 testRunner.Then("I should see my existing student availability");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No virtual schedule period")]
        public virtual void NoVirtualSchedulePeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No virtual schedule period", ((string[])(null)));
#line 18
this.ScenarioSetup(scenarioInfo);
#line 19
 testRunner.Given("I am a student agent");
#line 20
 testRunner.And("I do not have a virtual schedule period");
#line 21
 testRunner.When("I view student availability");
#line 22
 testRunner.Then("I should see a user-friendly message explaining I dont have anything to view");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No access to student availability menu item")]
        public virtual void NoAccessToStudentAvailabilityMenuItem()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No access to student availability menu item", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
 testRunner.Given("I am an agent without access to student availability");
#line 26
 testRunner.When("I am viewing an application page");
#line 27
 testRunner.Then("I should not be able to see student availability link");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No access to student availability page")]
        public virtual void NoAccessToStudentAvailabilityPage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No access to student availability page", ((string[])(null)));
#line 29
this.ScenarioSetup(scenarioInfo);
#line 30
 testRunner.Given("I am an agent without access to student availability");
#line 31
 testRunner.When("I am viewing an application page");
#line 32
 testRunner.And("I navigate to the student availability page");
#line 33
 testRunner.Then("I should see an error message");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate next virtual schedule period")]
        public virtual void NavigateNextVirtualSchedulePeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate next virtual schedule period", ((string[])(null)));
#line 35
this.ScenarioSetup(scenarioInfo);
#line 36
 testRunner.Given("I am a student agent");
#line 37
 testRunner.And("I have several virtual schedule periods");
#line 38
 testRunner.And("I am viewing student availability");
#line 39
 testRunner.When("I click next virtual schedule period button");
#line 40
 testRunner.Then("I should see next virtual schedule period");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate previous virtual schedule period")]
        public virtual void NavigatePreviousVirtualSchedulePeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate previous virtual schedule period", ((string[])(null)));
#line 42
this.ScenarioSetup(scenarioInfo);
#line 43
 testRunner.Given("I am a student agent");
#line 44
 testRunner.And("I have several virtual schedule periods");
#line 45
 testRunner.And("I am viewing student availability");
#line 46
 testRunner.When("I click previous virtual schedule period button");
#line 47
 testRunner.Then("I should see previous virtual schedule period");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Select period from period-picker")]
        public virtual void SelectPeriodFromPeriod_Picker()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Select period from period-picker", ((string[])(null)));
#line 49
this.ScenarioSetup(scenarioInfo);
#line 50
 testRunner.Given("I am a student agent");
#line 51
 testRunner.And("I am viewing student availability");
#line 52
 testRunner.When("I open the period-picker");
#line 53
 testRunner.And("I click on any day of a week");
#line 54
 testRunner.Then("the period-picker should close");
#line 55
 testRunner.And("I should see the selected virtual schedule period");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add student availability")]
        public virtual void AddStudentAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add student availability", ((string[])(null)));
#line 59
this.ScenarioSetup(scenarioInfo);
#line 60
 testRunner.Given("I am a student agent");
#line 61
 testRunner.And("I am in an open student availability period");
#line 62
 testRunner.And("I am viewing student availability");
#line 63
 testRunner.When("I select an editable day without student availability");
#line 64
 testRunner.And("I click the edit button");
#line 65
 testRunner.And("I input student availability values");
#line 66
 testRunner.And("I click the OK button");
#line 67
 testRunner.Then("I should see the student availability in the calendar");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add student availability with end time on next day")]
        public virtual void AddStudentAvailabilityWithEndTimeOnNextDay()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add student availability with end time on next day", ((string[])(null)));
#line 69
this.ScenarioSetup(scenarioInfo);
#line 70
 testRunner.Given("I am a student agent");
#line 71
 testRunner.And("I am in an open student availability period");
#line 72
 testRunner.And("I am viewing student availability");
#line 73
 testRunner.When("I select an editable day without student availability");
#line 74
 testRunner.And("I click the edit button");
#line 75
 testRunner.And("I input student availability values with end time on next day");
#line 76
 testRunner.And("I click the OK button");
#line 77
 testRunner.Then("I should see the student availability in the calendar");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Clicking edit student availability")]
        public virtual void ClickingEditStudentAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Clicking edit student availability", ((string[])(null)));
#line 79
this.ScenarioSetup(scenarioInfo);
#line 80
 testRunner.Given("I am a student agent");
#line 81
 testRunner.And("I am in an open student availability period");
#line 82
 testRunner.And("I have a student availability");
#line 83
 testRunner.And("I am viewing student availability");
#line 84
 testRunner.When("I select a day with student availability");
#line 85
 testRunner.And("I click the edit button");
#line 86
 testRunner.Then("I should see the student availability values in the input form");
#line 87
 testRunner.And("the calendar is disabled");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cancelling student availability editing")]
        public virtual void CancellingStudentAvailabilityEditing()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cancelling student availability editing", ((string[])(null)));
#line 89
this.ScenarioSetup(scenarioInfo);
#line 90
 testRunner.Given("I am a student agent");
#line 91
 testRunner.And("I am in an open student availability period");
#line 92
 testRunner.And("I have a student availability");
#line 93
 testRunner.And("I am viewing student availability");
#line 94
 testRunner.When("I select the day with student availability");
#line 95
 testRunner.And("I click the edit button");
#line 96
 testRunner.And("I click the cancel button");
#line 97
 testRunner.Then("I should not see the student availability values");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Editing student availability")]
        public virtual void EditingStudentAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Editing student availability", ((string[])(null)));
#line 99
this.ScenarioSetup(scenarioInfo);
#line 100
 testRunner.Given("I am a student agent");
#line 101
 testRunner.And("I am in an open student availability period");
#line 102
 testRunner.And("I have a student availability");
#line 103
 testRunner.And("I am viewing student availability");
#line 104
 testRunner.When("I select a day with student availability");
#line 105
 testRunner.And("I click the edit button");
#line 106
 testRunner.And("I input student availability values");
#line 107
 testRunner.And("I click the OK button");
#line 108
 testRunner.Then("I should see the new student availability values in the calendar");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Deleting student availability")]
        public virtual void DeletingStudentAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deleting student availability", ((string[])(null)));
#line 110
this.ScenarioSetup(scenarioInfo);
#line 111
 testRunner.Given("I am a student agent");
#line 112
 testRunner.And("I am in an open student availability period");
#line 113
 testRunner.And("I have a student availability");
#line 114
 testRunner.And("I am viewing student availability");
#line 115
 testRunner.When("I select a day with student availability");
#line 116
 testRunner.And("I click the delete button");
#line 117
 testRunner.Then("the student availability values in the calendar should disappear");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding invalid student availability values")]
        public virtual void AddingInvalidStudentAvailabilityValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding invalid student availability values", ((string[])(null)));
#line 119
this.ScenarioSetup(scenarioInfo);
#line 120
 testRunner.Given("I am a student agent");
#line 121
 testRunner.And("I am in an open student availability period");
#line 122
 testRunner.And("I am viewing student availability");
#line 123
 testRunner.When("I select an editable day without student availability");
#line 124
 testRunner.And("I click the edit button");
#line 125
 testRunner.And("I input invalid student availability values");
#line 126
 testRunner.And("I click the OK button");
#line 127
 testRunner.Then("I should see a message saying I have given an invalid time value");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not edit student availability without workflow control set")]
        public virtual void CanNotEditStudentAvailabilityWithoutWorkflowControlSet()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not edit student availability without workflow control set", ((string[])(null)));
#line 133
this.ScenarioSetup(scenarioInfo);
#line 134
 testRunner.Given("I am a student agent");
#line 135
 testRunner.And("I do not have a workflow control set");
#line 136
 testRunner.When("I view student availability");
#line 137
 testRunner.Then("I should see a message saying I am missing a workflow control set");
#line 138
 testRunner.And("the student availability calendar should not be editable");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Display student availability period information")]
        public virtual void DisplayStudentAvailabilityPeriodInformation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Display student availability period information", ((string[])(null)));
#line 140
this.ScenarioSetup(scenarioInfo);
#line 141
 testRunner.Given("I am a student agent");
#line 142
 testRunner.And("I have a workflow control set");
#line 143
 testRunner.When("I view student availability");
#line 144
 testRunner.Then("I should see the student availability period information");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not edit student availability in closed period")]
        public virtual void CanNotEditStudentAvailabilityInClosedPeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not edit student availability in closed period", ((string[])(null)));
#line 146
this.ScenarioSetup(scenarioInfo);
#line 147
 testRunner.Given("I am a student agent");
#line 148
 testRunner.And("I have a workflow control set with closed student availability periods");
#line 149
 testRunner.When("I view student availability");
#line 150
 testRunner.Then("the student availability calendar should not be editable");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can edit student availability in open period")]
        public virtual void CanEditStudentAvailabilityInOpenPeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can edit student availability in open period", ((string[])(null)));
#line 152
this.ScenarioSetup(scenarioInfo);
#line 153
 testRunner.Given("I am a student agent");
#line 154
 testRunner.And("I have a workflow control set with open availability periods");
#line 155
 testRunner.When("I view student availability");
#line 156
 testRunner.Then("the student availabilty calendar should be editable");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default to first virtual schedule period overlapping open student availability pe" +
            "riod")]
        public virtual void DefaultToFirstVirtualSchedulePeriodOverlappingOpenStudentAvailabilityPeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default to first virtual schedule period overlapping open student availability pe" +
                    "riod", ((string[])(null)));
#line 158
this.ScenarioSetup(scenarioInfo);
#line 159
 testRunner.Given("I am a student agent");
#line 160
 testRunner.And("I have a workflow control set with student availability periods open next month");
#line 161
 testRunner.When("I view student availability");
#line 162
 testRunner.Then("I should see the first virtual schedule period overlapping open student availabil" +
                    "ity period");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
