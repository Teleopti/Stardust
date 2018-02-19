@WFM
Feature: IntradaySaveState
As a intraday monitoring person I would like that the selected skill / skill group and tab (Incoming / Performance / Staffing), as well as Selected skill in skill group, and selected day (-7 - +1), is remembered when I come back to Intraday
so I can continue analysing the same selection after visiting another module
so I can get my last used selection when logging in to the WFM application
As a intraday monitoring person I would like have different tabs open in my browser, with different selection for the Intraday, so I can keep track of different skills / skill group, without having to make a new selection. 

Background:
    Given I have a role with
    | Field              | Value |
    | Access to Intraday | True  |
    And There is a skill to monitor called 'Skill A' with queue id '9' and queue name 'queue1' and activity 'activity1'
    And There is a skill to monitor called 'Skill B' with queue id '7' and queue name 'queue2' and activity 'activity2'
	And There is an email-like skill to monitor called 'Skill BackOffice' with queue id '3' and queue name 'queue3' and activity 'activity3'

@OnlyRunIfEnabled('WFM_Remember_My_Selection_In_Intraday_47254')
Scenario: Remember my selection in intraday when switching module 47254 Test 3
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-21 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-21'
	And I am viewing intraday page
	When I am navigating to intraday staffing view
	And I should see forecasted staffing data in the chart
	And I am viewing staffing page
	And I am viewing intraday page
	Then I should see forecasted staffing data in the chart

@OnlyRunIfEnabled('WFM_Remember_My_Selection_In_Intraday_47254')
@OnlyRunIfEnabled('WFM_Unified_Skill_Group_Management_45417')
Scenario: Remember my selection in intraday when coming back from SGM 47254 Test 5
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-21 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-21'
	And I am viewing intraday page

	When I am navigating to intraday staffing view
	And I should see forecasted staffing data in the chart
	And I select to manage Skill Groups
	And I select to create a new Skill Group in SGM
	And I name the Skill Group 'my Area'
	And I select the skill 'Skill A' in SGM
	And I save the Skill Groups
	And I close the Skill Manager

	Then I should see forecasted staffing data in the chart

@OnlyRunIfEnabled('WFM_Remember_My_Selection_In_Intraday_47254')
@OnlyRunIfEnabled('WFM_Intraday_Show_For_Other_Days_43504')
Scenario: Remember my selection of day in intraday when coming back from SGM
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-20 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-20'
	And I am viewing intraday page
	And There's no data available
	And I change date offset to '-1'
	And I should see incoming traffic data in the chart

	When I select to manage Skill Groups
	And I close the Skill Manager
	
	Then I should see the offset is set to '-1'