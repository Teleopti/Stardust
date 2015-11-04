﻿@OnlyRunIfEnabled('Wfm_Outbound_Campaign_32696')
@OnlyRunIfEnabled('Wfm_Outbound_Campaign_GanttChart_Navigation_34924')
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
	| Field | Value     |
	| Name  | Campaign1 |
	And there is an activity with
	| Field | Value     |
	| Name  | Campaign2 |
	And there is an activity with
	| Field | Value     |
	| Name  | Campaign3 |
	And there is an activity with
	| Field | Value     |
	| Name  | Campaign4 |
	And there is an activity with
	| Field | Value     |
	| Name  | Campaign5 |
	And there is a skill with
	| Field    | Value     |
	| Name     | Campaign1 |
	| Activity | Campaign1 |
	And there is a skill with
	| Field    | Value     |
	| Name     | Campaign2 |
	| Activity | Campaign2 |
	And there is a skill with
	| Field    | Value     |
	| Name     | Campaign3 |
	| Activity | Campaign3 |
	And there is a skill with
	| Field    | Value     |
	| Name     | Campaign4 |
	| Activity | Campaign4 |
	And there is a skill with
	| Field    | Value     |
	| Name     | Campaign5 |
	| Activity | Campaign5 |
	And there is a campaign with 
	| Field              | Value     |
	| Name               | Campaign1 |
	| Start Date         | 2015-7-01 |
	| End Date           | 2015-7-07 |
	| Skill              | Campaign1 |
	| Opening Hour Start | 08:00     |
	| Opening Hour End   | 16:00     |
	And there is a campaign with
	| Field              | Value     |
	| Name               | Campaign2 |
	| Start Date         | 2015-8-01 |
	| End Date           | 2015-8-07 |
	| Skill              | Campaign2 |
	| Opening Hour Start | 08:00     |
	| Opening Hour End   | 16:00     |
	And there is a campaign with
	| Field              | Value     |
	| Name               | Campaign3 |
	| Start Date         | 2015-9-01 |
	| End Date           | 2015-9-07 |
	| Skill              | Campaign3 |
	| Opening Hour Start | 08:00     |
	| Opening Hour End   | 16:00     |
	And there is a campaign with
	| Field              | Value      |
	| Name               | Campaign4  |
	| Start Date         | 2015-10-01 |
	| End Date           | 2015-10-07 |
	| Skill              | Campaign4  |
	| Opening Hour Start | 08:00      |
	| Opening Hour End   | 16:00      |
	And there is a campaign with
	| Field              | Value      |
	| Name               | Campaign5  |
	| Start Date         | 2015-11-01 |
	| End Date           | 2015-11-07 |
	| Skill              | Campaign5  |
	| Opening Hour Start | 08:00      |
	| Opening Hour End   | 16:00      |


Scenario: Display Gantt chart for showing the campaigns
When I view outbound
Then I should see the gantt chart


Scenario: List campaigns in three months
When I view outbound
And I set the starting month for viewing period to '2015-08-01'
Then I should see 'Campaign2' in campaign list
And I should see 'Campaign3' in campaign list
And I should see 'Campaign4' in campaign list
And I should not see 'Campaign1' in campaign list
And I should not see 'Campaign5' in campaign list

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
 
Scenario: Create a campaign
When I view the outbound campaign creation page 
And I see the new campaign form
And I submit the campaign form with the campaign detail
| Field                             | Value       |
| Name                              | NewCampaign |
| Start Date                        | 2015-12-01  |
| End Date                          | 2016-01-30  |
| Call List Len                     | 5555        |
| Target Rate                       | 55          |
| Connect Rate                      | 55          |
| Right Party Connect Rate          | 55          |
| Connect Average Handling Time     | 55          |
| Right Party Average Handling Time | 555         |
| Unproductive Time                 | 55          |
| Opening Hour Start                | 08:00       |
| Opening Hour End                  | 16:00       |
And after the creation I goto the campaign list page
And I set the starting month for viewing period to '2016-01-01'
Then I should see 'NewCampaign' in campaign list

Scenario: Delete a campaign
When I view campaign 'Campaign4'
And I confirm to delete the campaign
And after that I am redirected to the campaign list page 
And I set the starting month for viewing period to '2015-09-01'
Then I should not see 'Campaign4' in campaign list 


Scenario: Edit a campaign
When I view the detail of the campaign created with 
| Field                             | Value       |
| Name                              | NewCampaign |
| Start Date                        | 2015-12-01  |
| End Date                          | 2016-01-30  |
| Call List Len                     | 5555        |
| Target Rate                       | 55          |
| Connect Rate                      | 55          |
| Right Party Connect Rate          | 55          |
| Connect Average Handling Time     | 55          |
| Right Party Average Handling Time | 555         |
| Unproductive Time                 | 55          |
| Opening Hour Start                | 08:00       |
| Opening Hour End                  | 16:00       |
And I see the edit campaign form
And I change the campaign period to 
| Field      | Value      |
| Start Date | 2016-10-15 |
| End Date   | 2016-11-15 |
And after the update is done I goto the campaign list page
And I set the starting month for viewing period to '2016-10-01'
Then I should see 'NewCampaign' in campaign list 


