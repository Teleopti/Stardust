Feature: Preferences
	In order to view and submit when I prefer to work
	As an agent
	I want to view and submit my work preferences

Scenario: View preferences
	Given I am an agent
	When I view preferences
	Then I should see current or first future virtual schedule period +/- 1 week

Scenario: See shift category preference
	Given I am an agent
	And I have existing shift category preference
	When I view preferences
	Then I should see my existing shift category preference

Scenario: See day off preference
	Given I am an agent
	And I have existing day off preference
	When I view preferences
	Then I should see my existing day off preference

Scenario: See absence preference
	Given I am an agent
	And I have existing absence preference
	When I view preferences
	Then I should see my existing absence preference
	
Scenario: No schedule period
	Given I am an agent
	And I do not have a schedule period
	When I view preferences
	Then I should see a user-friendly message explaining I dont have anything to view
	
Scenario: No person period
	Given I am an agent
	And I do not have a person period
	When I view preferences
	Then I should see a user-friendly message explaining I dont have anything to view

Scenario: No access to preferences menu item
	Given I am an agent without access to preferences
	When I am viewing an application page
	Then I should not be able to see preferences link

#23776 disabling in previous versions
@ignore
Scenario: No access to preferences page
	Given I am an agent without access to preferences
	When I am viewing an application page
	And I navigate to the preferences page
	Then I should see an error message

Scenario: Navigate next virtual schedule period
	Given I am an agent
	And I have several virtual schedule periods
	And I am viewing preferences
	When I click next virtual schedule period button
	Then I should see next virtual schedule period

Scenario: Navigate previous virtual schedule period
	Given I am an agent
	And I have several virtual schedule periods
	And I am viewing preferences
	When I click previous virtual schedule period button
	Then I should see previous virtual schedule period

Scenario: View standard preference list
	Given I am an agent without access to extended preferences
	And I have an open workflow control set with an allowed standard preference
	And I am viewing preferences
	When I click the standard preference split-button
	Then I should see the workflow control set's standard preferences list

Scenario: Remember selected standard preference
	Given I am an agent without access to extended preferences
	And I have an open workflow control set with an allowed standard preference
	And I am viewing preferences
	When I change standard preference
	And I click next virtual schedule period button
	Then I should see the selected standard preference in the split-button

Scenario: Add standard preference
	Given I am an agent without access to extended preferences
	And I have an open workflow control set with an allowed standard preference
	And I am viewing preferences
	When I select an editable day without preference
	And I select a standard preference
	Then I should see the standard preference in the calendar

Scenario: Replace standard preference
	Given I am an agent without access to extended preferences
	And I have an open workflow control set with an allowed standard preference
	And I have existing standard preference
	And I am viewing preferences
	When I select an editable day with standard preference
	And I select a standard preference
	Then I should see the standard preference in the calendar
	And I should not see the former standard preference in the calendar

Scenario: Set multiple preference
	Given I am an agent without access to extended preferences
	And I have an open workflow control set with an allowed standard preference
	And I have existing standard preference
	And I am viewing preferences
	When I select an editable day with standard preference
	And I also select an editable day without standard preference
	And I select a standard preference
	Then I should see the 2 standard preferences in the calendar

Scenario: Delete multiple standard preference
	Given I am an agent without access to extended preferences
	And I have a workflow control set with open standard preference period
	And I have 2 existing standard preference
	And I am viewing preferences
	When I select 2 editable day with standard preference
	And I click the delete button
	Then I should no longer see the 2 standard preferences in the calendar




Scenario: Can not edit preference without workflow control set
	Given I am an agent
	And I do not have a workflow control set
	When I view preferences
	Then I should see a message saying I am missing a workflow control set
	And the preference calendar should not be editable

Scenario: Display preference period information
	Given I am an agent
	And I have a workflow control set
	When I view preferences
	Then I should see the preference period information

Scenario: Can not edit preference in closed period
	Given I am an agent
	And I have a workflow control set with closed preference periods
	When I view preferences
	Then the preference calendar should not be editable

Scenario: Can edit preference in open period
	Given I am an agent
	And I have a workflow control set with open standard preference period
	When I view preferences
	Then the preference calendar should be editable

Scenario: Default to first virtual schedule period overlapping open preference period
	Given I am an agent
	And I have a workflow control set with preference periods open next month
	When I view preferences
	Then I should see the first virtual schedule period overlapping open preference period

Scenario: Show friendly message on preference page when selected date is after leaving date
	Given I am an agent in a team that leaves tomorrow
	When I view preferences for date '2030-01-01'
	Then I should see a user-friendly message explaining I dont have anything to view

