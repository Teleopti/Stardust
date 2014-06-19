Feature: Performance view
	In order to evaluate performance of the backend processing
	As a developer
	I want to test, measure and view timings of scenarios
	
Background:
	Given I have a role with
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
Scenario: Measure real time adherence by sending in external user state
	Given the current time is '2014-06-19 12:00'
	And there is a datasouce with id 6
	And there is an activity named 'Phone'
	And there is a site named 'Paris'
	And there is a team named 'ParisTeam' on site 'Paris'
	And there is an external logon named 'Pierre Baldi' with datasource 6
	And Pierre Baldi has a person period with
	| Field          | Value        |
	| Team           | ParisTeam    |
	| Start Date     | 2014-01-01   |
	| External Logon | Pierre Baldi |
	And Pierre Baldi has a shift with
	| Field      | Value            |
	| Start time | 2014-06-19 08:00 |
	| End time   | 2014-06-19 17:00 |
	| Activity   | Phone            |
	And there is an alarm with 
	| Field           | Value        |
	| Activity        | Phone        |
	| Phone state     | Pause        |
	| Name            | Not adhering |
	| Staffing effect | -1           |
	When I am viewing the performance view
	And I select scenario 'Rta Load Test'
	And I input an RTA configuration scenario for 'Pierre Baldi' in json format on datasource 6
	And I click 'run'
	Then I should see that the test run has finished
	And I should see a count of 1 messages received for 'Real Time Adherence Load Test'
	And I should see total run time
	And I should see total time to send commands
	And I should see scenarios per second


