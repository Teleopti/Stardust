@WFM
Feature: Intraday
    In order to be in control of my part of the business
    As an intraday analyst
    I want to be able to monitor my part of the business

Background:
    Given I have a role with
    | Field              | Value |
    | Access to Intraday | True  |
    And There is a skill to monitor called 'Skill A'

Scenario: Create Skill Area
  Given I am viewing intraday page
  And I select to create a new Skill Area
  And I name the Skill Area 'my Area'
  And I select the skill 'Skill A'
  When I am done creating Skill Area 
	Then I select to monitor skill area 'my Area'
  And I should monitor 'my Area'


Scenario: Remove Skill Area
	Given there is a Skill Area called 'Area A' that monitors skill 'Skill A'
  And I am viewing intraday page
  When I select to remove 'Area A'
	Then I should no longer be able to monitor 'Area A'

Scenario: View incoming traffic for one skill
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-21 13:30'
	And there is forecast data for skill 'Skill A' for date '2016-12-21'
	And I should see a summary of incoming traffic
	When I am viewing intraday page
	Then I should see incoming traffic data in the chart
	And I should see a summary of incoming traffic
		
Scenario: View performance for a skill area
	Given the time is '2016-12-22 14:00'
	And there is a Skill Area called 'Area A' that monitors skill 'Skill A'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-22 13:30'
	And there is forecast data for skill 'Skill A' for date '2016-12-22'
	When I am viewing intraday page
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

Scenario: View incoming traffic for one skill for a provided day
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-20 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-20'
	And I am viewing intraday page
	And There's no data available
	When I choose to look at statistics for 'yesterday'
	Then I should see incoming traffic data in the chart
	And I should see a summary of incoming traffic

Scenario: Switch tab when other day than today is selected
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-22 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-22'
	And I am viewing intraday page
	When I choose to look at statistics for 'tomorrow'
	And I am navigating to intraday staffing view
	Then I should see staffing data in the chart
	And I should see that the date is '2016-12-22'

Scenario: Switch skill when other day than today is selected
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-22 17:00'
	And there is queue statistics for the skill 'Skill B' up until '2016-12-22 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-22'
	And there is forecast data for skill 'Skill B' for date '2016-12-22'
	And I am viewing intraday page
	And I select the skill 'Skill A'
	When I choose to look at statistics for 'tommorow'
	And I select the skill 'Skill B'
	Then I should see incoming traffic data in the chart
	And I should see that the date is '2016-12-22'

Scenario: View incoming traffic for one skill for a provided date
	Given the time is '2016-12-21 14:00'
	And there is queue statistics for the skill 'Skill A' up until '2016-12-20 17:00'
	And there is forecast data for skill 'Skill A' for date '2016-12-20'
	When I am viewing intraday page
	And I should not see incoming traffic data in the chart
	And I change date offset to '-1'
	Then I should see incoming traffic data in the chart