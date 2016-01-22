@OnlyRunIfEnabled('SeatPlanner_Logon_32003')

Feature: SeatPlan
	As a seat planner
	I want to be able to allocate seats to agents

Background: 
	Given I have a role with
		| Field                 | Value            |
		| Name                  | Seat Planner     |
		| Access to seatplanner | True             |
		| Name                  | Resource Planner |
		| Access to permissions | True             |
	And there is a planning period with
		| Field		| Value			|
		| Date		| 2015-04-10	|
	And there is a seatplan with 
		| Field		| Value		|
		| Date		| 2015-05-01	|
		| Status	| 2			| 
	And there is a seatplan with 
		| Field		| Value			|
		| Date		| 2015-05-02	|
		| Status	| 0				| 
	And there is a location with 
		| Field		| Value			|
		| Name		| Location1	|
	And I am englishspeaking swede
	

Scenario: display seat plan planning period list
	When I view Seat plan on "2015-05-01"
	Then I should see planning period available for seat planning from '2015-05-10'to '2015-06-09'

@Ignore
Scenario: display seat plan status for day
	When I view Seat plan on "2015-05-02"
	Then I should see a Seat plan status of "All chosen agents were allocated seats"

@Ignore
Scenario: select planning period to begin seat planning
	When I view Seat plan on "2015-05-01"
	And I choose the planning period beginning on '2015-05-10' for seat planning
	Then I should be able to initiate seat planning for location "Location1"
	
	
	