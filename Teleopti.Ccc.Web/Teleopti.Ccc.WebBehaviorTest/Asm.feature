Feature: ASM
	In order to improve adherence
	As an agent
	I want to be to see my current activities


Scenario: No permission to ASM module
	Given I am an agent without access to ASM
	When I am viewing week schedule
	Then ASM link should not be visible 

