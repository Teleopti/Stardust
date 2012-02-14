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
    [NUnit.Framework.DescriptionAttribute("Team schedule")]
    public partial class TeamScheduleFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "TeamSchedule.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Team schedule", "In order to know when my colleagues work\r\nAs an agent\r\nI want to see my team mate" +
                    "s\' schedules", GenerationTargetLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Team schedule tab")]
        public virtual void TeamScheduleTab()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Team schedule tab", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.When("I sign in");
#line 9
 testRunner.Then("I should see the team schedule tab");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View team schedule")]
        public virtual void ViewTeamSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View team schedule", ((string[])(null)));
#line 11
this.ScenarioSetup(scenarioInfo);
#line 12
 testRunner.Given("I am an agent in a team");
#line 13
 testRunner.And("I have a shift today");
#line 14
 testRunner.And("I have a colleague");
#line 15
 testRunner.And("My colleague has a shift today");
#line 16
 testRunner.When("I view team schedule");
#line 17
 testRunner.Then("I should see my schedule");
#line 18
 testRunner.And("I should see my colleague\'s schedule");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View only my team\'s schedule")]
        public virtual void ViewOnlyMyTeamSSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View only my team\'s schedule", ((string[])(null)));
#line 20
this.ScenarioSetup(scenarioInfo);
#line 21
 testRunner.Given("I am an agent in a team with access to the whole site");
#line 22
 testRunner.And("I have a shift today");
#line 23
 testRunner.And("I have a colleague");
#line 24
 testRunner.And("My colleague has a shift today");
#line 25
 testRunner.And("I have a colleague in another team");
#line 26
 testRunner.And("The colleague in the other team has a shift today");
#line 27
 testRunner.When("I view team schedule");
#line 28
 testRunner.Then("I should see my schedule");
#line 29
 testRunner.And("I should see my colleague\'s schedule");
#line 30
 testRunner.And("I should not see the other colleague\'s schedule");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View team schedule, day off")]
        public virtual void ViewTeamScheduleDayOff()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View team schedule, day off", ((string[])(null)));
#line 32
this.ScenarioSetup(scenarioInfo);
#line 33
 testRunner.Given("I am an agent in a team");
#line 34
 testRunner.And("I have a colleague");
#line 35
 testRunner.And("My colleague has a dayoff today");
#line 36
 testRunner.When("I view team schedule");
#line 37
 testRunner.Then("I should see my colleague\'s day off");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View team schedule, absence")]
        public virtual void ViewTeamScheduleAbsence()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View team schedule, absence", ((string[])(null)));
#line 39
this.ScenarioSetup(scenarioInfo);
#line 40
 testRunner.Given("I am an agent in a team");
#line 41
 testRunner.And("I have a colleague");
#line 42
 testRunner.And("My colleague has an absence today");
#line 43
 testRunner.When("I view team schedule");
#line 44
 testRunner.Then("I should see my colleague\'s absence");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View team schedule, no shift")]
        public virtual void ViewTeamScheduleNoShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View team schedule, no shift", ((string[])(null)));
#line 55
this.ScenarioSetup(scenarioInfo);
#line 56
 testRunner.Given("I am an agent in a team");
#line 57
 testRunner.And("I have a colleague");
#line 58
 testRunner.When("I view team schedule");
#line 59
 testRunner.Then("I should see myself without schedule");
#line 60
 testRunner.And("I should see my colleague without schedule");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can\'t see confidential absence")]
        public virtual void CanTSeeConfidentialAbsence()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can\'t see confidential absence", ((string[])(null)));
#line 62
this.ScenarioSetup(scenarioInfo);
#line 63
 testRunner.Given("I am an agent in a team");
#line 64
 testRunner.And("I have a colleague");
#line 65
 testRunner.And("My colleague has a confidential absence");
#line 66
 testRunner.When("I view team schedule");
#line 67
 testRunner.Then("I should see my colleague\'s schedule");
#line 68
 testRunner.And("I should not see the absence\'s color");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can\'t see the team schedule tab without permission")]
        public virtual void CanTSeeTheTeamScheduleTabWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can\'t see the team schedule tab without permission", ((string[])(null)));
