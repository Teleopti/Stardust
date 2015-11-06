Feature: CiscoFinesse
	This is a page that can be accessed through Cisco Finesse portal through an Url
	To show ASM and MyReport if has permission.

Scenario: Should show all modules when has permission
	Given I am an agent
	When I accesse teleopti page through Cisco Finesse portal 
	Then I should see teleopti logo
	And I should see Asm module
	And I should see MyReport module

