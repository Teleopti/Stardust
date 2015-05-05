Feature: OutboundCampaignBasicOperations
	In order to plan outbound campaigns
	As a resource planner
	I want to perform basic operations for outbound campaigns


Background: 
	Given I have a role with
	| Field              | Value            |
	| Name               | Resource Planner |
	| Access to outbound | true             |	
	And there is an activity with
	| Field | Value |
	| Name  | Phone |
	And there is a skill with
	| Field    | Value        |
	| Name     | Direct Sales |
	| Activity | Phone        |
	And there is a campaign with 
	| Field      | Value        |
	| Name       | Campaign1    |
	| Start Date | 2015-05-01   |
	| End Date   | 2015-05-30   |
	| Skill      | Direct Sales |
	And there is a campaign with
	| Field      | Value        |
	| Name       | Campaign2    |
	| Start Date | 2015-04-01   |
	| End Date   | 2015-04-30   |
	| Skill      | Direct Sales |
	And the time is '2015-04-22'


Scenario: List all active campaigns
	When I view outbound
	Then I should see 'Campaign1' in campaign list
	And I should see 'Campaign2' in campaign list