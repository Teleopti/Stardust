Feature: Reports
	In order to review my reports made
	As an agent
	I want to be able to view my reports

Background: 
Given there is a role with
	| Field                    | Value                       |
	| Name                     | No access to Matrix reports |
	| Access To Matrix Reports | False                       |

Scenario: Show reports with permissions
	When I am viewing an application page
	And I click reports menu
	Then I should see the dropdown report list 

Scenario: Should not show the reports menu with no permission for any report
	Given I have the role 'AgentWithoutAnyReport'
	When I am viewing an application page
	Then I should not see any report menu

Scenario: Show MyReport menu
	Given I have the role 'No access to Matrix reports'
	When I am viewing an application page
	Then MyReport tab should be visible 

#there are more than 3 normal reports by default when setup
#position 1 is MyReport, position 2 is divider by default
Scenario: Open standard report 
	When I am viewing an application page
	And I click reports menu
	And I click the report at position '3' in the list
	Then The report should not be opened in the same window
	
@ignore
Scenario: Open MyReport 
	Given  I have permission to MyReport
	When I click 'MyReport'
	Then I should see my report displayed in the same window