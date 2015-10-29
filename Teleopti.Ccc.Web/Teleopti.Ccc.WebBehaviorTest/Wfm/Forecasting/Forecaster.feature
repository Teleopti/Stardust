Feature: Forecaster
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
	When I am viewing forecast page
	And I use default forecast period and forecast for all
	And Forecast has succeeded
	Then there is forecast data for default period for 'TheWorkload1'
	And there is forecast data for default period for 'TheWorkload2'

Scenario: Forecast one workload
	Given there is a workload 'TheWorkload2' with skill 'TheSkill1' and queue 'Queue1'
	And there is no forecast data
	When I am viewing forecast page
	And I select workload 'TheWorkload2'
	And I use default forecast period and forecast for one workload
	And Forecast has succeeded
	Then there is forecast data for default period for 'TheWorkload2'
	And there is no forecast data for default period for 'TheWorkload1'

Scenario: Add campaign
	Given I am viewing forecast page
	And I select workload 'TheWorkload1'
	And I use default forecast period and forecast for one workload
	And Forecast has succeeded
	When I select the first day in the forecast chart
	And I select to modify the forecast
	And I choose to add a campaign
	And I increase the calls by 100 percent
	And I apply the campaign
	Then I should see that the total calls for the first day has doubled

Scenario: Keep campaigns when reforecasting
	Given I am viewing forecast page
	And I select workload 'TheWorkload1'
	And I use default forecast period and forecast for one workload
	And Forecast has succeeded
	When I select the first day in the forecast chart
	And I select to modify the forecast
	And I choose to add a campaign
	And I increase the calls by 100 percent
	And I apply the campaign
	And forecast result has loaded
	Then I use default forecast period and forecast for one workload
	And Forecast has succeeded
	And forecast result has loaded
	And I should see that the total calls for the first day has doubled

Scenario: Save forecast to scenario
	Given there is a scenario
	| Field         | Value        |
	| Name          | Scenario 1   |
	| Business Unit | BusinessUnit |
	And there is a workload 'TheWorkload2' with skill 'TheSkill1' and queue 'Queue1'
	And there is no forecast data
	When I am viewing forecast page
	And I select workload 'TheWorkload2'
	And I choose scenario 'Scenario 1'
	And I use default forecast period and forecast for one workload
	And Forecast has succeeded
	Then there is forecast data for default period for
	| Field    | Value        |
	| Workload | TheWorkload2 |
	| Scenario | Scenario 1   |

@OnlyRunIfEnabled('WfmForecast_CreateSkill_34591')
Scenario: Create new skill
	Given I am viewing forecast page
	When I choose to add a new skill
	And I input the new skill with
	| Field               | Value                                                        |
	| Name                | NewSkill1                                                    |
	| Activity            | TheActivity1                                                 |
	| Timezone            | (UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna |
	| Queues              | Queue1                                                       |
	| ServiceLevelPercent | 50                                                           |
	| ServiceLevelSeconds | 20                                                           |
	| Shrinkage           | 40                                                           |
	And I input opening hours with
	| Field     | Value      |
	| Monday    | 9:00-17:00 |
	| Tuesday   | 9:00-17:00 |
	| Wednesday | 9:00-17:00 |
	| Thursday  | 9:00-17:00 |
	| Friday    | 9:00-17:00 |
	| Saturday  | 9:00-17:00 |
	| Sunday    | 9:00-17:00 |
	And I save the new skill
	Then I should see the new skill 'NewSkill1' in the list

@Ignore
Scenario: Change the forecasted calls for one day
	Given I am viewing forecast page
	And I select workload 'TheWorkload1'
	And I use default forecast period and forecast for one workload
	And Forecast has succeeded
	When I select the first day in the forecast chart
	And I select to modify the forecast
	And I select to do a manual change
	And I enter '500' calls per day
	And I apply the manual change
	Then I should see that the total calls for the first day is '500'