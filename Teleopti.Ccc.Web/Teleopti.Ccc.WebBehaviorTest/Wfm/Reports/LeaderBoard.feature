@OnlyRunIfEnabled("WfmReportPortal_LeaderBoard_39440")
Feature: GamificationLeaderBoard

Background:
	Given I am american
	And I have a role with
	| Field                     | Value          |
	| Name                      | Wfm Team Green |
	| Access to anywhere        | True           |
	| Access to matrix reports  | True           |
	| Access to wfm leaderboard | True           |

Scenario: Should be able to see leader board report
	When I view wfm leader board report
	Then I should see leader board report table

@ignore
Scenario: Should be able to view leader board report by date