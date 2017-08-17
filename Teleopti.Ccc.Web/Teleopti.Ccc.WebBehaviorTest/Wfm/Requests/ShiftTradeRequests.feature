@WFM
Feature: Shift Trade Requests
	In order to approval/deny agent's shift trade requests
	As a resource planner
	I need to have a good overview of all the shift trade requests within a certain period

Background:
	Given I am englishspeaking swede
	And there is a team with
	| Field | Value    |
	| Name  | Red Team |
	And there is a team with
	| Field | Value      |
	| Name  | Green Team |
	And there is a workflow control set with
	| Field                            | Value                                     |
	| Name                             | Trade from tomorrow until 30 days forward |
	| Schedule published to date       | 2050-05-16                                |
	| Shift Trade sliding period start | 1                                         |
	| Shift Trade sliding period end   | 30                                        |
	And I have a role with
	| Field                  | Value            |
	| Name                   | Resource Planner |
	| Access to wfm requests | true             |
	| Access to everyone     | true             |
	And there is a shift category with
	| Field     | Value |
	| Name      | Day   |
	| ShortName | DA    |
	| Color     | Green |
	And there are activities
	| Name     | Color    |
	| Phone    | Green    |
	| Sales    | Red      |
	And 'I' has the workflow control set 'Trade from tomorrow until 30 days forward'
	And 'John Smith' has the workflow control set 'Trade from tomorrow until 30 days forward'
	And 'Pence H' has the workflow control set 'Trade from tomorrow until 30 days forward'
	And 'I' has a person period with 
	| Field      | Value      |
	| Start date | 2016-05-16 |
	| Team       | Red Team   |
	And 'John Smith' has a person period with 
	| Field      | Value      |
	| Start date | 2016-05-16 |
	| Team       | Green Team |
	And 'Pence H' has a person period with 
	| Field      | Value      |
	| Start date | 2016-05-16 |
	| Team       | Green Team |
	And I have created a shift trade request
	| Field    | Value      |
	| To       | John Smith |
	| DateTo   | 2016-05-19 |
	| DateFrom | 2016-05-19 |
	| Pending  | True       |
	| Accepted | True       |
	And I have created a shift trade request
	| Field    | Value      |
	| To       | Pence H    |
	| DateTo   | 2016-05-27 |
	| DateFrom | 2016-05-22 |
	| Pending  | True       |
	| Accepted | True       |
	And I has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-05-19 09:00 |
	| EndTime          | 2016-05-19 17:00 |
	And I has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-05-23 09:00 |
	| EndTime          | 2016-05-23 17:00 |
	And John Smith has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Phone            |
	| StartTime        | 2016-05-19 14:00 |
	| EndTime          | 2016-05-19 22:00 |
	And Pence H has a shift with
	| Field            | Value            |
	| Shift category   | Day              |
	| Activity         | Sales            |
	| StartTime        | 2016-05-23 15:30 |
	| EndTime          | 2016-05-23 23:30 |
@ignore
Scenario: View shift trade requests 
	When I view wfm requests
	And I select to go to shift trade requests view
	And I select to load requests from '2016-05-01' to '2016-05-30'
	Then I should see a shift request from 'John Smith' in the list
	And I should see a shift request from 'Pence H' in the list

@ignore
Scenario: View schedule detail
	When I view wfm requests
	And I select to go to shift trade requests view
	And I select to load requests from '2016-05-17' to '2016-05-24'
	When I click the shift trade schedule day
	Then I should see schedule detail

 @ignore
Scenario: Search for shift trade
	When I view wfm requests
	And I select to go to shift trade requests view
	And I search with
	| Field | Value |
	| name  | john  |
	Then I should see a request from 'John Smith' in the list

@ignore
Scenario: Show shift trade requests within a short date range
	When I view wfm requests
	And I select to go to shift trade requests view
	And I select to load requests from '2016-05-18' to '2016-05-19'
	Then I should see a request from 'I' in the list with
	| Field    | Value      |
	| To       | John Smith |
	| DateTo   | 2016-05-19 |
	| DateFrom | 2016-05-19 |
	And I should not see a request from 'I' to 'Pence H' in the list

@ignore
Scenario: Show shift trade requests within a long date range
	When I view wfm requests
	And I select to go to shift trade requests view
	And I select to load requests from '2016-05-18' to '2016-05-22'
	Then I should see a request from 'I' in the list with
	| Field    | Value      |
	| To       | John Smith |
	| DateTo   | 2016-05-19 |
	| DateFrom | 2016-05-19 |
	And I should see a request from 'I' in the list with
	| Field    | Value      |
	| To       | Pence H    |
	| DateTo   | 2016-05-27 |
	| DateFrom | 2016-05-22 |

@ignore
Scenario: First day of week should differ from locale
	Given I am a New Zealander
	When I view wfm requests
	And I select to go to shift trade requests view
	And I select to load requests from '2016-05-18' to '2016-05-22'
	Then I should see the first day to be '2016-05-15'