#line 70
this.ScenarioSetup(scenarioInfo);
#line 71
 testRunner.Given("I am an agent with no access to team schedule");
#line 72
 testRunner.When("I sign in");
#line 73
 testRunner.Then("I should not see the team schedule tab");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can\'t navigate to team schedule without permission")]
        public virtual void CanTNavigateToTeamScheduleWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can\'t navigate to team schedule without permission", ((string[])(null)));
#line 75
this.ScenarioSetup(scenarioInfo);
#line 76
 testRunner.Given("I am an agent with no access to team schedule");
#line 77
 testRunner.When("I sign in");
#line 78
 testRunner.And("I navigate to the team schedule");
#line 79
 testRunner.Then("I should see an error message");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can\'t see colleagues schedule without permission")]
        public virtual void CanTSeeColleaguesScheduleWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can\'t see colleagues schedule without permission", ((string[])(null)));
#line 81
this.ScenarioSetup(scenarioInfo);
#line 82
 testRunner.Given("I am an agent in a team with access only to my own data");
#line 83
 testRunner.And("I have a colleague");
#line 84
 testRunner.And("My colleague has a shift today");
#line 85
 testRunner.When("I view team schedule");
#line 86
 testRunner.Then("I should not see my colleagues schedule");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View time line +/- whole quarter of an hour")]
        public virtual void ViewTimeLine_WholeQuarterOfAnHour()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View time line +/- whole quarter of an hour", ((string[])(null)));
#line 88
this.ScenarioSetup(scenarioInfo);
#line 89
 testRunner.Given("I am an agent in a team");
#line 90
 testRunner.And("I have a shift from 7:56 to 17:00");
#line 91
 testRunner.And("I have a colleague");
#line 92
 testRunner.And("My colleague has a shift from 9:00 to 18:01");
#line 93
 testRunner.When("I view team schedule");
#line 94
 testRunner.Then("The time line should span from 7:30 to 18:30");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View time line default")]
        public virtual void ViewTimeLineDefault()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View time line default", ((string[])(null)));
#line 96
this.ScenarioSetup(scenarioInfo);
#line 97
 testRunner.Given("I am an agent in my own team");
#line 98
 testRunner.When("I view team schedule");
#line 99
 testRunner.Then("The time line should span from 7:45 to 17:15");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View time line default in hawaii")]
        public virtual void ViewTimeLineDefaultInHawaii()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View time line default in hawaii", ((string[])(null)));
#line 101
this.ScenarioSetup(scenarioInfo);
#line 102
 testRunner.Given("I am an agent in my own team");
#line 103
 testRunner.And("I am located in hawaii");
#line 104
 testRunner.When("I view team schedule");
#line 105
 testRunner.Then("The time line should span from 7:45 to 17:15");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to the next day")]
        public virtual void NavigateToTheNextDay()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to the next day", ((string[])(null)));
#line 107
this.ScenarioSetup(scenarioInfo);
#line 108
 testRunner.Given("I am an agent");
#line 109
 testRunner.And("I view team schedule");
#line 110
 testRunner.When("I click the next day button");
#line 111
 testRunner.Then("I should see the next day");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to the previous day")]
        public virtual void NavigateToThePreviousDay()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to the previous day", ((string[])(null)));
#line 113
this.ScenarioSetup(scenarioInfo);
#line 114
 testRunner.Given("I am an agent");
#line 115
 testRunner.And("I view team schedule");
#line 116
 testRunner.When("I click the previous day button");
#line 117
 testRunner.Then("I should see the previous day");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Select day from day-picker")]
        public virtual void SelectDayFromDay_Picker()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Select day from day-picker", ((string[])(null)));
#line 119
this.ScenarioSetup(scenarioInfo);
#line 120
 testRunner.Given("I am an agent in a team");
#line 121
 testRunner.And("I view team schedule");
#line 122
 testRunner.When("I open the day-picker");
#line 123
 testRunner.And("I click on a day");
#line 124
 testRunner.Then("the day-picker should close");
#line 125
 testRunner.And("I should see the selected day");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sort late shifts after early shifts")]
        public virtual void SortLateShiftsAfterEarlyShifts()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort late shifts after early shifts", ((string[])(null)));
