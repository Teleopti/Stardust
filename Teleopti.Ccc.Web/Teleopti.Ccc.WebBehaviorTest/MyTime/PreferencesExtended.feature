Feature: Preferences Extended
	In order to view and submit when I prefer to work in more detail
	As an agent
	I want to view and submit extended preferences


	
Background:
	Given there is a role with
	| Field                          | Value                          |
	| Name                           | Access to extended preferences |
	| Access to extended preferences | true                           |
	And there is a role with
	| Field                          | Value                             |
	| Name                           | No access to extended preferences |
	| Access to extended preferences | false                             |
	And there is a shift category with
	| Field | Value |
	| Name  | Late  |
	And there is a shift category with
	| Field | Value |
	| Name  | Early |
	And there is an activity with
	| Field | Value |
	| Name  | Lunch |
	And there is a dayoff with
	| Field | Value  |
	| Name  | Dayoff |
	And there is an absence with
	| Field | Value   |
	| Name  | Illness |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2012-06-24         |
	| Available shift category   | Late               |
	| Available dayoff           | Dayoff             |
	| Available absence          | Illness            |
	| Available activity         | Lunch              |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |

Scenario: Can see indication of an extended preference
	Given I have the role 'Access to extended preferences'
	And I have an extended preference on '2012-06-20'
	When I view preferences for date '2012-06-20'
	Then I should see that I have an extended preference on '2012-06-20'

Scenario: Can see extended preference
	Given I have the role 'Access to extended preferences'
	And I have an extended preference on '2012-06-20'
	When I view preferences for date '2012-06-20'
	And I click the extended preference indication on '2012-06-20'
	Then I should see the extended preference on '2012-06-20'

Scenario: Can see extended preference without permission
	Given I have the role 'No access to extended preferences'
	And I have an extended preference on '2012-06-20'
	When I view preferences for date '2012-06-20'
	And I click the extended preference indication on '2012-06-20'
	Then I should see the extended preference on '2012-06-20'




Scenario: Can not add extended preference without permission
	Given I have the role 'No access to extended preferences'
	When I am viewing preferences
	Then I should not see the extended preference button

Scenario: Add standard preference
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences for date '2012-06-20'
	When I select day '2012-06-20'
	And I click the add extended preference button
	And I input extended preference fields with
	| Field      | Value |
	| Preference | Late  |
	And I click the apply extended preferences button
	Then I should not see an extended preference indication on '2012-06-20'
	And I should see the preference Late on '2012-06-20'

Scenario: Add extended preference
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences for date '2012-06-20'
	When I select day '2012-06-20'
	And I click the add extended preference button
	And I input extended preference fields with
	| Field                       | Value |
	| Preference                  | Late  |
	| Start time minimum          | 10:30 |
	| Start time maximum          | 11:00 |
	| End time minimum            | 19:00 |
	| End time maximum            | 20:30 |
	| Work time minimum           | 8:00  |
	| Work time maximum           | 8:30  |
	| Activity                    | Lunch |
	| Activity Start time minimum | 11:30 |
	| Activity Start time maximum | 11:45 |
	| Activity End time minimum   | 12:00 |
	| Activity End time maximum   | 12:15 |
	| Activity time minimum       | 0:30  |
	| Activity time maximum       | 1:00  |
	And I click the apply extended preferences button
	And I click the extended preference indication on '2012-06-20'
	Then I should see extended preference with
	| Field                       | Value      |
	| Date                        | 2012-06-20 |
	| Preference                  | Late       |
	| Start time minimum          | 10:30      |
	| Start time maximum          | 11:00      |
	| End time minimum            | 19:00      |
	| End time maximum            | 20:30      |
	| Work time minimum           | 8:00       |
	| Work time maximum           | 8:30       |
	| Activity                    | Lunch      |
	| Activity Start time minimum | 11:30      |
	| Activity Start time maximum | 11:45      |
	| Activity End time minimum   | 12:00      |
	| Activity End time maximum   | 12:15      |
	| Activity time minimum       | 0:30       |
	| Activity time maximum       | 1:00       |

Scenario: Add extended preference when span to next day
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences for date '2012-09-05'
	When I select day '2012-09-05'
	And I click the add extended preference button
	And I input extended preference fields with
	| Field                     | Value |
	| End time minimum          | 02:00 |
	| End time minimum next day | true  |
	| End time maximum          | 02:30 |
	| End time maximum next day | true  |
	And I click the apply extended preferences button
	And I click the extended preference indication on '2012-09-05'
	Then I should see extended preference with
	| Field            | Value      |
	| Date             | 2012-09-05 |
	| End time minimum | 02:00 +1   |
	| End time maximum | 02:30 +1   |

Scenario: View available preference list when adding extended preference 
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	And I click to open thee extended preference drop down list
	Then I should see these available preferences
	| Value   |
	| Late    |
	| Dayoff  |
	| Illness |

Scenario: View available activity list when adding extended preference
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	And I click to open thee extended activity drop down list
	Then I should see these available activities
	| Value |
	| Lunch |

