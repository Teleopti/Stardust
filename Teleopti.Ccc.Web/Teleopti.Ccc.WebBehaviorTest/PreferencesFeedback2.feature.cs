﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.269
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
    [NUnit.Framework.DescriptionAttribute("Preferences feedback")]
    public partial class PreferencesFeedbackFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "PreferencesFeedback.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Preferences feedback", "In order to know at which times I might work\r\nAs an agent\r\nI want feedback for my" +
                    " preferences", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with shift category preference")]
        public virtual void FeedbackForADayWithShiftCategoryPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with shift category preference", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.And("I am american");
#line 9
 testRunner.And("I have a shift bag with two categories with shift from 8 to 16 and from 12 to 19");
#line 10
 testRunner.And("I have preference for the first category today");
#line 11
 testRunner.When("I view preferences");
#line 12
 testRunner.Then("I should see the start time boundry 8 to 8");
#line 13
 testRunner.And("I should see the end time boundry 16 to 16");
#line 14
 testRunner.And("I should see the contract time boundry 8 to 8");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with start time limitation preference")]
        public virtual void FeedbackForADayWithStartTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with start time limitation preference", ((string[])(null)));
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
 testRunner.Given("I am an agent");
#line 18
 testRunner.And("I have a shift bag with start times 8 to 13 and end times 12 to 22");
#line 19
 testRunner.And("I have a preference with start time limitation between 8 and 10");
#line 20
 testRunner.When("I view preferences");
#line 21
 testRunner.Then("I should see the start time boundry 8 to 10");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with end time limitation preference")]
        public virtual void FeedbackForADayWithEndTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with end time limitation preference", ((string[])(null)));
#line 23
this.ScenarioSetup(scenarioInfo);
#line 24
 testRunner.Given("I am an agent");
#line 25
 testRunner.And("I have a shift bag with start times 8 to 9 and end times 12 to 22");
#line 26
 testRunner.And("I have a preference with end time limitation between 13 and 19");
#line 27
 testRunner.When("I view preferences");
#line 28
 testRunner.Then("I should see the end time boundry 13 to 19");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with work time limitation preference")]
        public virtual void FeedbackForADayWithWorkTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with work time limitation preference", ((string[])(null)));
#line 30
this.ScenarioSetup(scenarioInfo);
#line 31
 testRunner.Given("I am an agent");
#line 32
 testRunner.And("I have a shift bag with start times 8 to 9 and end times 12 to 22");
#line 33
 testRunner.And("I have a preference with work time limitation between 4 and 5");
#line 34
 testRunner.When("I view preferences");
#line 35
 testRunner.Then("I should see the contract time boundry 4 to 5");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with lunch start time limitation preference")]
        public virtual void FeedbackForADayWithLunchStartTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with lunch start time limitation preference", ((string[])(null)));
#line 37
this.ScenarioSetup(scenarioInfo);
#line 38
 testRunner.Given("I am an agent");
#line 39
 testRunner.And("I have a shift bag with one shift 8 to 17 and lunch 12 to 13 and one shift 9 to 1" +
                    "9 and lunch 13 to 14");
#line 40
 testRunner.And("I have a preference with lunch start time limitation between 13 and 13");
#line 41
 testRunner.When("I view preferences");
#line 42
 testRunner.Then("I should see the start time boundry 9 to 9");
#line 43
 testRunner.And("I should see the end time boundry 19 to 19");
#line 44
 testRunner.And("I should see the contract time boundry 10 to 10");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with lunch end time limitation preference")]
        public virtual void FeedbackForADayWithLunchEndTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with lunch end time limitation preference", ((string[])(null)));
#line 46
this.ScenarioSetup(scenarioInfo);
#line 47
 testRunner.Given("I am an agent");
#line 48
 testRunner.And("I have a shift bag with one shift 9 to 18 and lunch 12 to 13 and one shift 9 to 1" +
                    "9 and lunch 12 to 14");
#line 49
 testRunner.And("I have a preference with lunch end time limitation between 12 and 13");
#line 50
 testRunner.When("I view preferences");
#line 51
 testRunner.Then("I should see the start time boundry 9 to 9");
#line 52
 testRunner.And("I should see the end time boundry 18 to 18");
#line 53
 testRunner.And("I should see the contract time boundry 9 to 9");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with lunch length limitation preference")]
        public virtual void FeedbackForADayWithLunchLengthLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with lunch length limitation preference", ((string[])(null)));
#line 55
this.ScenarioSetup(scenarioInfo);
#line 56
 testRunner.Given("I am an agent");
#line 57
 testRunner.And("I have a shift bag with one shift 8 to 17 and lunch 12 to 13 and one shift 9 to 1" +
                    "9 and lunch 12 to 14");
#line 58
 testRunner.And("I have a preference with lunch length limitation of 1 hour today");
#line 59
 testRunner.When("I view preferences");
#line 60
 testRunner.Then("I should see the start time boundry 8 to 8");
#line 61
 testRunner.And("I should see the end time boundry 17 to 17");
