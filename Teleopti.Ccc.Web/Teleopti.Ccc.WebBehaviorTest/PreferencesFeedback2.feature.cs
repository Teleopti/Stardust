﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.261
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
        [NUnit.Framework.DescriptionAttribute("Feedback for a day without restrictions")]
        public virtual void FeedbackForADayWithoutRestrictions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day without restrictions", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.And("I have a shift bag with start times 8 to 9 and end times 16 to 17");
#line 9
 testRunner.When("I view preferences");
#line 10
 testRunner.Then("I should see the start time boundry 8 to 9");
#line 11
 testRunner.And("I should see the end time boundry 16 to 17");
#line 12
 testRunner.And("I should see the contract time boundry 7 to 9");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with day off preference")]
        public virtual void FeedbackForADayWithDayOffPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with day off preference", ((string[])(null)));
#line 17
this.ScenarioSetup(scenarioInfo);
#line 18
 testRunner.Given("I am an agent");
#line 19
 testRunner.And("I have a shift bag");
#line 20
 testRunner.And("I have a day off preference");
#line 21
 testRunner.When("I view preferences");
#line 22
 testRunner.Then("I should see no feedback");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with absence preference")]
        public virtual void FeedbackForADayWithAbsencePreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with absence preference", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
 testRunner.Given("I am an agent");
#line 26
 testRunner.And("I have a shift bag");
#line 27
 testRunner.And("I have a absence preference");
#line 28
 testRunner.When("I view preferences");
#line 29
 testRunner.Then("I should see no feedback");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with shift category preference")]
        public virtual void FeedbackForADayWithShiftCategoryPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with shift category preference", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 32
 testRunner.Given("I am an agent");
#line 33
 testRunner.And("I have a shift bag with two categories with shift from 8 to 16 and from 12 to 19");
#line 34
 testRunner.And("I have preference for the first category today");
#line 35
 testRunner.When("I view preferences");
#line 36
 testRunner.Then("I should see the start time boundry 8 to 8");
#line 37
 testRunner.And("I should see the end time boundry 16 to 16");
#line 38
 testRunner.And("I should see the contract time boundry 8 to 8");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with start time limitation preference")]
        public virtual void FeedbackForADayWithStartTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with start time limitation preference", ((string[])(null)));
#line 40
this.ScenarioSetup(scenarioInfo);
#line 41
 testRunner.Given("I am an agent");
#line 42
 testRunner.And("I have a shift bag with start times 8 to 13 and end times 12 to 22");
#line 43
 testRunner.And("I have a preference with start time limitation between 8 and 10");
#line 44
 testRunner.When("I view preferences");
#line 45
 testRunner.Then("I should see the start time boundry 8 to 10");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with end time limitation preference")]
        public virtual void FeedbackForADayWithEndTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with end time limitation preference", ((string[])(null)));
#line 47
this.ScenarioSetup(scenarioInfo);
#line 48
 testRunner.Given("I am an agent");
#line 49
 testRunner.And("I have a shift bag with start times 8 to 9 and end times 12 to 22");
#line 50
 testRunner.And("I have a preference with end time limitation between 13 and 19");
#line 51
 testRunner.When("I view preferences");
#line 52
 testRunner.Then("I should see the end time boundry 13 to 19");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with work time limitation preference")]
        public virtual void FeedbackForADayWithWorkTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with work time limitation preference", ((string[])(null)));
#line 54
this.ScenarioSetup(scenarioInfo);
#line 55
 testRunner.Given("I am an agent");
#line 56
 testRunner.And("I have a shift bag with start times 8 to 9 and end times 12 to 22");
#line 57
 testRunner.And("I have a preference with work time limitation between 4 and 5");
#line 58
 testRunner.When("I view preferences");
#line 59
 testRunner.Then("I should see the contract time boundry 4 to 5");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with lunch start time limitation preference")]
        public virtual void FeedbackForADayWithLunchStartTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with lunch start time limitation preference", ((string[])(null)));
