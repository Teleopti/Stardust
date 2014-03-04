Feature: Reports
	In order to review my reports made
	As an agent
	I want to be able to view my reports

@ingore
Scenario: Show reports with permissions
	Given  I am an Agent
	And I have the permission for report 'Requests per Agent'
	When I click reports menu
	Then I should only see report 'Requests per Agent' 
	And I should not see any other reports

@ingore
Scenario: My Report should display at top of the drop list
	Given  I am an Agent
	And I have the permission for 'My Report'
	And I have the permission for report 'Requests per Agent'
	When I click reports menu
	Then I should see My Report display at top of the drop list 

@ingore
Scenario: Reports should be sorted alphabetically 
	Given  I am an Agent
	And I have the permission for report 'Requests per Agent'
	And I have the permission for report 'Agent Skills'
	And I have the permission for report 'Absence Time per Agent' 
	When I click reports menu
	Then I should see report 'Absence Time per Agent' display at top of the drop list 
	And I should see report 'Agent Skills' display as the second item of the drop list 
	And I should see report 'Requests per Agent' display at botom of the drop list 

@ingore
Scenario: Should not show the reports menu with no permission for any report
	Given  I am an Agent
	And I have no permission for any report 
	When I logon MyTime Web
	Then I should not see the reports menu

@ingore
Scenario: Should open report with permission
	Given  I am an Agent
	And I have permission for report 'Requests per Agent'
	When I click 'Requests per Agent' in the drop list
	Then It should open report 'Requests per Agent' in a new window

@ingore
Scenario: Should open My Report with permission
	Given  I am an Agent
	And I have permission for 'My Report'
	When I click 'My Report' in the drop list
	Then It should open My Report in the current page
	And I should see the date picker in the page
	