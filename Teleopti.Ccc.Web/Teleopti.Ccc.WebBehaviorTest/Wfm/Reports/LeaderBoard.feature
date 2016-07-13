@OnlyRunIfEnabled('WfmReportPortal_LeaderBoard_39440')
Feature: GamificationLeaderBoard

Background:
	Given I have a role with
	| Field                     | Value          |
	| Name                      | Wfm Team Green |
	| Access to anywhere        | True           |
	| Access to matrix reports  | True           |
	| Access to wfm leaderboard | True           |
	| Access to my site     | true  |
	And there is a site named 'The site'
	And there is a team named 'Team green' on 'The site'
	And Pierre Baldi has a person period with
		| Field      | Value      |
		| Team       | Team green   |
		| Start Date | 2014-01-06 |
	And Ashley Andeen has a person period with
		| Field      | Value      |
		| Team       | Team green   |
		| Start Date | 2014-01-06 |
	And I have a person period with
		| Field      | Value      |
		| Team       | Team green |
		| Start Date | 2014-01-06 |
	And There is a gamification setting with
		| Field                 | Value   |
		| Description           | setting |
		| AnsweredCallsUsed     | True    |
		| AHTUsed               | True    |
		| AdherenceUsed         | True    |
		| Silver to bronze rate | 5       |
		| Gold to silver rate   | 5       |
	And There are teams applied with settings with
		| Team        | GamificationSetting |
		| Team green  | setting             |
	And I have badges based on the specific setting with
		| Badge type          | Bronze | Silver | Gold | LastCalculatedDate |
		| AnsweredCalls       | 4      | 1      | 2    | 2014-08-11         |
		| AverageHandlingTime | 2      | 1      | 1    | 2014-08-11         |
		| Adherence           | 3      | 0      | 3    | 2014-08-11         |	
	And Pierre Baldi has badges based on the specific setting with
		| Badge type          | Bronze | Silver | Gold | LastCalculatedDate |
		| AnsweredCalls       | 4      | 1      | 2    | 2014-08-11         |
		| AverageHandlingTime | 2      | 1      | 1    | 2014-08-11         |
		| Adherence           | 3      | 0      | 0    | 2014-08-11         |
	And Ashley Andeen has badges based on the specific setting with
		| Badge type          | Bronze | Silver | Gold | LastCalculatedDate |
		| AnsweredCalls       | 4      | 1      | 2    | 2014-08-11         |
		| AverageHandlingTime | 2      | 1      | 1    | 2014-08-11         |
		| Adherence           | 3      | 0      | 2    | 2014-08-11         |
		| Adherence           | 0      | 0      | 1    | 2014-08-13         |

@OnlyRunIfDisabled('WfmReportPortal_LeaderBoardByPeriod_39620')
Scenario: Should be able to see leader board report
	When I view wfm leader board report
	Then I should see the ranks are
		| Rank | Agent         |
		| 1    | I             |
		| 1    | Ashley Andeen |
		| 3    | Pierre Baldi  |

@OnlyRunIfEnabled('WfmReportPortal_LeaderBoardByPeriod_39620')
Scenario: Should be able to see leader board in given date range
	When I view wfm leader board report
	And I select date from '2014-08-11' to '2014-08-11'
	Then I should see the ranks are
		| Rank | Agent         |
		| 1    | I             |
		| 2    | Ashley Andeen |
		| 3    | Pierre Baldi  |


