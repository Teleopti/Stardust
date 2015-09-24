@OnlyRunIfEnabled('WfmPeople_ImportUsers_33665')
Feature: ImportUsers
	In order to easily create large amount of users
	As a IT administrator
	I want to add users by importing a file which includes all users I want to add

	Background: 
	Given I have a role with
	 | Field              | Value       |
	 | Name               | Administrator |
	 | Access to everyone | true        |
	 | Access to people   | true        |

Scenario: can open action panel
	When I view people
	And I open the action panel
	Then I should see import user command
@ignore
Scenario: can open import panel
	When I view people
	And I open the action panel
	And I open the import user command
	Then I should see import panel
