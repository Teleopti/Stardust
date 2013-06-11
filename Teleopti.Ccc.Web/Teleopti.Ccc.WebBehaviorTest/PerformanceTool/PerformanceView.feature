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
Scenario: Default configuration for scenario add and remove full day absences
	When I am viewing the performance view
	And I select scenario 'Add and remove full day absence'
	Then I should see a default configuration in json format

Scenario: Measure add and remove full day absences
	When I am viewing the performance view
	And I select scenario 'Add and remove full day absence'
	And I input a configuration in json format
	And I click 'run'
	Then I should see that the test run has finished
	And I should see a count of messages received for each applicable model updated
	And I should see total run time
	And I should see total time to send commands
	And I should see scenarios per second
