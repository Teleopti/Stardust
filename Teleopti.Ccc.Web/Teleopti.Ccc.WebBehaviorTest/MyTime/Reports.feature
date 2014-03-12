Feature: Reports
	In order to review my reports made
	As an agent
	I want to be able to view my reports

Background: 
	Given there is a role with
	| Field                       | Value                       |
	| Name                        | Access to Request per Agent |
	| Access to Request per Agent | True                        |
	And there is a role with
	| Field                  | Value                  |
	| Name                   | Access to Agent Skills |
	| Access to Agent Skills | True                   |
	And there is a role with
	| Field                            | Value                            |
	| Name                             | Access to Absence Time per Agent |
	| Access to Absence Time per Agent | True                             |
	And there is a role with
	| Field              | Value                 |
	| Name               | No access to MyReport |
	| Access to MyReport | False                 |

@ignore
Scenario: Show reports with permissions
	Given I have the role 'Access to Request per Agent'
	And I have the role 'No access to MyReport'
	When I click reports menu
	Then I should only see report 'Requests per Agent' 
	And I should not see any other reports

@ignore
Scenario: My Report should display at top of the drop list
	Given  I am an agent
	And I have the role 'Access to Request per Agent'
	When I click reports menu
	Then I should see My Report display at top of the drop list 

@ignore
Scenario: Reports should be sorted alphabetically 
	Given  I have the role 'Access to Request per Agent'
	And I have the role 'Access to Absence Time per Agent'
	And I have the role 'Access to Agent Skills'
	And I have the role 'No access to MyReport'
	When I click reports menu
	Then I should see report 'Absence Time per Agent' display at top of the drop list 
	And I should see report 'Agent Skills' display as the second item of the drop list 
	And I should see report 'Requests per Agent' display at botom of the drop list 

@ignore
Scenario: Should not show the reports menu with no permission for any report
	Given  I have the role 'No access to MyReport' 
	When I logon MyTime Web
	Then I should not see the reports menu

@ignore
Scenario: Open standard report 
	Given  I have the role 'Access to Request per Agent'
	When I click 'Requests per Agent' in the list
	Then It should open report 'Requests per Agent' in a new window

@ignore
Scenario: Open My Report
	Given  I am an agent
	And I have my report data for '2013-10-02'
	And the current time is '2013-10-03'
	When I click 'My Report' in the list
	Then I should see my report with data for '2013-10-02' in the current page