Feature: Preferences
	In order to view and submit when I prefer to work
	As an agent
	I want to view and submit my work preferences

Scenario: View preferences
	Given I am an agent
	And the current time is '2014-05-02 08:00'
	When I view preferences
	Then I should see the virtual schedule period from '2014-04-21' to '2014-05-04'

Scenario: See shift category preference
	Given I am an agent
	And there is a shift category named 'Night'
	And I have existing shift category preference with
	| Field          | Value      |
	| Date           | 2014-05-03 |
	| Shift category | Night      |
	When I view preferences for date '2014-05-02'
	Then I should see my existing 'Night' preference

Scenario: See day off preference
	Given I am an agent
	And there is a dayoff named 'Day off'
	And I have existing day off preference with
	| Field            | Value      |
	| Date             | 2014-05-03 |
	| Day off template | Day off    |
	When I view preferences for date '2014-05-02'
	Then I should see my existing 'Day off' preference

Scenario: See absence preference
	Given I am an agent
	And there is an absence named 'Vacation'
	And I have existing absence preference with
	| Field   | Value      |
	| Date    | 2014-05-03 |
	| Absence | Vacation   |
	When I view preferences for date '2014-05-02'
	Then I should see my existing 'Vacation' preference
	
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
	Given I have a role with
         | Field                          | Value |
         | Access To Preferences          | False |
         | Access To Extended Preferences | False |
	When I am viewing an application page
	Then I should not be able to see preferences link

Scenario: No access to preferences page
	Given I have a role with
         | Field                          | Value |
         | Access To Preferences          | False |
         | Access To Extended Preferences | False |
	When I am viewing an application page
	And I navigate to the preferences page
	Then I should see an error message

Scenario: Navigate next virtual schedule period
	Given I am an agent
	And I have several virtual schedule periods
	And I am viewing preferences for date '2001-01-01'
	When I click next virtual schedule period button
	Then I should see the virtual schedule period from '2001-01-15' to '2001-01-28'

Scenario: Navigate previous virtual schedule period
	Given I am an agent
	And I have several virtual schedule periods
	And I am viewing preferences for date '2001-02-01'
	When I click previous virtual schedule period button
	Then I should see the virtual schedule period from '2001-01-15' to '2001-01-28'

Scenario: View standard preference list
	Given I have a role with
         | Field                          | Value |
         | Access To Extended Preferences | false |
	And I have schedule and person period 
	And there is a shift category named 'Night'
	And there is a dayoff named 'Day off'
	And there is an absence named 'Vacation'
	And I have a workflow control set with
	| Field                      | Value                |
	| Name                       | Workflow control set |
	| Schedule published to date | 2014-06-05           |
	| Preference period start    | 2014-05-03           |
	| Preference period end      | 2014-05-05           |
	| Available shift category   | Night                |
	| Available day off          | Day off              |
	| Available absence          | Vacation             |
	And I am viewing preferences for date '2014-05-02'
	When I click the standard preference split-button
	Then I should see the workflow control set's standard preferences list with 
	| Preference |
	| Night      |
	| Day off    |
	| Vacation   |

Scenario: Remember selected standard preference
	Given I have a role with
         | Field                          | Value |
         | Access To Extended Preferences | false |
	And I have schedule and person period 
	And there is a shift category named 'Night'
	And I have a workflow control set with
	| Field                   | Value      |
	| Name                    | Open       |
	| SchedulePublishedToDate | 2014-06-05 |
	| PreferencePeriodStart   | 2014-05-03 |
	| PreferencePeriodEnd     | 2014-05-05 |
	| AvailableShiftCategory  | Night      |
	And I am viewing preferences for date '2014-05-02'
	When I change standard preference to shift category 'Night'
	And I click next virtual schedule period button
	Then I should see the selected standard preference 'Night' in the split-button

Scenario: Add standard preference
	Given I have a role with
         | Field                          | Value |
         | Access To Extended Preferences | false |
	And I have schedule and person period 
	And the current time is '2014-05-02 08:00'
	And there is a shift category named 'Night'
	And I have a workflow control set with
	| Field                   | Value      |
	| Name                    | Open       |
	| SchedulePublishedToDate | 2014-06-05 |
	| PreferencePeriodStart   | 2014-05-03 |
	| PreferencePeriodEnd     | 2014-05-05 |
	| AvailableShiftCategory  | Night      |
	And I am viewing preferences
	When I select an editable day without preference
	And I select shift category 'Night' as standard preference
	Then I should see the standard preference 'Night' in the calendar