#line 127
this.ScenarioSetup(scenarioInfo);
#line 128
 testRunner.Given("I am an agent in a team");
#line 129
 testRunner.And("I have a shift from 9:00 to 17:00");
#line 130
 testRunner.And("I have a colleague");
#line 131
 testRunner.And("My colleague has a shift from 8:00 to 18:00");
#line 132
 testRunner.When("I view team schedule");
#line 133
 testRunner.Then("I should see my colleague before myself");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sort full-day absences after shifts")]
        public virtual void SortFull_DayAbsencesAfterShifts()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort full-day absences after shifts", ((string[])(null)));
#line 135
this.ScenarioSetup(scenarioInfo);
#line 136
 testRunner.Given("I am an agent in a team");
#line 137
 testRunner.And("I have a full-day absence today");
#line 138
 testRunner.And("I have a colleague");
#line 139
 testRunner.And("My colleague has a shift from 9:00 to 17:00");
#line 140
 testRunner.When("I view team schedule");
#line 141
 testRunner.Then("I should see my colleague before myself");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sort day offs after the absences")]
        public virtual void SortDayOffsAfterTheAbsences()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort day offs after the absences", ((string[])(null)));
#line 143
this.ScenarioSetup(scenarioInfo);
#line 144
 testRunner.Given("I am an agent in a team");
#line 145
 testRunner.And("I have a dayoff today");
#line 146
 testRunner.And("I have a colleague");
#line 147
 testRunner.And("My colleague has a full-day absence today");
#line 148
 testRunner.When("I view team schedule");
#line 149
 testRunner.Then("I should see my colleague before myself");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sort no schedule last")]
        public virtual void SortNoScheduleLast()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort no schedule last", ((string[])(null)));
#line 151
this.ScenarioSetup(scenarioInfo);
#line 152
 testRunner.Given("I am an agent in a team");
#line 153
 testRunner.And("I have a colleague");
#line 154
 testRunner.And("My colleague has a dayoff today");
#line 155
 testRunner.When("I view team schedule");
#line 156
 testRunner.Then("I should see my colleague before myself");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show tooltip with activity name and times")]
        public virtual void ShowTooltipWithActivityNameAndTimes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show tooltip with activity name and times", ((string[])(null)));
#line 158
this.ScenarioSetup(scenarioInfo);
#line 159
 testRunner.Given("I am an agent in a team");
#line 160
 testRunner.And("I have an activity from 8:00 to 12:00");
#line 161
 testRunner.When("I view team schedule");
#line 162
 testRunner.Then("The layer\'s start time attibute value should be 08:00");
#line 163
 testRunner.And("The layer\'s end time attibute value should be 12:00");
#line 164
 testRunner.And("The layer\'s activity name attibute value should be Phone");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show team-picker with multiple teams")]
        public virtual void ShowTeam_PickerWithMultipleTeams()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show team-picker with multiple teams", ((string[])(null)));
#line 166
this.ScenarioSetup(scenarioInfo);
#line 167
 testRunner.Given("I am an agent in a team with access to the whole site");
#line 168
 testRunner.And("the site has another team");
#line 169
 testRunner.And("I am viewing team schedule");
#line 170
 testRunner.When("I open the team-picker");
#line 171
 testRunner.Then("I should see the team-picker with both teams");
#line 172
 testRunner.And("the teams should be sorted alphabetically");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show other team\'s schedule")]
        public virtual void ShowOtherTeamSSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show other team\'s schedule", ((string[])(null)));
#line 174
this.ScenarioSetup(scenarioInfo);
#line 175
 testRunner.Given("I am an agent in a team with access to the whole site");
#line 176
 testRunner.And("I have a colleague in another team");
#line 177
 testRunner.And("I am viewing team schedule");
#line 178
 testRunner.When("I select the other team in the team picker");
#line 179
 testRunner.Then("I should see my colleague");
#line 180
 testRunner.And("I should not see myself");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Keep selected team when changing date")]
        public virtual void KeepSelectedTeamWhenChangingDate()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Keep selected team when changing date", ((string[])(null)));
