Feature: My Report Daily Metrics
	In order to improve my own performance
	As an agent
	I need to see metrics for a specific day
	
Scenario: My report tab
	Given I am an agent
	When I am viewing an application page
	Then MyReport tab should be visible 

Scenario: No permission to my report module
	Given there is a role with
	| Field						| Value						|
	| Name						| No access to MyReport		|
	| Access to MyReport		| False					    |
	And I have the role 'No access to MyReport'
	When I am viewing preferences
	Then MyReport tab should not be visible 

Scenario: No permission to my report module when navigating with URL
	Given there is a role with
	| Field						| Value						|
	| Name						| No access to MyReport		|
	| Access to MyReport		| False					    |
	And I have the role 'No access to MyReport'
	When I navigate to my report
	Then I should see a message saying I dont have access to MyReport

Scenario: Open my report shows yesterdays figures
	Given I am an agent
	And I have my report data for '2013-10-02'
	And the current time is '2013-10-03'
	When I navigate to my report
	Then I should see my report with data for '2013-10-02'
	
Scenario: Show friendly message when no report data
	Given I am an agent
	And I do not have any report data for date '2013-10-02'
	And the current time is '2013-10-03'
	When I navigate to my report
	Then I should see a user-friendly message explaining I dont have anything to view
	
@ignore
Scenario: Select date in date-picker
	Given I am an agent
	When I navigate to my report
	And I open the date-picker
	And I click on any date
	Then the date-picker should close
	And I should see the selected date
	
@ignore
Scenario: Navigate within my report view to previous day
	Given I am an agent
	And I have my report data for '2013-10-02'
	And the current time is '2013-10-04'
	When I navigate to my report
	And I click previous date
	Then I should see my report with data for '2013-10-02'
	
@ignore
Scenario: Navigate within my report view to next day
	Given I am an agent
	And I have my report data for '2013-10-02'
	And the current time is '2013-10-04'
	When I navigate to my report
	And I select date '2013-10-01'
	And I click next date
	Then I should see my report with data for '2013-10-02'
		
@ignore
Scenario: Do not navigate to today
	Given I am an agent
	And I have my report data for '2013-10-02'
	And the current time is '2013-10-03'
	When I navigate to my report
	Then I should not be able to click next date









