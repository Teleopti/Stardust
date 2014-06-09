Feature: My Report Daily Metrics
	In order to improve my own performance
	As an agent
	I need to see metrics for a specific day

Scenario: No permission to my report module when navigating with URL
	Given there is a role with
	| Field						| Value						|
	| Name						| No access to MyReport		|
	| Access to MyReport		| False					    |
	And I have the role 'No access to MyReport'
	When I navigate to my report
	Then I should see a message saying I dont have access

Scenario: Open my report shows yesterdays figures
	Given I am an agent
	And I have my report data for '2013-10-02'
	And the current time is '2013-10-03'
	When I navigate to my report
	Then I should see my report with data for '2013-10-02'
	
Scenario: Show friendly message when no report data
	Given I am an agent
	And I do not have any report data for date '2013-10-02'
	When I navigate to my report for '2013-10-02'
	Then I should see a user-friendly message explaining I dont have anything to view
	
Scenario: Navigate within my report view to previous day
	Given I am an agent
	And I view my report for '2013-10-04'
	When I navigate to the previous day
	Then I should end up in my report for '2013-10-03'
	
Scenario: Navigate within my report view to next day
	Given I am an agent
	And I view my report for '2013-10-04'
	When I navigate to the next day
	Then I should end up in my report for '2013-10-05'

Scenario: Navigate within my report using date picker
	Given I am an agent
	And I view my report for '2013-10-04'
	When I select the date '2013-10-10'
	Then I should end up in my report for '2013-10-10'
	
Scenario: Navigate to detailed adherence 
	Given I am an agent
	And I have my report data for '2013-10-04'
	And I view my report for '2013-10-04'
	When I choose to view my detailed adherence
	Then I should end up in my adherence report for '2013-10-04'

@OnlyRunIfEnabled('MyReport_AgentQueueMetrics_22254')
@ignore
Scenario: Navigate to detailed my queue metrics
	Given I am an agent
	And I view my report for '2014-05-15'
	When I click on answered calls
	Then I should see agent queue view for '2014-05-15'