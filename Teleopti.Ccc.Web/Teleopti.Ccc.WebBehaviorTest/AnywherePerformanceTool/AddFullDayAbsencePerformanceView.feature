Feature: Add full day absence performance view
	In order to test performance on add full day absence
	As a developer
	I want to add multiple absences and see performance result
	
Background:
	Given 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Start date | 2013-05-01 |		
	
Scenario: Add absences
	Given I am a developer
	And I view add full day absence performance view
	When I input that I want to add 1000 absences for 'Pierre Baldi'
	And I click 'run'
	Then I should see statistics when read models are updated
	And I should see total run time
	And I should see average run time
	And I should see run time for each