﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.239
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
    [NUnit.Framework.DescriptionAttribute("Preferences period feedback")]
    public partial class PreferencesPeriodFeedbackFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "PreferencesPeriodFeedback.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Preferences period feedback", "In order to know if my preferences are viable\r\nAs an agent\r\nI want feedback of my" +
                    " preferences compared to my contract for the period", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Period feedback of contract day off")]
        public virtual void PeriodFeedbackOfContractDayOff()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of contract day off", ((string[])(null)));
#line 8
this.ScenarioSetup(scenarioInfo);
#line 9
 testRunner.Given("I am an agent");
#line 10
 testRunner.And("I have a scheduling period of 1 week");
#line 11
 testRunner.And("I have a contract schedule with 2 days off");
#line 12
 testRunner.When("I view preferences");
#line 13
 testRunner.Then("I should see a message that I should have 2 days off");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of day off preferences")]
        public virtual void PeriodFeedbackOfDayOffPreferences()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of day off preferences", ((string[])(null)));
#line 17
this.ScenarioSetup(scenarioInfo);
#line 18
 testRunner.Given("I am an agent");
#line 19
 testRunner.And("I have a scheduling period of 1 week");
#line 20
 testRunner.And("I have a day off preference on weekday 3");
#line 21
 testRunner.And("I have a day off preference on weekday 5");
#line 22
 testRunner.When("I view preferences");
#line 23
 testRunner.Then("I should see a message that my preferences can result 2 days off");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of day off scheduled")]
        public virtual void PeriodFeedbackOfDayOffScheduled()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of day off scheduled", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("I am an agent");
#line 27
 testRunner.And("I have a scheduling period of 1 week");
#line 28
 testRunner.And("I have a day off scheduled on weekday 3");
#line 29
 testRunner.And("I have a day off scheduled on weekday 5");
#line 30
 testRunner.When("I view preferences");
#line 31
 testRunner.Then("I should see a message that my preferences can result 2 days off");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of day off preferences and scheduled")]
        public virtual void PeriodFeedbackOfDayOffPreferencesAndScheduled()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of day off preferences and scheduled", ((string[])(null)));
#line 33
this.ScenarioSetup(scenarioInfo);
#line 34
 testRunner.Given("I am an agent");
#line 35
 testRunner.And("I have a scheduling period of 1 week");
#line 36
 testRunner.And("I have a day off preference on weekday 3");
#line 37
 testRunner.And("I have a day off scheduled on weekday 5");
#line 38
 testRunner.When("I view preferences");
#line 39
 testRunner.Then("I should see a message that my preferences can result 2 days off");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of contract day off tolerance")]
        public virtual void PeriodFeedbackOfContractDayOffTolerance()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of contract day off tolerance", ((string[])(null)));
#line 43
this.ScenarioSetup(scenarioInfo);
#line 44
 testRunner.Given("I am an agent");
#line 45
 testRunner.And("I have a scheduling period of 1 week");
#line 46
 testRunner.And("I have a contract schedule with 2 days off");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table1.AddRow(new string[] {
                        "Positive day off tolerance",
                        "1"});
            table1.AddRow(new string[] {
                        "Negative day off tolerance",
                        "1"});
#line 47
 testRunner.And("I have a contract with:", ((string)(null)), table1);
#line 51
 testRunner.When("I view preferences");
#line 52
 testRunner.Then("I should see a message that I should have between 1 and 3 days off");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of absence on contract schedule day off")]
        public virtual void PeriodFeedbackOfAbsenceOnContractScheduleDayOff()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of absence on contract schedule day off", ((string[])(null)));
#line 56
this.ScenarioSetup(scenarioInfo);
#line 57
 testRunner.Given("I am an agent");
#line 58
 testRunner.And("I am swedish");
#line 59
 testRunner.And("I have a scheduling period of 1 week");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Monday work day",
                        "true"});
            table2.AddRow(new string[] {
                        "Tuesday work day",
                        "true"});
            table2.AddRow(new string[] {
                        "Wednesday work day",
                        "true"});
            table2.AddRow(new string[] {
                        "Thursday work day",
                        "true"});
            table2.AddRow(new string[] {
                        "Friday work day",
                        "true"});
            table2.AddRow(new string[] {
                        "Saturday work day",
                        "false"});
            table2.AddRow(new string[] {
                        "Sunday work day",
                        "false"});
#line 60
 testRunner.And("I have a contract schedule with:", ((string)(null)), table2);
#line 69
 testRunner.And("I have a absence preference on weekday 5");
#line 70
 testRunner.And("I have a absence preference on weekday 6");
#line 71
 testRunner.When("I view preferences");
#line 72
 testRunner.Then("I should see a message that my preferences can result 1 days off");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of contract time for employment type Fixed staff normal work time" +
            "")]
        public virtual void PeriodFeedbackOfContractTimeForEmploymentTypeFixedStaffNormalWorkTime()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of contract time for employment type Fixed staff normal work time" +
                    "", ((string[])(null)));
