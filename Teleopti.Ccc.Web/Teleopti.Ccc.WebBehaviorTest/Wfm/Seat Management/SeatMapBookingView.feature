@OnlyRunIfEnabled('SeatPlanner_Logon_32003')
@OnlyRunIfEnabled('Wfm_SeatPlan_SeatMapBookingView_32814')

Feature: SeatMapBookingView
	I want to view the seat bookings

Background: 
	Given I have a role with
		| Field                 | Value            |
		| Name                  | Seat Planner     |
		| Access to seatplanner | True             |
		| Name                  | Resource Planner |
		| Access to permissions | True             |
	And there is a seat in root location with
		| Field    | Value |
		| Name     | 1     |
		| Priority | 1     |
	And there is a seat booking for me
		| Field         | Value            |
		| BelongsToDate | 2015-01-01       |
		| StartDateTime | 2015-01-01 8:00  |
		| EndDateTime   | 2015-01-01 16:00 |
		| SeatName      | 1                |


Scenario: be able to open the seat map booking view
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then the seat map booking should be opened

Scenario: be able to go back to seat plan
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	And I press back button on seat booking view
	Then I should go back to seat plan

Scenario: the date is the same with I chosed
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then the date of datepicker should be correct

Scenario: be able to view booking detail of selected seat
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then I am able to view booking detail of selected seat

Scenario: be able to delete seat booking for the first seat
	When I view Seat plan on "2015-01-01"
	And I press seat map booking view button
	Then I delete the first record under the seat booking details
