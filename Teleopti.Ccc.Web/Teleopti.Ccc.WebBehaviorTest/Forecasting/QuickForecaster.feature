Feature: QuickForecaster
	In order to create a forecast
	As a user
	I want to be able to do a forecast

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
	And I click Quickforecaster
	And Quickforecast has succeeded
	Then there are SkillDays for default period

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
	And I click Quickforecaster
	And Quickforecast has succeeded
	Then I should see the accuracy for the forecast method

Scenario: Show message if no historical data for measurement
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity'
	And there is a skill named 'TheSkill' with activity 'TheActivity'
	And there is a QueueSource named 'TheQueueSource'
	And there is a Workload 'TheWorkload' with Skill 'TheSkill' and queuesource 'TheQueueSource'
	And there is no SkillDays in the database
	When I am viewing quick forecast page
	And I click Quickforecaster
	And Quickforecast has succeeded
	Then I should see a message of no historical data for measurement

@Ignore
Scenario: Forecast for one workload
	Given I have a role with
	| Field           | Value      |
	| QuickForecaster | True       |
	And there is an activity named 'TheActivity1'
	And there is an activity named 'TheActivity2'
	And there is a skill named 'TheSkill1' with activity 'TheActivity1'
	And there is a skill named 'TheSkill2' with activity 'TheActivity2'
	And there is queue statistics for 'Queue1'
	And there is a Workload 'TheWorkload1' with Skill 'TheSkill1' and queuesource 'Queue1'
	And there is a Workload 'TheWorkload2' with Skill 'TheSkill1' and queuesource 'Queue1'
	And there is a Workload 'TheWorkload3' with Skill 'TheSkill2' and queuesource 'Queue1'
	And there is no SkillDays in the database
	When I am viewing quick forecast page
	And I select skill 'TheSkill1'
	And I select workload 'TheWorkload2'
	And I click Quickforecaster
	And Quickforecast has succeeded
	Then I should see the forecasting accuracy for 'TheWorkload2'
