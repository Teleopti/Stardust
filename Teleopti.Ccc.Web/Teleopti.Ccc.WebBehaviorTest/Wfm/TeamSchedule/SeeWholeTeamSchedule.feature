@WFM
Feature: SeeWholeTeamSchedule
	As a team leader works in a big team
	I want to be able to see schedules of my whole team

Background: 
	Given there is a team with
 	| Field                      | Value       |     
	| Name  | My Team |
	And there is a role with
	| Field                       | Value      |
	| Name                        | TeamLeader |
	| Access to team              | My Team    |
	| Access to Outbound          | true       |
	| View unpublished schedules  | true       |
	And I have a person period with
	| Field      | Value      |
	| Team       | My Team    |
	| Start date | 2013-09-26 |
