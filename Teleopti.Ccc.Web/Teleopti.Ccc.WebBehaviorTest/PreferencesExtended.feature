Feature: Preferences Extended
	In order to view and submit when I prefer to work in more detail
	As an agent
	I want to view and submit extended preferences


	
Background:
         Given there is a role with
         | Field                          | Value                          |
         | Name                           | Access to extended preferences |
         | Access to mobile reports       | false                          |
         | Access to extended preferences | true                           |
         And there is a role with
         | Field                          | Value                             |
         | Name                           | No access to extended preferences |
         | Access to extended preferences | false                             |
         And there is a shift category 
         | Field | Value |
         | Name  | Late  |
         And there is an activity 
         | Field | Value |
         | Name  | Lunch |
         And there is a dayoff 
         | Field | Value  |
         | Name  | Dayoff |
         And there is an absence 
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








Scenario: Cannot edit extended preference without permission
         Given I have the role 'No access to extended preferences'
         And I have an extended preference on '2012-06-20'
         When I view preferences for date '2012-06-20'
         And I click the extended preference indication on '2012-06-20'
         Then I should not be able to edit extended preference on '2012-06-20'

Scenario: Cannot see extended preference button without permission
         Given I have the role 'No access to extended preferences'
         When I am viewing preferences
         Then I should not see the extended preference button

Scenario: Preference list contains available preferences when adding extended preference 
         Given I have the role 'Access to extended preferences'
         And I am viewing preferences
         When I click the add extended preference button
         Then I should see these available preferences
         | Value   |
         | Late    |
         | Dayoff  |
         | Illness |

Scenario: Preference list contains available preferences when editing extended preference 
         Given I have the role 'Access to extended preferences'
         And I have an extended preference on '2012-06-20'
         When I view preferences for date '2012-06-20'
         And I click the extended preference indication on '2012-06-20'
         Then I should see these available preferences
         | Value   |
         | Late    |
         | Dayoff  |
         | Illness |

Scenario: Activity list contains available activities when adding extended preference 
         Given I have the role 'Access to extended preferences'
         And I am viewing preferences
         When I click the add extended preference button
         Then I should see these available activities
         | Field    | Value |
         | Activity | Lunch |

Scenario: Activity list contains available activities when editing extended preference  
         Given I have the role 'Access to extended preferences'
         And I have an extended preference on '2012-06-20'
         When I view preferences for date '2012-06-20'
         And I click the extended preference indication on '2012-06-20'
         Then I should see these available activities
         | Field    | Value |
         | Activity | Lunch |

Scenario: Add standard preference
         Given I have the role 'Access to extended preferences'
         And I view preferences for date '2012-06-20'
         When I select day '2012-06-20'
         And I click the add extended preference button
         And I input extended preference fields with
         | Field      | Value |
         | Preference | Late  |
         And I click the save button
         Then I should not see an extended preference indication on '2012-06-20'
         And I should see the preference 'Late' on '2012-06-20'

Scenario: Add extended preference
         Given I have the role 'Access to extended preferences'
         And I view preferences for date '2012-06-20'
         When I select day '2012-06-20'
         And I click the add extended preference button
         And I input extended preference fields with
         | Field                     | Value |
         | Preference                | Late  |
         | Start time upper          | 10:30 |
         | Start time lower          | 11:00 |
         | End time upper            | 19:00 |
         | End time lower            | 20:30 |
         | Work time upper           | 08:00 |
         | Work time lower           | 08:30 |
         | Activity                  | Lunch |
         | Activity Start time upper | 11:30 |
         | Activity Start time lower | 11:45 |
         | Activity End time upper   | 12:00 |
         | Activity End time lower   | 12:15 |
         | Activity Work time upper  | 00:15 |
         | Activity Work time upper  | 00:45 |
         And I click the save button
         And I click the extended preference indication on '2012-06-20'
         Then I should see extended panel with
         | Field                     | Value |
         | Preference                | Late  |
         | Start time upper          | 10:30 |
         | Start time lower          | 11:00 |
         | End time upper            | 19:30 |
         | End time lower            | 20:00 |
         | Work time upper           | 08:00 |
         | Work time lower           | 08:30 |
         | Activity                  | Lunch |
         | Activity Start time upper | 11:30 |
         | Activity Start time lower | 11:45 |
         | Activity End time upper   | 12:00 |
         | Activity End time lower   | 12:15 |
         | Activity Work time upper  | 00:15 |
         | Activity Work time upper  | 00:45 |

