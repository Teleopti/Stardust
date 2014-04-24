Feature: Reports for detailed adherence
	In order to improve my adherence
	As an agent
	I want to see my schedule and my adherence interval by interval for any previous day

Background: 
Given there is a role with
	| Field                               | Value                               |
	| Name                                | Access To Detailed Adherence Report |
	| Access To Detailed Adherence Report | True                                |

Scenario: Should not show the detailed adherence report if no permission
	Given I have a role with
	| Field                               | Value |
	| Access To Detailed Adherence Report | False |
	When I view my adherence report
	Then I should see a user-friendly message "no permission"

Scenario: Show the detailed adherence report
	Given I am an agent
	And I have adherence report data for '2013-10-02'
	When I view my adherence report for '2013-10-02'
	Then I should see the detailed adherence report for '2013-10-02'

Scenario: Navigate to previous day
	Given I am an agent
	And I view my adherence report for '2013-10-04'
	When I navigate to the previous day
	Then I should end up in my adherence report for '2013-10-03'
	
Scenario: Navigate to next day
	Given I am an agent
	And I view my adherence report for '2013-10-04'
	When I navigate to the next day
	Then I should end up in my adherence report for '2013-10-05'

Scenario: Navigate using date picker
	Given I am an agent
	And I view my adherence report for '2013-10-04'
	When I select the date '2013-10-10'
	Then I should end up in my adherence report for '2013-10-10'
	
Scenario: Navigate to overview with same date
	Given I am an agent
	And I view my adherence report for '2013-10-04'
	When I choose to go to overview
	Then I should end up in my report for '2013-10-04'

Scenario: Show the detailed adherence report in mobile view
	Given I am an agent
	And I have adherence report data for mobile view for '2013-10-02'
	When I view my adherence report for '2013-10-02'
	Then I should see the detailed adherence report for '2013-10-02'

