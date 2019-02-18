Feature: Student availability
	In order to view and submit when I am available for work
	As a student agent
	I want to view and submit my availability

	@NotKeyExample
Scenario: View student availability
	Given I am a student agent
	And the time is '2014-05-02 08:00'
	When I view student availability
	Then I should see the virtual schedule period from '2014-04-21' to '2014-05-04'
	
Scenario: See student availability
	Given I am a student agent
	And the time is '2014-05-02 08:00'
	And I have a student availability with
	| Field      | Value      |
	| Date       | 2014-05-03 |
	| Start time | 07:00      |
	| End time   | 18:00      |
	And My schedule is published
	When I view student availability for '2014-05-02'
	Then I should see my existing student availability 

	@NotKeyExample
Scenario: No virtual schedule period
	Given I am a student agent
	And I do not have a virtual schedule period
	When I view student availability
	Then I should see a user-friendly message explaining I dont have anything to view

	@NotKeyExample
Scenario: No access to student availability menu item
	Given I have a role with
         | Field                          | Value |
         | Access To Student Availability | False |
	When I am viewing an application page
	Then I should not be able to see student availability link

	@NotKeyExample
Scenario: No access to student availability page
	Given I have a role with
         | Field                          | Value |
         | Access To Student Availability | False |
	When I am viewing an application page
	And I navigate to the student availability page
	Then I should see an error message
	
	@NotKeyExample
Scenario: Navigate next virtual schedule period
	Given I am a student agent
	And I have several virtual schedule periods
	And I am viewing student availability for '2001-01-01'
	When I click next virtual schedule period button
	Then I should see the virtual schedule period from '2001-01-15' to '2001-01-28'

	@NotKeyExample
Scenario: Navigate previous virtual schedule period
	Given I am a student agent
	And I have several virtual schedule periods
	And I am viewing student availability for date '2001-02-01'
	When I click previous virtual schedule period button
	Then I should see the virtual schedule period from '2001-01-15' to '2001-01-28'

	@NotKeyExample
Scenario: Can not edit student availability without workflow control set
	Given I am a student agent
	And I do not have a workflow control set
	When I view student availability
	Then I should see a message saying I am missing a workflow control set
	And the student availability calendar should not be editable

Scenario: Display student availability period information
	Given I am a student agent
	And the time is '2014-05-02 08:00'
	And I have a workflow control set
	When I view student availability
	Then I should see the student availability period information with period '1900-04-30' to '2077-11-16', and input period '1900-04-30' to '2077-11-16'
	
	@NotKeyExample
Scenario: Can not edit student availability in closed period
	Given I am a student agent
	And I have a workflow control set with closed student availability periods
	When I view student availability
	Then the student availability calendar should not be editable

Scenario: Can edit student availability in open period
	Given I am a student agent
	And the time is '2014-05-02 08:00'
	And I have a workflow control set with open availability periods
	When I view student availability
	Then the student availabilty calendar should be editable

Scenario: Can edit all student availabilities in open period
	Given I am a student agent
	And the time is '2014-05-02 08:00'
	And I have a workflow control set with open availability periods
	When I view student availability
	Then All the student availabilties calendar should be editable

	@NotKeyExample
Scenario: Default to first virtual schedule period overlapping open student availability period
	Given I am a student agent
	And the time is '2014-05-02 08:00'
	And I have a workflow control set with student availability periods open from '2014-06-01' to '2014-06-30'
	When I view student availability
	Then I should see the first virtual schedule period overlapping open student availability period starting at '2014-06-01'


Scenario: Should indicate days not available
	Given I am a student agent
	And the time is '2014-05-02 08:00'
	When I view student availability
	Then I should see I am not available for '2014-05-02'


Scenario: Should indicate days have no valid shift for availability setting
	Given I am a student agent
	And I have a shift bag with start times 8 to 9 and end times 12 to 22
	And the time is '2014-05-02 08:00'
	And I have a student availability with
	| Field      | Value      |
	| Date       | 2014-05-03 |
	| Start time | 07:00      |
	| End time   | 07:15      |
	When I view student availability
	Then I should see there is no valid shift for my availability on '2014-05-03'

	@NotKeyExample
Scenario: Should show valid shift for days with availability setting
	Given I am a student agent
	And I have a shift bag with start times 8 to 9 and end times 12 to 22
	And the time is '2014-05-02 08:00'
	And I have a student availability with
	| Field      | Value      |
	| Date       | 2014-05-03 |
	| Start time | 08:30      |
	| End time   | 19:30      |
	When I view student availability
	Then I should see valid shift for my availability on '2014-05-03'

	@NotKeyExample
Scenario:  Should display period feedback 
	Given I am a student agent
	And the time is '2014-05-02 08:00'
	When I view student availability
	Then I should see the period feedback