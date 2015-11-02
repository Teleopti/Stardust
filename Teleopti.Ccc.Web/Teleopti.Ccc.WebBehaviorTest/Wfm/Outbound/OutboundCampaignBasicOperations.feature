@ignore
@OnlyRunIfEnabled('Wfm_Outbound_Campaign_32696')
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
	| Start Date | 2015-7-01   |
	| End Date   | 2015-7-07   |
	| Skill      | Direct Sales |
	And there is a campaign with
	| Field      | Value        |
	| Name       | Campaign2    |
	| Start Date | 2015-8-01   |
	| End Date   | 2015-8-07   |
	| Skill      | Direct Sales |
	And there is a campaign with
	| Field      | Value        |
	| Name       | Campaign3    |
	| Start Date | 2015-9-01   |
	| End Date   | 2015-9-07   |
	| Skill      | Direct Sales |
	And there is a campaign with
	| Field      | Value        |
	| Name       | Campaign4    |
	| Start Date | 2015-10-01   |
	| End Date   | 2015-10-07   |
	| Skill      | Direct Sales |
	And there is a campaign with
	| Field      | Value        |
	| Name       | Campaign5    |
	| Start Date | 2015-11-01   |
	| End Date   | 2015-11-07   |
	| Skill      | Direct Sales |


Scenario: Display Gantt chart for showing the campaigns
When I view outbound
Then I should see the gantt chart

@OnlyRunIfEnabled('Wfm_Outbound_Campaign_GanttChart_Navigation_34924')
Scenario: List campaigns in three months
When I view outbound
And I set the starting month for viewing period to '2015-08-01'
Then I should see 'Campaign2' in campaign list
And I should see 'Campaign3' in campaign list
And I should see 'Campaign4' in campaign list
And I should not see 'Campaign1' in campaign list
And I should not see 'Campaign5' in campaign list

@OnlyRunIfEnabled('Wfm_Outbound_Campaign_GanttChart_Navigation_34924')
Scenario: Navigate in gantt chart by month
When I view outbound
And I set the starting month for viewing period to '2015-09-01'
And I can see 'Campaign4' in campaign list
And I set the starting month for viewing period to '2015-07-01'
Then I should see 'Campaign1' in campaign list
And I should not see 'Campaign4' in campaign list


Scenario: Visualize campaign backlog
When I view outbound
And I set the starting month for viewing period to '2015-09-01'
And I can see 'Campaign4' in campaign list
And I click at campaign name tag 'Campaign4'
Then I should see the backlog visualization of 'Campaign4'
 
