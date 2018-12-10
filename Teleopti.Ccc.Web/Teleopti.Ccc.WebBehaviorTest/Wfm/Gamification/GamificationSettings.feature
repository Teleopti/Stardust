Feature: GamificationSettings
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Background:
	Given I am american
	And there is a gamification setting with
	| Field | Value   |
	| Name  | Default |
	And there is a gamification setting with
	| Field | Value |
	| Name  | Test  |
	And I have a role with
	| Field						 | Value            |
	| Name						 | Gamification     |
	| Access to wfm gamification | true             |

Scenario: Should view gamification settings
    When I view wfm gamification
	Then I should see a gamification setting 'Default' in the dropdown list

Scenario: Should popup confirm dialog before removing a gamification setting
    When I view wfm gamification
	And I select a gamification setting 'Test' to remove from the dropdown list
	And I click remove button
	Then I should see a popup confirm dialog before removing a gamification setting 'Test'
	
Scenario: Should not remove a gamification setting when click cancel button of confirm dialog
    When I view wfm gamification
	And I select a gamification setting 'Test' to remove from the dropdown list
	And I click remove button
	And I click cancel button of confirm dialog
	Then I should see a gamification setting 'Test' in the dropdown list

Scenario: Should remove a gamification setting when click ok button of confirm dialog
    When I view wfm gamification
	And I select a gamification setting 'Test' to remove from the dropdown list
	And I click remove button
	And I click ok button of confirm dialog
	Then I should not see a gamification setting 'Test' in the dropdown list


	
