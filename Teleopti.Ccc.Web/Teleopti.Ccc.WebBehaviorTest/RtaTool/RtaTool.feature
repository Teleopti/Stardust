Feature: RTA Tool
	In order make sure that RTA actually does stuff
	As a developer
	I want to be able to send statechanges
	
Background:
	Given I have a role with
	| Field | Value     |
	| Name  | Developer |
	And I have a person period with
    | Field      | Value      |	
	| Start date | 2013-06-01 |

Scenario: Should see agents from team Preference
	When I am viewing the RTA Tool
	Then I should see the 'agent-name' 'Ashley Andeen'
	