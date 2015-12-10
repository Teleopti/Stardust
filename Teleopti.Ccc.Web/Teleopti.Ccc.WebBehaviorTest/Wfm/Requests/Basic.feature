@OnlyRunIfEnabled('Wfm_Requests_Basic_35986')
Feature: Requests Basic Operations
	In order to approval/deny agent's requests
	As a resource planner
	I need to have a good overview of all the requests within a certain period

Background: 
	Given I have a role with
	| Field                  | Value            |
	| Name                   | Resource Planner |
	| Access to wfm requests | true             |
	| Access to everyone     | true             |
	And 'Ashley' has a person period with 
	| Field      | Value        |
	| Start date | 2015-06-18   |
	And 'John' has a person period with 
	| Field      | Value      |
	| Start date | 2015-06-18 |

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
	And I select to loading requests from '2015-10-01' to '2015-10-04'
	Then I should see a request from 'Ashley' in the list
	And I should see a request from 'John' in the list

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
	When I view wfm requests
	And I select to loading requests from '2015-10-01' to '2015-10-04'
	And I sort the request list by descending agent name
	Then I should see the request from 'John' before the request from 'Ashley' in the list

@ignore
@OnlyRunIfEnabled('Wfm_Requests_People_Search_36294')
Scenario: Find requests for filtered agents
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
	When I view wfm requests
	And I select to loading requests from '2015-10-01' to '2015-10-04'
	And I pick the team "Green Team"
	Then I should see a request from 'Ashley' in the list
	And I should not see any request from 'John' in the list



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

