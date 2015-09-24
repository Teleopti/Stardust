Feature: Health Check
	In order to know the status of the installation
	As a developer
	I want to see the details without remoting to the server
	
Background:
	Given I have a role with
	| Field | Value     |
	| Name  | Developer |
	And I have a person period with
    | Field      | Value      |	
	| Start date | 2013-06-01 |

@ignore
Scenario: Should see running services
	When I am viewing the health check view
	Then I should see the 'services' ''
	And I should see the 'etl-log-objects' ''
	And I should see the 'etl-history' ''