#line 62
 testRunner.And("I should see the contract time boundry 9 to 9");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with start time limitation availability")]
        public virtual void FeedbackForADayWithStartTimeLimitationAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with start time limitation availability", ((string[])(null)));
#line 64
this.ScenarioSetup(scenarioInfo);
#line 65
 testRunner.Given("I am an agent");
#line 66
 testRunner.And("I have a shift bag with start times 8 to 13 and end times 12 to 22");
#line 67
 testRunner.And("I have a availabilty with earliest start time at 10");
#line 68
 testRunner.When("I view preferences");
#line 69
 testRunner.Then("I should see the start time boundry 10 to 13");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with end time limitation availability")]
        public virtual void FeedbackForADayWithEndTimeLimitationAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with end time limitation availability", ((string[])(null)));
#line 71
this.ScenarioSetup(scenarioInfo);
#line 72
 testRunner.Given("I am an agent");
#line 73
 testRunner.And("I have a shift bag with start times 8 to 13 and end times 12 to 22");
#line 74
 testRunner.And("I have a availabilty with latest end time at 21");
#line 75
 testRunner.When("I view preferences");
#line 76
 testRunner.Then("I should see the end time boundry 12 to 21");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with work time limitation availability")]
        public virtual void FeedbackForADayWithWorkTimeLimitationAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with work time limitation availability", ((string[])(null)));
#line 78
this.ScenarioSetup(scenarioInfo);
#line 79
 testRunner.Given("I am an agent");
#line 80
 testRunner.And("I have a shift bag with start times 8 to 13 and end times 12 to 22");
#line 81
 testRunner.And("I have a availabilty with work time between 5 and 7 hours");
#line 82
 testRunner.When("I view preferences");
#line 83
 testRunner.Then("I should see the contract time boundry 5 to 7");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with availability and preference")]
        public virtual void FeedbackForADayWithAvailabilityAndPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with availability and preference", ((string[])(null)));
#line 85
this.ScenarioSetup(scenarioInfo);
#line 86
 testRunner.Given("I am an agent");
#line 87
 testRunner.And("I have a shift bag with two categories with shift start from 8 to 10 and from 12 " +
                    "to 14 and end from 16 to 18 and from 12 to 20");
#line 88
 testRunner.And("I have preference for the first category today");
#line 89
 testRunner.And("I have a availabilty with earliest start time at 9");
#line 90
 testRunner.When("I view preferences");
#line 91
 testRunner.Then("I should see the start time boundry 9 to 10");
#line 92
 testRunner.And("I should see the end time boundry 16 to 18");
#line 93
 testRunner.And("I should see the contract time boundry 6 to 9");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with a schedule, preference and availability")]
        public virtual void FeedbackForADayWithASchedulePreferenceAndAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with a schedule, preference and availability", ((string[])(null)));
#line 95
this.ScenarioSetup(scenarioInfo);
#line 96
 testRunner.Given("I am an agent");
#line 97
 testRunner.And("I have a shift bag");
#line 98
 testRunner.And("I have a shift today");
#line 99
 testRunner.And("I have existing shift category preference");
#line 100
 testRunner.And("I have a availabilty with earliest start time at 9");
#line 101
 testRunner.When("I view preferences");
#line 102
 testRunner.Then("I should see my shift");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback from conflicting preferences and availability")]
        public virtual void FeedbackFromConflictingPreferencesAndAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback from conflicting preferences and availability", ((string[])(null)));
#line 104
this.ScenarioSetup(scenarioInfo);
#line 105
 testRunner.Given("I am an agent");
#line 106
 testRunner.And("I have a shift bag");
#line 107
 testRunner.And("I have a conflicting preference and availability today");
#line 108
 testRunner.When("I view preferences");
#line 109
 testRunner.Then("I should see that there are no available shifts");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback from an added preference")]
        public virtual void FeedbackFromAnAddedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback from an added preference", ((string[])(null)));
#line 111
this.ScenarioSetup(scenarioInfo);
#line 112
 testRunner.Given("I am an agent without access to extended preferences");
#line 113
 testRunner.And("I have an open workflow control set with an allowed standard preference");
#line 114
 testRunner.And("I have a shift bag");
#line 115
 testRunner.And("I am viewing preferences");
#line 116
 testRunner.When("I select an editable day without preference");
#line 117
 testRunner.And("I select a standard preference");
#line 118
 testRunner.Then("I should see the preference feedback");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback from a deleted preference")]
        public virtual void FeedbackFromADeletedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback from a deleted preference", ((string[])(null)));
#line 120
this.ScenarioSetup(scenarioInfo);
#line 121
 testRunner.Given("I am an agent");
#line 122
 testRunner.And("I have an open workflow control set with an allowed standard preference");
#line 123
 testRunner.And("I have a shift bag");
#line 124
 testRunner.And("I have existing standard preference");
#line 125
 testRunner.And("I am viewing preferences");
#line 126
 testRunner.When("I select an editable day with standard preference");
#line 127
 testRunner.And("I click the delete button");
#line 128
 testRunner.Then("I should see the preference feedback");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
