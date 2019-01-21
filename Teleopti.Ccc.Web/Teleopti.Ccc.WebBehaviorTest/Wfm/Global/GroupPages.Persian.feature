@WFM

Feature: GroupPages_Persian
	As a team leader
	I should be able to search agents using group pages picker in Teams

Background: 
	Given I am persian
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And there is an activity named 'MyActivity'
	And there is a skill named 'A skill' with activity 'MyActivity'
	And I have a person period with
	| Field      | Value      |
	| Start date | 2016-01-01 |
	| Team       | Team green |
	| Skill      | A skill    |
	And I have a role with
	| Field                          | Value          |
	| Name                           | Wfm Team Green |
	| Access to view all group pages | true           |
	| Access to everyone             | True           |
	| Access to Wfm MyTeam Schedule  | true           |


Scenario: search agents using group pages picker in teams
	Given today is '2016-10-10'
	When I view wfm team schedules
	And I open group pages picker
	Then I should see group pages picker tab
	And I click on group page picker icon
	And I select all skills on group page picker
	And I close group pages picker
	And I click button to search for schedules
	Then I should see agent 'I' in the table