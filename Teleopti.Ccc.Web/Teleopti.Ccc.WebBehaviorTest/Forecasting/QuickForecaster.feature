Feature: QuickForecaster
	In order to create a forecast
	As a user
	I want to be able to do a forecast
	
Scenario: Show message if no historical data for measurement
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is no queue statistics for 'Queue1'
	And there is a workload 'TheWorkload1' with skill 'TheSkill1' and queue 'Queue1'
	When I am viewing quick forecast page
	And I use default forecast period and continue
	Then I should see no accuracy for total
	And I should see no accuracy for skill 'TheSkill1'
	And I should see no accuracy for workload 'TheWorkload1'

Scenario: Show accuracy
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is queue statistics for 'Queue1'
	And there is a workload 'TheWorkload1' with skill 'TheSkill1' and queue 'Queue1'
	When I am viewing quick forecast page
	And I use default forecast period and continue
	Then I should see the total forecasting accuracy
	And I should see the forecasting accuracy for skill 'TheSkill1'
	And I should see the forecasting accuracy for workload 'TheWorkload1'

@Ignore
Scenario: Forecast one skill
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is queue statistics for 'Queue1'
	And there is a workload 'TheWorkload1' with skill 'TheSkill1' and queue 'Queue1'
	And there is a workload 'TheWorkload2' with skill 'TheSkill1' and queue 'Queue1'
	And there is no forecast data
	When I am viewing quick forecast page
	And I use default forecast period and continue
	And I select skill 'TheSkill1'
	And I choose to forecast the selected targets
	And Forecast has succeeded
	Then there is forecast data for default period for 'TheWorkload1'
	And there is forecast data for default period for 'TheWorkload2'
@Ignore
Scenario: Forecast one workload
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is queue statistics for 'Queue1'
	And there is a workload 'TheWorkload1' with skill 'TheSkill1' and queue 'Queue1'
	And there is a workload 'TheWorkload2' with skill 'TheSkill1' and queue 'Queue1'
	And there is no forecast data
	When I am viewing quick forecast page
	And I use default forecast period and continue
	And I select workload 'TheWorkload2'
	And I choose to forecast the selected targets
	And Forecast has succeeded
	Then there is forecast data for default period for 'TheWorkload2'
	And there is no forecast data for default period for 'TheWorkload1'