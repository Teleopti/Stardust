Feature: QuickForecaster
	In order to create a forecast
	As a user
	I want to be able to do a forecast


@Ignore
Scenario: Simple forecast
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity'
	And there is a skill named 'TheSkill' with activity 'TheActivity'
	And there is a QueueSource named 'TheQueueSource'
	And there is a Workload 'TheWorkload' with Skill 'TheSkill' and queuesource 'TheQueueSource'
	And there is no SkillDays in the database
	When I am viewing quick forecast page
	And I use default forecast period and continue
	And Quickforecast has succeeded
	Then there are SkillDays for default period


@Ignore
Scenario: Show accuracy for forecast method
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is queue statistics for 'Queue1'
	And there is an activity named 'TheActivity'
	And there is a skill named 'TheSkill' with activity 'TheActivity'
	And there is a Workload 'TheWorkload' with Skill 'TheSkill' and queuesource 'Queue1'
	And there is no SkillDays in the database
	When I am viewing quick forecast page
	And I use default forecast period and continue
	And Quickforecast has succeeded
	Then I should see the accuracy for the forecast method

@Ignore
Scenario: Show message if no historical data for forecasting

@Ignore
Scenario: Show message if no historical data for measurement
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity'
	And there is a skill named 'TheSkill' with activity 'TheActivity'
	And there is a QueueSource named 'TheQueueSource'
	And there is a Workload 'TheWorkload' with Skill 'TheSkill' and queuesource 'TheQueueSource'
	And there is no forecast data
	When I am viewing quick forecast page
	And I use default forecast period and continue
	And Quickforecast has succeeded
	Then I should see a message of no historical data for measurement

Scenario: Show accuracy
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is queue statistics for 'Queue1'
	And there is a Workload 'TheWorkload1' with Skill 'TheSkill1' and queuesource 'Queue1'
	When I am viewing quick forecast page
	And I use default forecast period and continue
	Then I should see the total forecasting accuracy
	And I should see the forecasting accuracy for skill 'TheSkill1'
	And I should see the forecasting accuracy for workload 'TheWorkload1'

Scenario: Forecast one skill
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is queue statistics for 'Queue1'
	And there is a Workload 'TheWorkload1' with Skill 'TheSkill1' and queuesource 'Queue1'
	And there is a Workload 'TheWorkload2' with Skill 'TheSkill1' and queuesource 'Queue1'
	And there is no forecast data
	When I am viewing quick forecast page
	And I use default forecast period and continue
	And I select skill 'TheSkill1'
	And I choose to forecast the selected targets
	Then there is forecast data for default period for 'TheWorkload1'
	And there is forecast data for default period for 'TheWorkload2'

Scenario: Forecast one workload
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is queue statistics for 'Queue1'
	And there is a Workload 'TheWorkload1' with Skill 'TheSkill1' and queuesource 'Queue1'
	And there is a Workload 'TheWorkload2' with Skill 'TheSkill1' and queuesource 'Queue1'
	And there is no forecast data
	When I am viewing quick forecast page
	And I use default forecast period and continue
	And I select workload 'TheWorkload2'
	And I choose to forecast the selected targets
	Then there is forecast data for default period for 'TheWorkload2'
	And there is no forecast data for default period for 'TheWorkload1'