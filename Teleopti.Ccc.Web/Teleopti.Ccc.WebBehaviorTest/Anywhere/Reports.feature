Feature: Reports
	In order to know the reports for my team
	As a team leader
	I want to see the reports for the team
	
Background:
	Given there is a site named 'The site'
	And there is a team named 'Team green' on site 'The site'
	And there is a role with
	| Field                    | Value               |
	| Name                     | Anywhere Team Green |
	| Access to team           | Team green          |
	| Access to Anywhere       | true                |
	| Access To Matrix Reports | true                |

	 
Scenario: open report new
	Given I have the role 'Anywhere Team Green'
	When I view schedules for '2014-11-06'
	And I click reports menu
	And I click one report in drop down list
	Then The report should be opened in another tab