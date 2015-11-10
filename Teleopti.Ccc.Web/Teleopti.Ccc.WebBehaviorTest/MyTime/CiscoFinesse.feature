Feature: CiscoFinesse
	This is a page that can be accessed through Cisco Finesse portal through an Url
	To show ASM and MyReport if has permission.

Background:
Given there is a role with
| Field                    | Value                 |
| Name                     | Full access to mytime |
And there is a role with
| Field         | Value            |
| Name          | No access to ASM |
| Access To Asm | False            |

And there is a role with
| Field						| Value						|
| Name						| No access to MyReport		|
| Access to MyReport		| False					    |

And there is a role with
| Field              | Value                         |
| Name               | No access to ASM and MyReport |
| Access to MyReport | False                         |
| Access To Asm      | False                         |

Scenario: Should show all modules when has permission
	Given I am an agent
	When I accesse teleopti page through Cisco Finesse portal 
	Then I should see teleopti logo
	And I should see Asm module
	And I should see MyReport module

Scenario: should not see Asm when there is no Asm permission
Given I have the role 'No access to ASM'
When I accesse teleopti page through Cisco Finesse portal 
Then I should see teleopti logo
And I should see MyReport module
And I should not see Asm module

Scenario: should not see myreport when there is no myreport permission
Given I have the role 'No access to MyReport'
When I accesse teleopti page through Cisco Finesse portal 
Then I should see teleopti logo
And I should see Asm module
And I should not see MyReport module

Scenario: should see only a logo when there is no asm permission no myreport permission
Given I have the role 'No access to ASM and MyReport'
When I accesse teleopti page through Cisco Finesse portal 
Then I should see teleopti logo
And I should not see Asm module
And I should not see MyReport module



