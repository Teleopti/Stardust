@WFM
Feature: overtime Requests
	In order to approval/deny agent's overtime requests
	As a resource planner
	I need to have a good overview of all the overtime requests within a certain period

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
	And there is an activity named 'Phone'
	And there is a skill named 'Phone' with activity 'Phone'
	And 'Ashley Andeen' has a person period with 
	| Field      | Value      |
	| Start date | 2018-02-01 |
	| Team       | Red Team   |
	| Skill      | Phone      |
	And 'Ashley Andeen' has a workflow control set with overtime request open periods
	And 'Ashley Andeen' has an overtime request with
	| Field     | Value                                          |
	| StartTime | 2018-02-03 10:00                               |
	| End Time  | 2018-02-03 14:00                               |
	| Status    | Pending                                        |
	| Subject   | Subject - Overtime request subject from Ashley |
	| Messsage  | Message - Overtime request message from Ashley |

Scenario: Should view overtime requests 
	Given today is '2018-02-03'
	When I select to go to overtime view
	And I select date range from '2018-02-01' to '2018-02-06' after '1000' milliseconds
	And I select all the team
	And I click button for search requests
	Then I should see a overtime request from 'Ashley Andeen' in the list

Scenario: Should approve overtime request
	Given today is '2018-02-03'
	When I select to go to overtime view
	And I select date range from '2018-02-01' to '2018-02-06' after '1000' milliseconds
	And I select all the team
	And I click button for search requests
	Then I should see a overtime request from 'Ashley Andeen' in the list
	And I approve all requests that I see
	Then I should see a success message

Scenario: Should deny overtime request
	Given today is '2018-02-03'
	When I select to go to overtime view
	And I select date range from '2018-02-01' to '2018-02-06' after '1000' milliseconds
	And I select all the team
	And I click button for search requests
	Then I should see a overtime request from 'Ashley Andeen' in the list
	And I deny all requests that I see
	Then I should see a success message