Scenario: Replace extended preference
	Given I have the role 'Access to extended preferences'
	And I have an extended preference with
	| Field          | Value      |
	| Date           | 2012-09-05 |
	| IsExtended     | true       |
	| Shift Category | Early      |
	And I am viewing preferences for date '2012-09-05'
	When I select day '2012-09-05'
	And I click the add extended preference button
	And I input extended preference fields with
	| Field                       | Value |
	| Preference                  | Late  |
	| Start time minimum          | 10:30 |
	| Start time maximum          | 11:00 |
	| End time minimum            | 19:00 |
	| End time maximum            | 20:30 |
	| Work time minimum           | 8:00  |
	| Work time maximum           | 8:30  |
	| Activity                    | Lunch |
	| Activity Start time minimum | 12:00 |
	| Activity Start time maximum | 12:15 |
	| Activity End time minimum   | 12:30 |
	| Activity End time maximum   | 12:45 |
	| Activity time minimum       | 0:30  |
	| Activity time maximum       | 1:00  |
	And I click the apply extended preferences button
	Then I should see the preference Late on '2012-09-05'
	When I click the extended preference indication on '2012-09-05'
	Then I should see extended preference with
	| Field                       | Value      |
	| Date                        | 2012-09-05 |
	| Preference                  | Late       |
	| Start time minimum          | 10:30      |
	| Start time maximum          | 11:00      |
	| End time minimum            | 19:00      |
	| End time maximum            | 20:30      |
	| Work time minimum           | 8:00       |
	| Work time maximum           | 8:30       |
	| Activity                    | Lunch      |
	| Activity Start time minimum | 12:00      |
	| Activity Start time maximum | 12:15      |
	| Activity End time minimum   | 12:30      |
	| Activity End time maximum   | 12:45      |
	| Activity time minimum       | 0:30       |
	| Activity time maximum       | 1:00       |

Scenario: Validate preference times
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences for date '2012-06-20'
	When I select day '2012-06-20'
	And I click the add extended preference button
	And I input extended preference fields with
	| Field              | Value |
	| Preference         | Late  |
	| Start time minimum | 10:30 |
	| Start time maximum | 10:00 |
	And I click the apply extended preferences button
	Then I should see add extended preferences panel with error 'Invalid time startTime'

Scenario: Hide all time fields when absence preference is selected
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	And I input extended preference fields with
	| Field      | Value   |
	| Preference | Illness |
	Then I should not see the edit time fields

Scenario: Hide all time fields, when day off is selected
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	And I input extended preference fields with
	| Field      | Value  |
	| Preference | Dayoff |
	Then I should not see the edit time fields
	
Scenario: Reset time input fields when day off is selected
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	And I input extended preference fields with
	| Field                       | Value |
	| Preference                  | Late  |
	| Start time minimum          | 10:30 |
	| Start time maximum          | 11:00 |
	| End time minimum            | 19:00 |
	| End time maximum            | 20:30 |
	| Work time minimum           | 8:00 |
	| Work time maximum           | 8:30 |
	| Activity                    | Lunch |
	| Activity Start time minimum | 12:00 |
	| Activity Start time maximum | 12:15 |
	| Activity End time minimum   | 12:30 |
	| Activity End time maximum   | 12:45 |
	| Activity time minimum       | 0:30 |
	| Activity time maximum       | 1:00 |
	And I input extended preference fields with
	| Field      | Value  |
	| Preference | Dayoff |
	And I input extended preference fields with
	| Field      | Value  |
	| Preference | Late |
	Then all the time fields should be reset
	
Scenario: Reset time input fields when absence is selected
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	And I input extended preference fields with
	| Field                       | Value |
	| Preference                  | Late  |
	| Start time minimum          | 10:30 |
	| Start time maximum          | 11:00 |
	| End time minimum            | 19:00 |
	| End time maximum            | 20:30 |
	| Work time minimum           | 8:00 |
	| Work time maximum           | 8:30 |
	| Activity                    | Lunch |
	| Activity Start time minimum | 12:00 |
	| Activity Start time maximum | 12:15 |
	| Activity End time minimum   | 12:30 |
	| Activity End time maximum   | 12:45 |
	| Activity time minimum       | 0:30 |
	| Activity time maximum       | 1:00 |
	And I input extended preference fields with
	| Field      | Value   |
	| Preference | Illness |
	And I input extended preference fields with
	| Field      | Value  |
	| Preference | Late |
	Then all the time fields should be reset
		 
Scenario: Show activity time fields when activity is selected
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	And I input extended preference fields with
	| Field      | Value |
	| Activity   | Lunch |
	Then I should see the activity minimum and maximum fields

Scenario: Hide activity time fields when no activity selected
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	Then I should not see the edit activity minimum and maximum fields

Scenario: Reset extended preference form 
	Given I have the role 'Access to extended preferences'
	And I am viewing preferences
	When I click the add extended preference button
	And I input extended preference fields with
	| Field                       | Value |
	| Preference                  | Late  |
	| Start time minimum          | 10:30 |
	| Start time maximum          | 11:00 |
	| End time minimum            | 19:00 |
	| End time maximum            | 20:30 |
	| Work time minimum           | 8:00 |
	| Work time maximum           | 8:30 |
	| Activity                    | Lunch |
	| Activity Start time minimum | 12:00 |
	| Activity Start time maximum | 12:15 |
	| Activity End time minimum   | 12:30 |
	| Activity End time maximum   | 12:45 |
	| Activity time minimum       | 0:30 |
	| Activity time maximum       | 1:00 |
	And I click the reset extended preference button
	Then I should see nothing selected in the preference dropdown
	And all the time fields should be reset
	And I should see nothing selected in the activity dropdown
	And I should not see the edit activity minimum and maximum fields