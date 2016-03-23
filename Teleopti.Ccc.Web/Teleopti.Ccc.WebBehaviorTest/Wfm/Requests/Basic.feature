﻿@OnlyRunIfEnabled('Wfm_Requests_Basic_35986')
Feature: Requests Basic Operations
	In order to approval/deny agent's requests
	As a resource planner
	I need to have a good overview of all the requests within a certain period

Background: 
	Given I am englishspeaking swede
	And there is a team with
	| Field | Value    |
	| Name  | Red Team |
	And there is a team with
	| Field | Value      |
	| Name  | Green Team |
	And I have a role with
	| Field                  | Value            |
	| Name                   | Resource Planner |
	| Access to wfm requests | true             |
	| Access to everyone     | true             |
	And 'Ashley Andeen' has a person period with 
	| Field      | Value        |
	| Start date | 2015-06-18   |
	| Team      | Red Team   |
	And 'John Smith' has a person period with 
	| Field      | Value      |
	| Start date | 2015-06-18 |
	| Team      | Green Team |
	And 'Ashley Andeen' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	| Update Time  | 2015-09-03 14:00 |
	And 'John Smith' has an existing absence request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	| Update Time | 2015-09-01 14:00 |

Scenario: Display requests	
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	Then I should see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list

Scenario: Sort requests	
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I sort the request list by descending agent name
	Then I should see the request from 'John Smith' before the request from 'Ashley Andeen' in the list

@ignore
Scenario: Not fetch requests when choosing incorrect date range
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	Then I should not see any requests loaded

@ignore
@OnlyRunIfEnabled('Wfm_Requests_People_Search_36294')
Scenario: Find requests by agent team	
	Given 'Ashley Andeen' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	| Update Time  | 2015-09-03 14:00 |
	And 'John Smith' has an existing absence request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	| Update Time | 2015-09-01 14:00 |
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I search with
	| Field        | Value |
	| organization | Red   |
	Then I should see a request from 'Ashley Andeen' in the list
	

@ignore
@OnlyRunIfEnabled('Wfm_Requests_People_Search_36294')
Scenario: Find requests by team that agent belongs to at the request period start date
	Given 'Ashley Andeen' has person period with
	| Field     | Value      |
	| StartDate | 2015-10-01 |
	| Team      | Red Team   |
	And 'Ashley Andeen' has person period with
	| Field     | Value      |
	| StartDate | 2015-10-07 |
	| Team      | Green Team |
	And 'Ashley Andeen' creates an absence request on '2015-10-02'
	| Field     | Value            |
	| StartTime | 2015-10-08 00:00 |
	| EndTime   | 2015-10-10 00:00 |
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-10'
	And I pick the team 'Green Team'
	Then I should see the request from 'Ashley Andeen' in the list
	And I should see the team to be 'Green Team'

@ignore
@OnlyRunIfEnabled('Wfm_Requests_People_Search_36294')
Scenario: Keep team filter value when changing period
	When I view wfm requests
	And I pick the team 'Green Team'
	And I select to load requests from '2015-10-01' to '2015-10-10'
	Then I should see 'Green Team' in the team filter
	
@ignore
@OnlyRunIfEnabled('Wfm_Requests_People_Search_36294')
Scenario: Keep team filter value after refreshing page
	When I view wfm requests
	And I pick the team 'Green Team'	
	And I refresh page
	Then I should see 'Green Team' in the team filter

@ignore
@OnlyRunIfEnabled('Wfm_Requests_People_Search_36294')
Scenario: Find requests for multiple agent search criterias
	Given 'Ashley A' has person period with
	| Field     | Value      |
	| StartDate | 2015-10-01 |
	| Team      | Red Team   |
	And 'Ashley B' has person period with
	| Field     | Value      |
	| StartDate | 2015-10-07 |
	| Team      | Green Team |
	Given 'John A' has person period with
	| Field     | Value      |
	| StartDate | 2015-10-01 |
	| Team      | Red Team   |
	And 'John B' has person period with
	| Field     | Value      |
	| StartDate | 2015-10-07 |
	| Team      | Green Team |
	And 'Ashley A' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	And 'Ashley B' has an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	And 'John A' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	And 'John B' has an existing absence request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-10'
	And I pick the team 'Green Team' 
	And I search agent name 'Ashley Andeen'
	Then I should only see the request from 'Ashley B' in the list


@ignore
@OnlyRunIfEnabled('Wfm_Requests_Performance_36295')
Scenario: Can use paging
	Given the page size is set to '1'
	And there are '10' requests from 'Ashley Andeen' on '2015-10-05'
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-10'
	Then I should see '1' request in the list
	And I should see in total '10' pages 

@ignore
@OnlyRunIfEnabled('Wfm_Requests_Performance_36295')
Scenario: Can change page size
	Given there are '30' requests from 'Ashley Andeen' on '2015-10-05'
	When I view wfm requests
	And I can see the page size to be '20' by default
	And I select to load requests from '2015-10-01' to '2015-10-10'
	And I change page size to '30'
	Then I should see '30' request in the list
	And I should see in total '1' pages 


