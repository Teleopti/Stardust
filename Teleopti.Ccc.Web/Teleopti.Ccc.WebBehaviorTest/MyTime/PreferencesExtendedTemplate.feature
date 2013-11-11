@WatiN
Feature: Preferences Extended Template
	In order to add my preferences fast and easy
	As an agent
	I want to create and use a preference template
	
Background:
	Given there is a role with
	| Field                          | Value                          |
	| Name                           | Access to extended preferences |
	| Access to extended preferences | true                           |
	And there is a shift category with
	| Field | Value |
	| Name  | Late  |
	And there is an activity with
	| Field | Value |
	| Name  | Lunch |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2013-02-25         |
	| Available shift category   | Late               |
	| Available activity         | Lunch              |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2013-02-25 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2013-02-25 |
	And I have a preference template with 
	| Field              | Value     |
	| Name               | template1 |
	| Shift Category     | Late      |
	| Start time minimum | 10:30     |
	And I have the role 'Access to extended preferences'

Scenario: View available templates 
	Given I am viewing preferences
	When I click the add extended preference button
	And I click to open the templates dropdown
	Then I should see these available templates
	| Value     |
	| template1 |

Scenario: Select preference template
	Given I am viewing preferences
	When I click the add extended preference button
	And I select preference template with 'template1'
	Then I should see extended preference fields filled with
	| Field                       | Value      |
	| Preference                  | Late       |
	| Start time minimum          | 10:30      |
	
Scenario: Create preference template
	Given I am viewing preferences
	When I click the add extended preference button
	And I input extended preference fields with
	| Field                       | Value |
	| Preference                  | Late  |
	| Start time minimum          | 10:30 |
	| Start time maximum          | 11:00 |
	| End time minimum            | 19:00 |
	| End time minimum next day   | true  |
	| End time maximum            | 20:30 |
	| End time maximum next day   | true  |
	| Work time minimum           | 8:00 |
	| Work time maximum           | 8:30 |
	| Activity                    | Lunch |
	| Activity Start time minimum | 11:30 |
	| Activity Start time maximum | 11:45 |
	| Activity End time minimum   | 12:00 |
	| Activity End time maximum   | 12:15 |
	| Activity time minimum       | 0:30 |
	| Activity time maximum       | 1:00 |
	And I click Save as new template
	And I input new template name 'test template'
	And I click save template button
	Then I should see these available templates
	| Value         |
	| template1     |
	| test template |

Scenario: Display preference template name
	Given I am viewing preferences for date '2013-03-07'
	When I select day '2013-03-07'
	And I click the add extended preference button
	And I select preference template with 'template1'
	And I click the apply extended preferences button
	Then I should see the preference template1 on '2013-03-07'

Scenario: Delete preference template
	Given I am viewing preferences
	When I click the add extended preference button
	And I click to open the templates dropdown
	And I select preference template with 'template1'
	And I click delete button for 'template1'
	Then I should not see 'template1' in templates list
