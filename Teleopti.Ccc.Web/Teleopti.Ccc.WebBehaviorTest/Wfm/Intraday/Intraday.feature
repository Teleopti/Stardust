﻿@WFM
Feature: Intraday
    In order to be in control of my part of the business
    As an intraday analyst
    I want to be able to monitor my part of the business

Background:
    Given I have a role with
    | Field              | Value |
    | Access to Intraday | True  |
    And There is a skill to monitor called 'Skill A' with queue id '9' and queue name 'queue1' and activity 'activity1'
    And There is a skill to monitor called 'Skill B' with queue id '7' and queue name 'queue2' and activity 'activity2'
	And There is an email-like skill to monitor called 'Skill BackOffice' with queue id '3' and queue name 'queue3' and activity 'activity3'

@OnlyRunIfDisabled('WFM_Unified_Skill_Group_Management_45417')
Scenario: Create Skill Area
	Given I am viewing intraday page
	And I select to create a new Skill Area
	And I name the Skill Area 'my Area'
	And I select the skill 'Skill A'
	When I am done creating Skill Area 
	Then I select to monitor skill area 'my Area'
	And I should monitor 'my Area'

@OnlyRunIfEnabled('WFM_Unified_Skill_Group_Management_45417')
@OnlyRunIfDisabled('WFM_Modify_Skill_Groups_43727')
Scenario: Create Skill Group
	Given I am viewing intraday page
	And I select to create a new Skill Group
	And I name the Skill Area 'my Area'
	And I select the skill 'Skill A'
	When I am done creating Skill Area 
	Then I select to monitor skill area 'my Area'
	And I should monitor 'my Area'

@OnlyRunIfEnabled('WFM_Unified_Skill_Group_Management_45417')
@OnlyRunIfEnabled('WFM_Modify_Skill_Groups_43727')
Scenario: Create Skill Group in SGM
	Given I am viewing intraday page
	And I select to manage Skill Groups
	And I select to create a new Skill Group in SGM
	And I name the Skill Group 'my Area'
	And I select the skill 'Skill A' in SGM
	And I save the Skill Groups
	When I close the Skill Manager
	And I pick the skillgroup 'my Area'
	Then I should monitor 'my Area'

@OnlyRunIfEnabled('WFM_Unified_Skill_Group_Management_45417')
@OnlyRunIfEnabled('WFM_Modify_Skill_Groups_43727')
Scenario: Rename Skill Group in SGM
	Given I am viewing intraday page
	And I select to manage Skill Groups
	And I select to create a new Skill Group in SGM
	And I name the Skill Group 'my Area'
	And I select the skill 'Skill A' in SGM
	And I save the Skill Groups
	And I select the Skill Group 'my Area'
	And I chose to rename the Skill Group 'my Area'
	When I name the Skill Group 'my Area 2'
	And I save the Skill Groups
	And I close the Skill Manager
	And I pick the skillgroup 'my Area 2'
	Then I should monitor 'my Area 2'

@OnlyRunIfEnabled('WFM_Unified_Skill_Group_Management_45417')
@OnlyRunIfEnabled('WFM_Modify_Skill_Groups_43727')
	Scenario: Add Skill to Skill Group in SGM
	Given there is a Skill Area called 'SkillArea A' that monitors skills 'Skill B'
	And I am viewing intraday page
	And I select to manage Skill Groups
	And I select the Skill Group 'SkillArea A'
	And I select 'Skill A' from available skill
	When I save the Skill Groups
	And I close the Skill Manager
	And I pick the skillgroup 'SkillArea A'
	Then I should see 'Skill A' as included skill

@OnlyRunIfEnabled('WFM_Unified_Skill_Group_Management_45417')
@OnlyRunIfEnabled('WFM_Modify_Skill_Groups_43727')
Scenario: Remove Skill from Skill Group in SGM
	Given there is a Skill Area called 'SkillArea A' that monitors skills 'Skill A, Skill B'
	And I am viewing intraday page
	And I select to manage Skill Groups
	And I select the Skill Group 'SkillArea A'
	And I select 'Skill A' from SkillsInThisGroup
	When I save the Skill Groups
	And I close the Skill Manager
	And I pick the skillgroup 'SkillArea A'
	Then I should monitor 'SkillArea A'
	And I should not see 'Skill A' as included skill

@OnlyRunIfDisabled('WFM_Modify_Skill_Groups_43727')
Scenario: Remove Skill Area
	Given there is a Skill Area called 'Area A' that monitors skill 'Skill A'
	And I am viewing intraday page
	When I select to remove 'Area A'
	Then I should no longer be able to monitor 'Area A'

Scenario: View incoming traffic for one skill
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-21 13:30'
	And there is forecast data for skill 'Skill A' for date '2016-12-21'
	When I am viewing intraday page
	Then I should see incoming traffic data in the chart
	And I should see a summary of incoming traffic
		
Scenario: View performance for a skill area
	Given the time is '2016-12-22 14:00'
	And there is a Skill Area called 'Area A' that monitors skill 'Skill A'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-22 13:30'
	And there is forecast data for skill 'Skill A' for date '2016-12-22'
	When I am viewing intraday page
	And I pick the skillgroup 'Area A'
	And I should see incoming traffic data in the chart
	And I am navigating to intraday performance view
	Then I should see performance data in the chart
	And I should see a summary of today's performance
	
