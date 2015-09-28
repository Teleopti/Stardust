Feature: Tenants
In order to host multiple customers on a single installation
As a application owner
I want to simpler life

Background:
	Given there is a role with
	| Field | Value                 |
	| Name  | Full access to mytime |

Scenario: Log on to newly added scenario
	Given There is a tenant called 'NewTenant'
	And I have user credential with
	| Field    | Value       |
	| UserName | theUser     |
	| Password | thePassword |
	| Tenant   | NewTenant   |
	When I am viewing an application page as 'theUser' with password 'thePassword'
	Then I should be signed in