#line 61
this.ScenarioSetup(scenarioInfo);
#line 62
 testRunner.Given("I am an agent");
#line 63
 testRunner.And("I have a shift bag");
#line 64
 testRunner.And("I have a preference with lunch start time limitation between 11:00 and 12:00");
#line 65
 testRunner.When("I view preferences");
#line 66
 testRunner.Then("I should see the start time boundry for the shift bag\'s shifts matching the prefe" +
                    "rence");
#line 67
 testRunner.And("I should see the end time boundry for the shift bag\'s shifts matching the prefere" +
                    "nce");
#line 68
 testRunner.And("I should see the minimum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line 69
 testRunner.And("I should see the maximum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with lunch end time limitation preference")]
        public virtual void FeedbackForADayWithLunchEndTimeLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with lunch end time limitation preference", ((string[])(null)));
#line 71
this.ScenarioSetup(scenarioInfo);
#line 72
 testRunner.Given("I am an agent");
#line 73
 testRunner.And("I have a shift bag");
#line 74
 testRunner.And("I have a preference with lunch end time limitation between 12:00 and 13:00");
#line 75
 testRunner.When("I view preferences");
#line 76
 testRunner.Then("I should see the start time boundry for the shift bag\'s shifts matching the prefe" +
                    "rence");
#line 77
 testRunner.And("I should see the end time boundry for the shift bag\'s shifts matching the prefere" +
                    "nce");
#line 78
 testRunner.And("I should see the minimum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line 79
 testRunner.And("I should see the maximum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with lunch length limitation preference")]
        public virtual void FeedbackForADayWithLunchLengthLimitationPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with lunch length limitation preference", ((string[])(null)));
#line 81
this.ScenarioSetup(scenarioInfo);
#line 82
 testRunner.Given("I am an agent");
#line 83
 testRunner.And("I have a shift bag with one shift 8 to 17 and lunch 12 to 13 and one shift 9 to 1" +
                    "9 and lunch 12 to 14");
#line 84
 testRunner.And("I have a preference with lunch length limitation of 1 hour today");
#line 85
 testRunner.When("I view preferences");
#line 86
 testRunner.Then("I should see the start time boundry 8 to 8");
#line 87
 testRunner.And("I should see the end time boundry 17 to 17");
#line 88
 testRunner.And("I should see the contract time boundry 9 to 9");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with availability")]
        public virtual void FeedbackForADayWithAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with availability", ((string[])(null)));
#line 93
this.ScenarioSetup(scenarioInfo);
#line 94
 testRunner.Given("I am an agent");
#line 95
 testRunner.And("I have a shift bag");
#line 96
 testRunner.And("I have availability");
#line 97
 testRunner.When("I view preferences");
#line 98
 testRunner.Then("I should see the start time boundry for the shift bag\'s shifts that match the ava" +
                    "ilability");
#line 99
 testRunner.And("I should see the end time boundry for the shift bag\'s shifts that match the avail" +
                    "ability");
#line 100
 testRunner.And("I should see the minimum contract time for the shift bag\'s shifts that match the " +
                    "availability");
#line 101
 testRunner.And("I should see the maximum contract time for the shift bag\'s shifts that match the " +
                    "availability");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with start time limitation availability")]
        public virtual void FeedbackForADayWithStartTimeLimitationAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with start time limitation availability", ((string[])(null)));
#line 103
this.ScenarioSetup(scenarioInfo);
#line 104
 testRunner.Given("I am an agent");
#line 105
 testRunner.And("I have a shift bag");
#line 106
 testRunner.And("I have a availability with start time limitation of 7:00 at the earliest");
#line 107
 testRunner.When("I view preferences");
#line 108
 testRunner.Then("I should see the start time boundry for the shift bag\'s shifts matching the prefe" +
                    "rence");
#line 109
 testRunner.And("I should see the end time boundry for the shift bag\'s shifts matching the prefere" +
                    "nce");
