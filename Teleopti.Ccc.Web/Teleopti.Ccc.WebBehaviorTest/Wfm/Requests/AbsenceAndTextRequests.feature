@WFM
Feature: absence and text Requests
	In order to approval/deny agent's absence and text requests
	As a resource planner
	I need to have a good overview of all the absence and text requests within a certain period

Background:
	Given I am englishspeaking swede
	And there is a team with
	| Field | Value    |
	| Name  | Red Team |
	And there is a team with
	| Field | Value      |
	| Name  | Green Team |
	And I have a role with
	| Field                  | Value            |
	| Name                   | Resource Planner |
	| Access to wfm requests | true             |
	| Access to everyone     | true             |
	And 'Ashley Andeen' has a person period with 
	| Field      | Value      |
	| Start date | 2015-06-18 |
	| Team       | Red Team   |
	And 'Ashley Andeen' has '22' text requests with
	| Field        | Value                                 |
	| StartTime    | 2015-10-03 10:00                      |
	| End Time     | 2015-10-03 14:00                      |
	| Update Time  | 2015-09-03 14:00                      |
	| Absence Type | Illness                               |
	| Status       | Pending                               |
	| Subject      | Subject - Illness request from Ashley |
	| Message      | Message - Illness request from Ashley |

@OnlyRunIfEnabled('Wfm_Requests_Refactoring_45470')
Scenario: Should view absence and text requests 
	When I view wfm requests
	And I select to go to absence and text requests view
	And I select date range from '2015-10-01' to '2015-10-20'
	And I select all the team
	And I click button for search requests
	Then I should see a absence and text request from 'Ashley Andeen' in the list

@OnlyRunIfEnabled('Wfm_Requests_Refactoring_45470')
Scenario: Should keep selected requests when navigating to different page
	When I view wfm requests
	And I select to go to absence and text requests view
	And I select date range from '2015-10-01' to '2015-10-20'
	And I select all the team
	And I click button for search requests
	And I select all requests in the first page
	And I change to the second page
	And I change to the first page
	Then I should see all requests should be selected