#line 76
this.ScenarioSetup(scenarioInfo);
#line 77
 testRunner.Given("I am an agent");
#line 78
 testRunner.And("I have a scheduling period of 1 week");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Employment type",
                        "Fixed staff normal work time"});
            table3.AddRow(new string[] {
                        "Average work time per day",
                        "8"});
#line 79
 testRunner.And("I have a contract with:", ((string)(null)), table3);
#line 83
 testRunner.And("I have a contract schedule with 2 days off");
#line 84
 testRunner.When("I view preferences");
#line 85
 testRunner.Then("I should see a message that I should work 40 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of contract time for employment type Fixed staff day work time")]
        public virtual void PeriodFeedbackOfContractTimeForEmploymentTypeFixedStaffDayWorkTime()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of contract time for employment type Fixed staff day work time", ((string[])(null)));
#line 87
this.ScenarioSetup(scenarioInfo);
#line 88
 testRunner.Given("I am an agent");
#line 89
 testRunner.And("I have a scheduling period of 1 week");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Employment type",
                        "Fixed staff day work time"});
            table4.AddRow(new string[] {
                        "Average work time per day",
                        "8"});
#line 90
 testRunner.And("I have a contract with:", ((string)(null)), table4);
#line 94
 testRunner.And("I have a day off preference on weekday 6");
#line 95
 testRunner.And("I have a day off preference on weekday 7");
#line 96
 testRunner.When("I view preferences");
#line 97
 testRunner.Then("I should see a message that I should work 40 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of contract time for employment type Fixed staff period work time" +
            "")]
        public virtual void PeriodFeedbackOfContractTimeForEmploymentTypeFixedStaffPeriodWorkTime()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of contract time for employment type Fixed staff period work time" +
                    "", ((string[])(null)));
#line 99
this.ScenarioSetup(scenarioInfo);
#line 100
 testRunner.Given("I am an agent");
#line 101
 testRunner.And("I have a scheduling period of 1 week");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Employment type",
                        "Fixed staff period work time"});
            table5.AddRow(new string[] {
                        "Average work time per day",
                        "8"});
#line 102
 testRunner.And("I have a contract with:", ((string)(null)), table5);
#line 106
 testRunner.And("I have a contract schedule with 2 days off");
#line 107
 testRunner.When("I view preferences");
#line 108
 testRunner.Then("I should see a message that I should work 40 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of contract time with target tolerance")]
        public virtual void PeriodFeedbackOfContractTimeWithTargetTolerance()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of contract time with target tolerance", ((string[])(null)));
#line 112
this.ScenarioSetup(scenarioInfo);
#line 113
 testRunner.Given("I am an agent");
#line 114
 testRunner.And("I have a scheduling period of 1 week");
#line 115
 testRunner.And("I have a contract with employment type Fixed staff normal work time");
#line 116
 testRunner.And("I have a contract with an 8 hour average work time per day");
#line 117
 testRunner.And("I have a contract with a target tolerance of negative 5 hours");
#line 118
 testRunner.And("I have a contract with a target tolerance of positive 5 hours");
#line 119
 testRunner.And("I have a contract schedule with 2 days off");
#line 120
 testRunner.When("I view preferences");
#line 121
 testRunner.Then("I should see a message that I should work 35 to 45 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of nothing")]
        public virtual void PeriodFeedbackOfNothing()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of nothing", ((string[])(null)));
#line 125
this.ScenarioSetup(scenarioInfo);
#line 126
 testRunner.Given("I am an agent");
#line 127
 testRunner.And("I have a scheduling period of 1 week");
#line 128
 testRunner.And("I have a shift bag with start times 7 to 9 and end times 15 to 17");
#line 129
 testRunner.When("I view preferences");
#line 130
 testRunner.Then("I should see a message that my preferences can result in 42 to 70 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of preferences")]
        public virtual void PeriodFeedbackOfPreferences()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of preferences", ((string[])(null)));
#line 132
this.ScenarioSetup(scenarioInfo);
#line 133
 testRunner.Given("I am an agent");
#line 134
 testRunner.And("I have a scheduling period of 1 week");
#line 135
 testRunner.And("I have a shift bag with start times 7 to 9 and end times 15 to 17");
#line 136
 testRunner.And("I have a shift category preference on weekday 1");
#line 137
 testRunner.And("I have a shift category preference on weekday 2");
#line 138
 testRunner.And("I have a shift category preference on weekday 3");
#line 139
 testRunner.And("I have a shift category preference on weekday 4");
#line 140
 testRunner.And("I have a shift category preference on weekday 5");
#line 141
 testRunner.And("I have a day off preference on weekday 6");
#line 142
 testRunner.And("I have a day off preference on weekday 7");
