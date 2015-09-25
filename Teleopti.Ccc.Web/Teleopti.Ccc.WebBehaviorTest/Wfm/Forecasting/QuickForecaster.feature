Feature: QuickForecaster
	In order to create a forecast
	As a user
	I want to be able to do a forecast

Background: 
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is queue statistics for 'Queue1'
	And there is a workload 'TheWorkload1' with skill 'TheSkill1' and queue 'Queue1'

Scenario: Forecast all
	Given there is a workload 'TheWorkload2' with skill 'TheSkill1' and queue 'Queue1'
	And there is no forecast data
	When I am viewing quick forecast page
	And I use default forecast period and forecast for all
	And Forecast has succeeded
	Then there is forecast data for default period for 'TheWorkload1'
	And there is forecast data for default period for 'TheWorkload2'

@ignore
Scenario: Forecast one workload
	Given there is a workload 'TheWorkload2' with skill 'TheSkill1' and queue 'Queue1'
	And there is no forecast data
	When I am viewing quick forecast page
	And I select workload 'TheWorkload2'
	And I use default forecast period and forecast for one workload
	And Forecast has succeeded
	Then there is forecast data for default period for 'TheWorkload2'
	And there is no forecast data for default period for 'TheWorkload1'

@ignore
Scenario: Add campaign
	Given I am viewing quick forecast page
	And I select workload 'TheWorkload1'
	And I use default forecast period and forecast for one workload
	And Forecast has succeeded
	When I select the first day in the forecast chart
	And I choose to add a campaign
	And I increase the calls by 100 percent
	And I apply the campaign
	Then I should see that the total calls for the first day has doubled

@ignore
Scenario: Keep campaigns when reforecasting
	Given I am viewing quick forecast page
	And I select workload 'TheWorkload1'
	And I use default forecast period and forecast for one workload
	And Forecast has succeeded
	When I select the first day in the forecast chart
	And I choose to add a campaign
	And I increase the calls by 100 percent
	And I apply the campaign
	Then I use default forecast period and forecast for one workload
	And forecast result has loaded
	And I should see that the total calls for the first day has doubled