Scenario: Replace standard preference
	Given I have a role with
         | Field                          | Value |
         | Access To Extended Preferences | false |
	And I have schedule and person period 
	And the current time is '2014-05-02 08:00'
	And there is a shift category named 'Night'
	And there is a day off named 'Day off'
	And I have a workflow control set with
	| Field                   | Value      |
	| Name                    | Open       |
	| SchedulePublishedToDate | 2014-06-05 |
	| PreferencePeriodStart   | 2014-05-03 |
	| PreferencePeriodEnd     | 2014-05-05 |
	| AvailableShiftCategory  | Night      |
	| AvailableDayOff         | Day off    |
	And I have existing standard preference with
	| Field      | Value      |
	| Date       | 2014-05-03 |
	| Preference | Day off    |
	And I am viewing preferences for date '2014-05-02'
	When I select an editable day with standard preference
	And I select shift category 'Night' as standard preference
	Then I should see the standard preference 'Night' in the calendar
	And I should not see the former standard preference in the calendar

Scenario: Set multiple preference
	Given I have a role with
         | Field                          | Value |
         | Access To Extended Preferences | false |
	And I have schedule and person period 
	And the current time is '2014-05-02 08:00'
	And there is a shift category named 'Night'
	And there is a day off named 'Day off'
	And I have a workflow control set with
	| Field                   | Value      |
	| Name                    | Open       |
	| SchedulePublishedToDate | 2014-06-05 |
	| PreferencePeriodStart   | 2014-05-03 |
	| PreferencePeriodEnd     | 2014-05-05 |
	| AvailableShiftCategory  | Night      |
	| AvailableDayOff         | Day off    |
	And I have existing standard preference with
	| Field      | Value      |
	| Date       | 2014-05-03 |
	| Preference | Day off    |
	And I am viewing preferences for date '2014-05-03'
	When I select an editable day with standard preference
	And I also select an editable day without standard preference
	And I select shift category 'Night' as standard preference
	Then I should see the 2 standard preferences 'Night' in the calendar

Scenario: Delete multiple standard preference
	Given I have a role with
         | Field                          | Value |
         | Access To Extended Preferences | false |
	And I have schedule and person period 
	And I have a workflow control set with open standard preference period
	And I have 2 existing standard preference with
	| Field | Value      |
	| Date  | 2014-05-02 |
	And I am viewing preferences for date '2014-05-02'
	When I select 2 editable day with standard preference
	And I click the delete preference button
	Then I should no longer see the 2 standard preferences in the calendar


Scenario: Can not edit preference without workflow control set
	Given I am an agent
	And I do not have a workflow control set
	When I view preferences
	Then I should see a message saying I am missing a workflow control set
	And the preference calendar should not be editable

Scenario: Display preference period information
	Given I am an agent
	And the current time is '2014-05-02 08:00'
	And I have a workflow control set
	When I view preferences
	Then I should see the preference period information with open from '1900-04-30' to '2077-11-16' and input from '1900-04-30' to '2077-11-16'

Scenario: Can not edit preference in closed period
	Given I am an agent
	And I have a workflow control set with closed preference periods
	When I view preferences
	Then the preference calendar should not be editable

Scenario: Can edit preference in open period
	Given I am an agent
	And the current time is '2014-05-02 08:00'
	And I have a workflow control set with open standard preference period
	When I view preferences
	Then the preference calendar should be editable

Scenario: Default to first virtual schedule period overlapping open preference period
	Given I am an agent
	And the current time is '2014-05-02 08:00'
	And I have a workflow control set with preference periods open from '2014-06-01' to '2014-06-30'
	When I view preferences
	Then I should see the first virtual schedule period overlapping open preference period starting at '2014-06-01'
	
Scenario: Show friendly message on preference page when selected date is after leaving date
	Given the current time is '2014-05-02 08:00'
	And I am an agent in a team that leaves on '2014-12-31'
	When I view preferences for date '2030-01-01'
	Then I should see a user-friendly message explaining I dont have anything to view

