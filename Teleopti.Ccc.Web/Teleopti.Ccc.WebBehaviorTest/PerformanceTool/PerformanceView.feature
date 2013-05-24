Feature: Performance view
	In order to prove performance of a web application
	As a developer
	I want to test, measure and view the performance
	
Background:
	Given 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Start date | 2013-05-01 |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |			
	
Scenario: Add full day absences in Anywhere
	Given I am a developer
	And I view the performance view
	When I select 'anywhere'
	And I select 'add full day absences'
	And I input that I want to add 1000 absences
	And I click 'run'
	Then I should see statistics when read models are updated
	And I should see number of added absences
	And I should see total run time
	And I should see average run time
	And I should see run time for each transaction

Scenario: Remove absences in Anywhere
	Given I am a developer
	And I view the performance view
	And there are 1000 absences
	When I select 'anywhere'
	And I select 'remove absences'
	And I input that I want to remove all absences
	And I click 'run'
	Then I should see statistics when read models are updated
	And I should see number of removed absences
	And I should see total run time
	And I should see average run time
	And I should see run time for each transaction