#line 110
 testRunner.And("I should see the minimum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line 111
 testRunner.And("I should see the maximum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with end time limitation availability")]
        public virtual void FeedbackForADayWithEndTimeLimitationAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with end time limitation availability", ((string[])(null)));
#line 113
this.ScenarioSetup(scenarioInfo);
#line 114
 testRunner.Given("I am an agent");
#line 115
 testRunner.And("I have a shift bag");
#line 116
 testRunner.And("I have a availability with end time limitation of 20:00 at the latest");
#line 117
 testRunner.When("I view preferences");
#line 118
 testRunner.Then("I should see the start time boundry for the shift bag\'s shifts matching the prefe" +
                    "rence");
#line 119
 testRunner.And("I should see the end time boundry for the shift bag\'s shifts matching the prefere" +
                    "nce");
#line 120
 testRunner.And("I should see the minimum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line 121
 testRunner.And("I should see the maximum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with work time limitation availability")]
        public virtual void FeedbackForADayWithWorkTimeLimitationAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with work time limitation availability", ((string[])(null)));
#line 123
this.ScenarioSetup(scenarioInfo);
#line 124
 testRunner.Given("I am an agent");
#line 125
 testRunner.And("I have a shift bag");
#line 126
 testRunner.And("I have a availability with work time limitation between 7 and 9 hours");
#line 127
 testRunner.When("I view preferences");
#line 128
 testRunner.Then("I should see the start time boundry for the shift bag\'s shifts matching the prefe" +
                    "rence");
#line 129
 testRunner.And("I should see the end time boundry for the shift bag\'s shifts matching the prefere" +
                    "nce");
#line 130
 testRunner.And("I should see the minimum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line 131
 testRunner.And("I should see the maximum contract time for the shift bag\'s shifts matching the pr" +
                    "eference");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with availability and preference")]
        public virtual void FeedbackForADayWithAvailabilityAndPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with availability and preference", ((string[])(null)));
#line 135
this.ScenarioSetup(scenarioInfo);
#line 136
 testRunner.Given("I am an agent");
#line 137
 testRunner.And("I have a shift bag");
#line 138
 testRunner.And("I have availability");
#line 139
 testRunner.And("I have preference with shift category AM");
#line 140
 testRunner.When("I view preferences");
#line 141
 testRunner.Then("I should see the start time boundry for the shift bag\'s shifts of category AM and" +
                    " that match the availability");
#line 142
 testRunner.And("I should see the end time boundry for the shift bag\'s shifts of category AM and t" +
                    "hat match the availability");
#line 143
 testRunner.And("I should see the minimum contract time for the shift bag\'s shifts of category AM " +
                    "and that match the availability");
#line 144
 testRunner.And("I should see the maximum contract time for the shift bag\'s shifts of category AM " +
                    "and that match the availability");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with a schedule")]
        public virtual void FeedbackForADayWithASchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with a schedule", ((string[])(null)));
#line 148
this.ScenarioSetup(scenarioInfo);
#line 149
 testRunner.Given("I am an agent");
#line 150
 testRunner.And("I have a shift");
#line 151
 testRunner.When("I view preferences");
#line 152
 testRunner.Then("I should see the start time of the shift");
#line 153
 testRunner.And("I should see the end time of the shift");
#line 154
 testRunner.And("I should see the contract time of the shift");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Feedback for a day with a schedule, preference and availability")]
        public virtual void FeedbackForADayWithASchedulePreferenceAndAvailability()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Feedback for a day with a schedule, preference and availability", ((string[])(null)));
#line 156
this.ScenarioSetup(scenarioInfo);
#line 157
 testRunner.Given("I am an agent");
#line 158
 testRunner.And("I have a shift");
#line 159
 testRunner.And("I have a shift bag");
#line 160
 testRunner.And("I have preference for shift category AM");
#line 161
 testRunner.And("I have availability");
#line 162
 testRunner.When("I view preferences");
#line 163
 testRunner.Then("I should see the start time of the shift");
#line 164
 testRunner.And("I should see the end time of the shift");
#line 165
 testRunner.And("I should see the contract time of the shift");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
