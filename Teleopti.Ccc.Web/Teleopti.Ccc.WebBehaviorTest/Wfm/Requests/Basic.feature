Feature: Requests Basic Operations
	In order to approval/deny agent's requests
	As a resource planner
	I need to have a good overview of all the requests within a certain period

Background: 
	Given I have a role with
	| Field                  | Value            |
	| Name                   | Resource Planner |
	| Access to wfm requests | true             |
	And 'Ashley' has a person period with 
	| Field      | Value        |
	| Start date | 2015-06-18   |
	And 'John' has a person period with 
	| Field      | Value        |
	| Start date | 2015-06-18   |

@ignore
Scenario: Display requests
	Given 'Ashley' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	And 'John' has an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	When I view wfm requests
	Then I should see a text request from 'Ashley' in the list
	And I should see a absence request from 'John' in the list

@ignore
Scenario: Sort requests
	Given 'Ashley' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	| Update Time  | 2015-11-03 14:00 |
	And 'John' has an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	| Update Time  | 2015-11-01 14:00 |
	And I view wfm requests
	When I sort the request list by ascending update time
	Then I should see the request from 'John' before the request from 'Ashley' in the list

@ignore
Scenario: View request details
	Given 'Ashley' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	| Update Time  | 2015-11-03 14:00 |
	And 'John' has an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	| Update Time  | 2015-11-01 14:00 |
	And I view wfm requests
	When I expand the request from 'Ashley'
	Then I should see detailed text request from 'Ashley'