#line 143
 testRunner.When("I view preferences");
#line 144
 testRunner.Then("I should see a message that my preferences can result in 30 to 50 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of schedules")]
        public virtual void PeriodFeedbackOfSchedules()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of schedules", ((string[])(null)));
#line 146
this.ScenarioSetup(scenarioInfo);
#line 147
 testRunner.Given("I am an agent");
#line 148
 testRunner.And("I have a scheduling period of 1 week");
#line 149
 testRunner.And("I have a scheduled shift of 8 hours on weekday 1");
#line 150
 testRunner.And("I have a scheduled shift of 8 hours on weekday 2");
#line 151
 testRunner.And("I have a scheduled shift of 8 hours on weekday 3");
#line 152
 testRunner.And("I have a scheduled shift of 8 hours on weekday 4");
#line 153
 testRunner.And("I have a scheduled shift of 8 hours on weekday 5");
#line 154
 testRunner.And("I have a scheduled day off on weekday 6");
#line 155
 testRunner.And("I have a scheduled day off on weekday 7");
#line 156
 testRunner.When("I view preferences");
#line 157
 testRunner.Then("I should see a message that my preferences can result in 40 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of schedules and preferences")]
        public virtual void PeriodFeedbackOfSchedulesAndPreferences()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of schedules and preferences", ((string[])(null)));
#line 159
this.ScenarioSetup(scenarioInfo);
#line 160
 testRunner.Given("I am an agent");
#line 161
 testRunner.And("I have a scheduling period of 1 week");
#line 162
 testRunner.And("I have a shift bag with start times 7 to 9 and end times 15 to 17");
#line 163
 testRunner.And("I have a scheduled shift of 8 hours on weekday 1");
#line 164
 testRunner.And("I have a scheduled shift of 8 hours on weekday 2");
#line 165
 testRunner.And("I have a shift category preference on weekday 3");
#line 166
 testRunner.And("I have a shift category preference on weekday 4");
#line 167
 testRunner.And("I have a shift category preference on weekday 5");
#line 168
 testRunner.And("I have a scheduled day off on weekday 6");
#line 169
 testRunner.And("I have a day off preference on weekday 7");
#line 170
 testRunner.When("I view preferences");
#line 171
 testRunner.And("I should see a message that my preferences can result in 34 to 46 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of contract time absence")]
        public virtual void PeriodFeedbackOfContractTimeAbsence()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of contract time absence", ((string[])(null)));
#line 175
this.ScenarioSetup(scenarioInfo);
#line 176
 testRunner.Given("I am an agent");
#line 177
 testRunner.And("I have a scheduling period of 1 week");
#line 178
 testRunner.And("I have a contract with an 8 hour average work time per day");
#line 179
 testRunner.And("I have a contract schedule with weekday 6 day off");
#line 180
 testRunner.And("I have a contract schedule with weekday 7 day off");
#line 181
 testRunner.And("I have a contract time absence preference on weekday 1");
#line 182
 testRunner.And("I have a contract time absence preference on weekday 2");
#line 183
 testRunner.And("I have a contract time absence preference on weekday 3");
#line 184
 testRunner.And("I have a contract time absence preference on weekday 4");
#line 185
 testRunner.And("I have a contract time absence preference on weekday 5");
#line 186
 testRunner.And("I have a contract time absence preference on weekday 6");
#line 187
 testRunner.And("I have a contract time absence preference on weekday 7");
#line 188
 testRunner.When("I view preferences");
#line 189
 testRunner.Then("I should see a message that my preferences can result in 40 hours");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Period feedback of non-contract time absence")]
        public virtual void PeriodFeedbackOfNon_ContractTimeAbsence()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Period feedback of non-contract time absence", ((string[])(null)));
#line 191
this.ScenarioSetup(scenarioInfo);
#line 192
 testRunner.Given("I am an agent");
#line 193
 testRunner.And("I have a scheduling period of 1 week");
#line 194
 testRunner.And("I have a contract with an 8 hour average work time per day");
#line 195
 testRunner.And("I have a contract schedule with weekday 6 day off");
#line 196
 testRunner.And("I have a contract schedule with weekday 7 day off");
#line 197
 testRunner.And("I have a non-contract time absence preference on weekday 1");
#line 198
 testRunner.And("I have a non-contract time absence preference on weekday 2");
#line 199
 testRunner.And("I have a non-contract time absence preference on weekday 3");
#line 200
 testRunner.And("I have a non-contract time absence preference on weekday 4");
#line 201
 testRunner.And("I have a non-contract time absence preference on weekday 5");
#line 202
 testRunner.And("I have a non-contract time absence preference on weekday 6");
#line 203
 testRunner.And("I have a non-contract time absence preference on weekday 7");
#line 204
 testRunner.When("I view preferences");
#line 205
 testRunner.Then("I should see a message that my preferences can result in 0 hours");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
