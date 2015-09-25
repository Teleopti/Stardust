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

Scenario: Should see running services
	When I am viewing the health check view
	Then I should see status on 'services'
	And I should see status on 'etl-log-objects'
	And I should see status on 'etl-history'