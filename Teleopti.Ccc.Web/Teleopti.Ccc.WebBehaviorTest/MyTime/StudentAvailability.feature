Feature: Student availability
	In order to view and submit when I am available for work
	As a student agent
	I want to view and submit my availability

Scenario: View student availability
	Given I am a student agent
	When I view student availability
	Then I should see current or first future virtual schedule period +/- 1 week
	
Scenario: See student availability
	Given I am a student agent
	And I have existing student availability
	And My schedule is published
	When I view student availability
	Then I should see my existing student availability 

Scenario: No virtual schedule period
	Given I am a student agent
	And I do not have a virtual schedule period
	When I view student availability
	Then I should see a user-friendly message explaining I dont have anything to view

Scenario: No access to student availability menu item
	Given I am an agent without access to student availability
	When I am viewing an application page
	Then I should not be able to see student availability link

Scenario: No access to student availability page
	Given I am an agent without access to student availability
	When I am viewing an application page
	And I navigate to the student availability page
	Then I should see an error message

Scenario: Navigate next virtual schedule period
	Given I am a student agent
	And I have several virtual schedule periods
	And I am viewing student availability
	When I click next virtual schedule period button
	Then I should see next virtual schedule period

Scenario: Navigate previous virtual schedule period
	Given I am a student agent
	And I have several virtual schedule periods
	And I am viewing student availability
	When I click previous virtual schedule period button
	Then I should see previous virtual schedule period

Scenario: Can not edit student availability without workflow control set
	Given I am a student agent
	And I do not have a workflow control set
	When I view student availability
	Then I should see a message saying I am missing a workflow control set
	And the student availability calendar should not be editable

Scenario: Display student availability period information
	Given I am a student agent
	And I have a workflow control set
	When I view student availability
	Then I should see the student availability period information

Scenario: Can not edit student availability in closed period
	Given I am a student agent
	And I have a workflow control set with closed student availability periods
	When I view student availability
	Then the student availability calendar should not be editable

Scenario: Can edit student availability in open period
	Given I am a student agent
	And I have a workflow control set with open availability periods
	When I view student availability
	Then the student availabilty calendar should be editable

Scenario: Default to first virtual schedule period overlapping open student availability period
	Given I am a student agent
	And I have a workflow control set with student availability periods open next month
	When I view student availability
	Then I should see the first virtual schedule period overlapping open student availability period