#line 182
this.ScenarioSetup(scenarioInfo);
#line 183
 testRunner.Given("I am an agent in a team with access to the whole site");
#line 184
 testRunner.And("I have a colleague in another team");
#line 185
 testRunner.And("I am viewing team schedule");
#line 186
 testRunner.When("I select the other team in the team picker");
#line 187
 testRunner.And("I click the next day button");
#line 188
 testRunner.Then("I should see my colleague");
#line 189
 testRunner.And("I should not see myself");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Keep selected date when changing team")]
        public virtual void KeepSelectedDateWhenChangingTeam()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Keep selected date when changing team", ((string[])(null)));
#line 191
this.ScenarioSetup(scenarioInfo);
#line 192
 testRunner.Given("I am an agent in a team with access to the whole site");
#line 193
 testRunner.And("the site has another team");
#line 194
 testRunner.And("I am viewing team schedule for tomorrow");
#line 195
 testRunner.When("I select the other team in the team picker");
#line 196
 testRunner.Then("I should see tomorrow");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show team-picker with teams for my site for another day")]
        public virtual void ShowTeam_PickerWithTeamsForMySiteForAnotherDay()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show team-picker with teams for my site for another day", ((string[])(null)));
#line 198
this.ScenarioSetup(scenarioInfo);
#line 199
 testRunner.Given("I am an agent in a team with access to the whole site");
#line 200
 testRunner.And("I belong to another site\'s team tomorrow");
#line 201
 testRunner.And("I am viewing team schedule for today");
#line 202
 testRunner.When("I click the next day button");
#line 203
 testRunner.Then("I should see the team-picker with the other site\'s team");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show default team when no access to a team on a date")]
        public virtual void ShowDefaultTeamWhenNoAccessToATeamOnADate()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show default team when no access to a team on a date", ((string[])(null)));
#line 205
this.ScenarioSetup(scenarioInfo);
#line 206
 testRunner.Given("I am an agent in a team with access to the whole site");
#line 207
 testRunner.And("I belong to another site\'s team tomorrow");
#line 208
 testRunner.And("I am viewing team schedule for today");
#line 209
 testRunner.When("I click the next day button");
#line 210
 testRunner.Then("I should see the other site\'s team");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default to my team")]
        public virtual void DefaultToMyTeam()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default to my team", ((string[])(null)));
#line 212
this.ScenarioSetup(scenarioInfo);
#line 213
 testRunner.Given("I am an agent in a team with access to the whole site");
#line 214
 testRunner.And("the site has another team");
#line 215
 testRunner.And("I am viewing team schedule");
#line 216
 testRunner.Then("the team-picker should have my team selected");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default to first team if no access to my team")]
        public virtual void DefaultToFirstTeamIfNoAccessToMyTeam()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default to first team if no access to my team", ((string[])(null)));
#line 218
this.ScenarioSetup(scenarioInfo);
#line 219
 testRunner.Given("I am an agent in a team with access to another site");
#line 220
 testRunner.And("the other site has 2 teams");
#line 221
 testRunner.And("I am viewing team schedule");
#line 222
 testRunner.Then("the team-picker should have the first of the other site\'s teams selected");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Don\'t show team-picker with no team access")]
        public virtual void DonTShowTeam_PickerWithNoTeamAccess()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Don\'t show team-picker with no team access", ((string[])(null)));
#line 224
this.ScenarioSetup(scenarioInfo);
#line 225
 testRunner.Given("I am an agent in a team with access only to my own data");
#line 226
 testRunner.When("I view team schedule");
#line 227
 testRunner.Then("I should not see the team-picker");
#line hidden
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Don\'t show team-picker with only one team")]
        public virtual void DonTShowTeam_PickerWithOnlyOneTeam()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Don\'t show team-picker with only one team", ((string[])(null)));
#line 229
this.ScenarioSetup(scenarioInfo);
#line 230
 testRunner.Given("I am an agent in a team with access to my team");
#line 231
 testRunner.When("I view team schedule");
#line 232
 testRunner.Then("I should not see the team-picker");
#line hidden
            testRunner.CollectScenarioErrors();
        }
    }
}
#endregion
