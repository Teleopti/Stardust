Feature: Remove absence performance view
	In order to test performance on remove absence
	As a developer
	I want to remove multiple absences and see performance result
	
Background:
	Given 'Pierre Baldi' has a person period with
	| Field      | Value      |
	| Start date | 2013-05-01 |		
	
Scenario: Remove absences
	Given I am a developer
	And I view remove absence performance view
	And 'Pierre Baldi' has 1000 absences
	When I input that I want to remove all absences for 'Pierre Baldi'
	And I click 'run'
	Then I should see statistics when read models are updated
	And I should see total run time
	And I should see average run time
	And I should see run time for each