Scenario: Edit extended preference
         Given I have the role 'Access to extended preferences'
         And I have an extended preference with
         | Field      | Value      |
         | Date       | 2012-06-20 |
         | IsExtended | true       |
         | Preference | Dayoff     |
         And I view preferences for date '2012-06-20'
         When I click the extended preference indication on '2012-06-20'
         And I input extended preference fields with
         | Field                     | Value |
         | Preference                | Late  |
         | Start time upper          | 10:30 |
         | Start time lower          | 11:00 |
         | End time upper            | 19:00 |
         | End time lower            | 20:30 |
         | Work time upper           | 08:00 |
         | Work time lower           | 08:30 |
         | Activity                  | Lunch |
         | Activity Start time upper | 12:00 |
         | Activity Start time lower | 12:15 |
         | Activity End time upper   | 12:30 |
         | Activity End time lower   | 12:45 |
         | Activity Work time upper  | 00:15 |
         | Activity Work time upper  | 00:45 |
         And I click the save button
         And I click the extended preference indication on '2012-06-20'
         Then I should see extended panel with
         | Field                     | Value |
         | Preference                | Late  |
         | Start time upper          | 10:30 |
         | Start time lower          | 11:00 |
         | End time upper            | 19:30 |
         | End time lower            | 20:00 |
         | Work time upper           | 08:00 |
         | Work time lower           | 08:30 |
         | Activity                  | Lunch |
         | Activity Start time upper | 12:00 |
         | Activity Start time lower | 12:15 |
         | Activity End time upper   | 12:30 |
         | Activity End time lower   | 12:45 |
         | Activity Work time upper  | 00:15 |
         | Activity Work time upper  | 00:45 |       

# Validation
Scenario: Verify time validation for preference start and end time
         Given I have the role 'Access to extended preferences'
         And I view preferences for date '2012-06-20'
         When I select day '2012-06-20'
         And I click the extended preference button
         And I input extended preference fields with
         | Field            | Value |
         | Preference       | Late  |
         | Start time upper | 10:30 |
         | Start time lower | 10:00 |
         And I click the save button
         Then I should see extended panel with
         | Field            | Value               |
         | Validation Error | Invalid time period |

Scenario: Disable all time fields when absence preference is selected
         Given I have the role 'Access to extended preferences'
         And I am viewing preferences
         When I click the add extended preference button
         And I input extended preference fields with
         | Field      | Value  |
         | Preference | Illness |
         Then I should not be able to edit time fields

Scenario: Disable all time fields, when day off is selected
         Given I have the role 'Access to extended preferences'
         And I am viewing preferences
         When I click the add extended preference button
         And I input extended preference fields with
         | Field    | Value |
         | Activity | Lunch |
         And I input extended preference fields with
         | Field      | Value  |
         | Preference | Dayoff |
         Then I should not be able to edit activity time fields

Scenario: Reset activity field when day off is selected
         Given I have the role 'Access to extended preferences'
         And I am viewing preferences
         When I click the add extended preference button
         And I input extended preference fields with
         | Field    | Value |
         | Activity | Lunch |
         And I input extended preference fields with
         | Field      | Value  |
         | Preference | Dayoff |
         Then I should see activity dropdown list selected to "none"

Scenario: Reset activity field when absence is selected
         Given I have the role 'Access to extended preferences'
         And I am viewing preferences
         And I input extended preference fields with
         | Field    | Value |
         | Activity | Lunch |
         When I click the extended preference button
         And I input extended preference fields with
         | Field      | Value   |
         | Preference | Illness |
         And I should see activity dropdown list selected to "none"
         
Scenario: Enable activity time fields when activity is selected
         Given I have the role 'Access to extended preferences'
         And I am viewing preferences
         When I click the add extended preference button
         And I input extended preference fields with
         | Field      | Value |
         | Activity   | Lunch |
         Then I should be able to edit activity upper and lower fields