@ignore
@OnlyRunIfEnabled('Wfm_Requests_Performance_36295')
Scenario: Can view different page
	Given the page size is set to '1'
	And 'Ashley Andeen' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-02 10:00 |
	| End Time  | 2015-10-03 14:00 |
	And 'John Smith' has an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-10' order by ascending period start time
	And I see the request from 'Asheley' in the list	
	And I select to view the last page
	Then I see the request from 'John Smith' in the list	

@OnlyRunIfEnabled('Wfm_Requests_ApproveDeny_36297')
Scenario: Can approve requests
	Given 'Ashley Andeen' has an existing text request with
	| Field       | Value            |
	| StartTime   | 2016-01-03 10:00 |
	| End Time    | 2016-01-03 14:00 |
	| Update Time | 2016-01-01 1:00  |
	| Status      | Pending          |
	When I view wfm requests
	And I select to load requests from '2016-01-01' to '2016-01-07'
	And I approve all requests that I see
	Then I should see request for 'Ashley Andeen' approved

@OnlyRunIfEnabled('Wfm_Requests_ApproveDeny_36297')
Scenario: Can approve waitlisted requests
	Given 'John Smith' has an open workflow control set with absence request waitlisting enabled
	And 'John Smith' has an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-3-01 10:00 |
	| End Time  | 2015-3-01 14:00 |
	| Status    | AutoDenied       |
	| Absence   | Vacation         |
	When I view wfm requests
	And I select to load requests from '2015-03-01' to '2015-03-02'
	And I approve all requests that I see
	Then I should see request for 'John Smith' approved

@OnlyRunIfEnabled('Wfm_Requests_ApproveDeny_36297')
Scenario: Can deny waitlisted requests
	Given 'John Smith' has an open workflow control set with absence request waitlisting enabled
	And 'John Smith' has an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-3-01 10:00 |
	| End Time  | 2015-3-01 14:00 |
	| Status    | AutoDenied       |
	| Absence   | Vacation         |
	When I view wfm requests
	And I select to load requests from '2015-03-01' to '2015-03-02'
	And I deny all requests that I see
	Then I should see request for 'John Smith' denied

@ignore
@OnlyRunIfEnabled('Wfm_Requests_ApproveDeny_36297')
Scenario: Can deny requests
	Given 'Ashley Andeen' has an existing text request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	| Update Time | 2015-09-05 1:00  |
	| Status      | Pending          |
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-10'
	And I deny requests from 'Ashley Andeen'
	Then I should see the request from 'Ashley Andeen' with status 'Denied'


@ignore
@OnlyRunIfEnabled('Wfm_Requests_ApproveReject_36297')
Scenario: Can approve multiple requests
	Given 'Ashley Andeen' has an existing text request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	| Update Time | 2015-09-05 1:00  |
	| Status      | Pending          |
	And 'John Smith' has an existing absence request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	| Update Time | 2015-09-02 14:00 |
	| Status      | Pending          |
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-10'
	And I approve requests from 'Ashley Andeen' and 'John Smith' at the same time
	Then I should see the request from 'Ashley Andeen' with status 'Approved'
	And I should see the request from 'John Smith' with status 'Approved'

@ignore
@OnlyRunIfEnabled('Wfm_Requests_ApproveReject_36297')
Scenario: Cannot approve or deny non-pending request
	Given 'Ashley Andeen' has an existing text request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	| Update Time | 2015-09-05 1:00  |
	| Status      | Approved         |
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-10'
	And I deny requests from 'Ashley Andeen'
	Then I should see the request from 'Ashley Andeen' with status 'Approved'

@ignore
@OnlyRunIfEnabled('Wfm_Requests_ApproveReject_36297')
Scenario: Cannot approve or deny obsolete request
	Given 'Ashley Andeen' has an existing text request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	| Update Time | 2015-09-05 1:00  |
	| Status      | Pending          |
	And today is '2015-12-01'
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-10'
	And I deny requests from 'Ashley Andeen'
	Then I should see the request from 'Ashley Andeen' with status 'Pending'

@ignore
@OnlyRunIfEnabled('Wfm_Requests_ApproveReject_36297')
Scenario: Refresh the request list with status filter after approving or denying request
	Given 'Ashley Andeen' has an existing text request with
	| Field       | Value            |
	| StartTime   | 2015-10-03 10:00 |
	| End Time    | 2015-10-03 14:00 |
	| Update Time | 2015-09-05 1:00  |
	| Status      | Pending          |
	When I view wfm requests	
	And I select to load requests from '2015-10-01' to '2015-10-10'
	And I choose to view 'Pending' requests
	And I approve requests from 'Ashley Andeen'
	Then I should not see the request from 'Ashley Andeen'
	
@ignore
Scenario: View request details
	Given 'Ashley Andeen' has an existing text request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	| Update Time  | 2015-11-03 14:00 |
	And 'John Smith' has an existing absence request with
	| Field     | Value            |
	| StartTime | 2015-10-03 10:00 |
	| End Time  | 2015-10-03 14:00 |
	| Update Time  | 2015-11-01 14:00 |
	And I view wfm requests
	When I expand the request from 'Ashley Andeen'
	Then I should see detailed text request from 'Ashley Andeen'

