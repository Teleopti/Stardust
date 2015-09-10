Feature: Tenants
In order to host multiple customers on a single installation
As a application owner
I want to simpler life

Background:
	Given I have a role with
	| Field | Value |
	| Name  | Agent |

@Ignore
Scenario: Log on to newly added scenario
	Given There is a tenant called 'NewTenant'
	And I have user credential with
	| Field    | Value       |
	| UserName | theUser     |
	| Password | thePassword |
	| Tenant   | NewTenant   |
		When I try to sign in with
	| Field    | Value       |
	| UserName | theUser     |
	| Password | thePassword |
	Then I should be signed in
