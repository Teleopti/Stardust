@WFM
Feature: Filtering
	In order to approve certain requests before other.
	As a manager
	I need to be able to filter out requests what I want.

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
	| Field      | Value      |
	| Start date | 2015-06-18 |
	| Team       | Red Team   |
	And 'John Smith' has a person period with 
	| Field      | Value      |
	| Start date | 2015-06-18 |
	| Team       | Green Team |
	And 'Bill Gates' has a person period with 
	| Field      | Value      |
	| Start date | 2015-06-18 |
	| Team       | Green Team |
	And 'Ashley Andeen' has an existing text request with
	| Field        | Value                                 |
	| StartTime    | 2015-10-03 10:00                      |
	| End Time     | 2015-10-03 14:00                      |
	| Update Time  | 2015-09-03 14:00                      |
	| Absence Type | Illness                               |
	| Status       | New                                   |
	| Subject      | Subject - Illness request from Ashley |
	| Message      | Message - Illness request from Ashley |
	And 'Bill Gates' has an existing text request with
	| Field        | Value                                |
	| StartTime    | 2015-10-04 10:00                     |
	| End Time     | 2015-10-04 14:00                     |
	| Update Time  | 2015-09-04 14:00                     |
	| Absence Type | Training                             |
	| Status       | Denied                               |
	| Subject      | Subject - Training request from Bill |
	| Message      | Message - Training request from Bill |
	And 'John Smith' has an existing absence request with
	| Field        | Value                               |
	| StartTime    | 2015-10-03 10:00                    |
	| End Time     | 2015-10-03 14:00                    |
	| Update Time  | 2015-09-01 14:00                    |
	| Absence Type | Holiday                             |
	| Status       | Pending                             |
	| Subject      | Subject - Holiday request from John |
	| Message      | Message - Holiday request from John |

@ignore
Scenario: Could filter requests by Absence Type
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field        | Value   |
		| Absence Type | Illness |
	Then I should see a request from 'Ashley Andeen' in the list
	And I should not see a request from 'John Smith' in the list
	And I should not see a request from 'Bill Gates' in the list

@ignore
Scenario: Could filter requests by Status
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field  | Value   |
		| Status | Pending |
	Then I should not see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should not see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could filter requests by Subject
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field   | Value  |
		| Subject | Ashley |
	Then I should see a request from 'Ashley Andeen' in the list
	And I should not see a request from 'John Smith' in the list
	And I should not see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could filter requests by Message
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field   | Value  |
		| Message | Ashley |
	Then I should see a request from 'Ashley Andeen' in the list
	And I should not see a request from 'John Smith' in the list
	And I should not see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could reset filter of Absence Type
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field        | Value   |
		| Absence Type | Illness |
	And I reset filter for "Absence Type"
	Then I should see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could reset filter of Status
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field  | Value   |
		| Status | Pending |
	And I reset filter for "Status"
	Then I should see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could reset filter of Subject
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field   | Value  |
		| Subject | Ashley |
	And I reset filter for "Subject"
	Then I should see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could reset filter of Message
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field   | Value  |
		| Message | Ashley |
	And I reset filter for "Message"
	Then I should see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could filter requests by Multiple Absence Type
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field        | Value            |
		| Absence Type | Illness Training |
	Then I should see a request from 'Ashley Andeen' in the list
	And I should not see a request from 'John Smith' in the list
	And I should see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could filter requests by Multiple Status
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with
		| Field  | Value       |
		| Status | New Pending |
	Then I should see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should not see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could filter requests by Subject with Multiple words
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with 
		| Field   | Value        |
		| Subject | Illness John |
	Then I should see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should not see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could filter requests by Message with Multiple words
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with 
		| Field   | Value        |
		| Message | Holiday Bill |
	Then I should not see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could filter requests by Multiple filters
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with 
		| Field   | Value        |
		| Status  | Denied       |
		| Message | Holiday Bill |
	Then I should not see a request from 'Ashley Andeen' in the list
	And I should not see a request from 'John Smith' in the list
	And I should see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Could reset all filters
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with 
		| Field   | Value        |
		| Status  | Denied       |
		| Message | Holiday Bill |
	And I reset all filters
	Then I should see a request from 'Ashley Andeen' in the list
	And I should see a request from 'John Smith' in the list
	And I should see a request from 'Bill Gates' in the list
	
@ignore
Scenario: Notify user that no result for current filter
	When I view wfm requests
	And I select to load requests from '2015-10-01' to '2015-10-04'
	And I set filter with 
		| Field   | Value        |
		| Status  | New          |
		| Message | Holiday Bill |
	Then I should see a notification that no result match the filters
