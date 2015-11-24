Feature: Performance view
	In order to evaluate performance of the backend processing
	As a developer
	I want to test, measure and view timings of scenarios
	
Background:
	Given there is a switch
	And I have a role with
	| Field | Value     |
	| Name  | Developer |
	And I have a person period with
    | Field      | Value      |	
	| Start date | 2013-06-01 |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |

#Only for spec, not for testing
@ignore
Scenario: Default configuration for scenario add full day absences
	When I am viewing the performance view
	And I select scenario 'Add full day absence -> PersonScheduleDayReadModel'
	Then I should see a default configuration in json format

Scenario: Measure PersonScheduleDayReadModel by adding full day absences
	When I am viewing the performance view
	And I select scenario 'Add full day absence -> PersonScheduleDayReadModel'
	And I input a configuration with 1 scenarios in json format
	And I click 'run'
	Then I should see that the test run has finished
	And I should see a count of 1 messages received for 'PersonScheduleDayReadModel'
	And I should see total run time
	And I should see total time to send commands
	And I should see scenarios per second

# Bug 25359 re-enable with PBI 25562
@ignore
Scenario: Measure ScheduledResourcesReadModel by adding full day absences
	When I am viewing the performance view
	And I select scenario 'Add full day absence -> ScheduledResourcesReadModel'
	And I input a configuration with 1 scenarios in json format
	And I click 'run'
	Then I should see that the test run has finished
	And I should see a count of 1 messages received for 'ScheduledResourcesReadModel'
	And I should see total run time
	And I should see total time to send commands
	And I should see scenarios per second

@ignore
Scenario: Measure manage adherence by rta states
	Given the time is '2015-01-14 12:00'
	And there is a site named 'Paris'
	And there is a team named 'Team1' on site 'Paris'
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | Team1    |
	| Start Date     | 2014-01-01   |
	And there are 1 rta state codes and state code groups
	When I am viewing the performance view for 'Manage Adherence Load Test'
	And I input a configuration for Pierre Baldi of 'Team1' with 1 states and 1 poll per second on datasource 6
	And I click 'run'
	Then I should see that the test run has finished
	And I should see total run time
	And I should see total time to send commands
	And I should see scenarios per second