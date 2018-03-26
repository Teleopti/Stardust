@WFM
Feature: OutboundCampaignBasicOperations
	In order to plan outbound campaigns
	As a resource planner
	I want to perform basic operations for outbound campaigns


Background: 
	Given I am englishspeaking swede
	And I have a role with
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

Scenario: Should see correct compaign start date and end date
Given there is an activity named 'NewCampaign'
And there is a skill in timezone 'Central America Standard Time' named 'NewCampaign' with activity 'NewCampaign' 
And there is a workload named 'TheWorkload1' with skill 'NewCampaign'
And I have created a campaign with
| Field                             | Value       |
| Name                              | NewCampaign |
| Start Date                        | 2015-12-01  |
| End Date                          | 2015-12-14  |
| Skill                             | NewCampaign |
| Call List Len                     | 5555        |
| Target Rate                       | 55          |
| Connect Rate                      | 55          |
| Right Party Connect Rate          | 55          |
| Connect Average Handling Time     | 55          |
| Right Party Average Handling Time | 555         |
| Unproductive Time                 | 55          |
| Opening Hour Start                | 08:00       |
| Opening Hour End                  | 16:00       |
When I view campaign 'NewCampaign'
And I see the edit campaign form
Then I should see the campaign start date to be '01'
And I should see the campaign end date to be '14'