Scenario: View staffing for one skill
	Given the time is '2016-12-22 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-22 13:30'
	And there is forecast data for skill 'Skill A' for date '2016-12-22'
	And there are scheduled agents for 'Skill A' for date '2016-12-22'
	When I am viewing intraday page
	And I should see incoming traffic data in the chart
	And I am navigating to intraday staffing view
	Then I should see staffing data in the chart

@OnlyRunIfEnabled('WFM_Intraday_Show_For_Other_Days_43504')
Scenario: View incoming traffic for one skill for a provided day
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-20 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-20'
	And I am viewing intraday page
	And There's no data available
	When I change date offset to '-1'
	Then I should see incoming traffic data in the chart
	And I should see a summary of incoming traffic
 
@OnlyRunIfEnabled('WFM_Intraday_Show_For_Other_Days_43504')
Scenario: Switch tab when other day than today is selected
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-22 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-22'
	And I am viewing intraday page
	When I change date offset to '1'
	And I am navigating to intraday staffing view
	Then I should see forecasted staffing data in the chart
	And I should see the date

@OnlyRunIfEnabled('WFM_Intraday_Show_For_Other_Days_43504')
Scenario: Switch skill when other day than today is selected
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill B' up until '2016-12-22 17:00'
	And there is forecast data for skill 'Skill B' for date '2016-12-22'
	And I am viewing intraday page
	And I pick the skill 'Skill A'
	When I change date offset to '1'
	And I pick the skill 'Skill B'
	Then I should see incoming traffic data in the chart
	And I should see the date

@OnlyRunIfEnabled('WFM_Intraday_Export_To_Excel_44892')
Scenario: If toggled we should see export to excel button
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-21 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-21'
	And I am viewing intraday page
	Then I should see the export to excel button


@OnlyRunIfEnabled('WFM_Intraday_SupportOtherSkillsLikeEmail_44026')
Scenario: If toggled we should be able to select skill of type backoffice
	Given the time is '2016-12-21 14:00'
	And I am viewing intraday page
	And there is queue statistics for the skill 'Skill BackOffice' up until '2016-12-21 17:00'
	And there is forecast data for skill 'Skill BackOffice' for date '2016-12-21'
	When I pick the skill 'Skill BackOffice'
	Then I should see incoming traffic data in the chart

@OnlyRunIfEnabled('WFM_Intraday_SupportOtherSkillsLikeEmail_44026')
Scenario: If an email like skill is chosen a warning for no visible abandonrate should appear
	Given the time is '2016-12-21 14:00'
	And I am viewing intraday page
	And there is queue statistics for the skill 'Skill BackOffice' up until '2016-12-21 17:00'
	And there is forecast data for skill 'Skill BackOffice' for date '2016-12-21'
	When I pick the skill 'Skill BackOffice'
	And I am navigating to intraday performance view
	Then I should see the no abandonrate warning

@OnlyRunIfEnabled('WFM_Intraday_SupportOtherSkillsLikeEmail_44026')
Scenario: If an email like skill is chosen a warning for no visible reforcasted should appear
	Given the time is '2016-12-21 14:00'
	And I am viewing intraday page
	And there is queue statistics for the skill 'Skill BackOffice' up until '2016-12-21 17:00'
	And there is forecast data for skill 'Skill BackOffice' for date '2016-12-21'
	When I pick the skill 'Skill BackOffice'
	And I am navigating to intraday staffing view
	Then I should see the no reforcasted warning

@OnlyRunIfEnabled('WFM_Intraday_SupportOtherSkillsLikeEmail_44026')
Scenario: If and email like skill is chosen summary for abandonrate should not appear
	Given the time is '2016-12-21 14:00'
	And I am viewing intraday page
	And there is queue statistics for the skill 'Skill BackOffice' up until '2016-12-21 17:00'
	And there is forecast data for skill 'Skill BackOffice' for date '2016-12-21'
	When I pick the skill 'Skill BackOffice'
	And I am navigating to intraday performance view
	Then I should not se summary for abandonrate

Scenario: Select skill when skill group is selected
	Given the time is '2016-12-22 14:00'
	And there is queue statistics for the skill 'Skill B' up until '2016-12-22 17:00'
	And there is forecast data for skill 'Skill B' for date '2016-12-22'
	And there is a Skill Area called 'SkillArea1' that monitors skills 'Skill A, Skill B'
	And I am viewing intraday page
	And I pick the skillgroup 'SkillArea1'
	When I select skill 'Skill B' from included skills in skill group
	Then I should see incoming traffic data in the chart
	And I Should see skill 'Skill B' as selected skill

Scenario: Return to skill group when viewing included skill
	Given the time is '2016-12-22 14:00'
	And there is queue statistics for the skill 'Skill B' up until '2016-12-22 17:00'
	And there is forecast data for skill 'Skill B' for date '2016-12-22'
	And there is a Skill Area called 'SkillArea1' that monitors skills 'Skill A, Skill B'
	And I am viewing intraday page
	And I pick the skillgroup 'SkillArea1'
	And I select skill 'Skill A' from included skills in skill group
	And There's no data available
	When I return to skill group from skill 'Skill A'
	Then I should see incoming traffic data in the chart
	And I should see a summary of incoming traffic
