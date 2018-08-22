﻿Feature: Requests
	In order to review my requests made
	As an agent
	I want to be able to view my requests

Scenario: View request list
	Given I am an agent
	And I have an existing text request
	When I view requests
	Then I should see a requests list

	@NotKeyExample
Scenario: See text request
	Given I am an agent
	And I have an existing text request
	When I view requests
	Then I should see my existing text request

	@NotKeyExample
Scenario: See absence request
	Given I am an agent
	And I have an existing absence request
	When I view requests
	Then I should see my existing absence request

	@NotKeyExample
Scenario: Show created shift trade request
	Given I am an agent
	And I have created a shift trade request
	| Field			| Value				|
	| Subject		| swap with me		|
	When I view requests
	Then I should see my existing shift trade request with subject 'swap with me'

	@NotKeyExample
Scenario: Show created overtime request
	Given I am an agent
	And I have created an overtime request with subject 'test overtime request'
	When I am viewing requests
	Then I should see my existing overtime request with subject 'test overtime request'

	@NotKeyExample
Scenario: Show status of a created shift trade request
	Given I am an agent
	And I have created a shift trade request
	| Field   | Value        |
	| Subject | swap with me |
	| Pending | true         |
	And I am viewing requests
	Then I should see my existing shift trade request with status waiting for other part

	@NotKeyExample
Scenario: Show status of a recieved shift trade request
	Given I am an agent
	And I have received a shift trade request
	| Field   | Value         |
	| Subject | swap with me  |
	| From    | Ashley Andeen |
	| Pending | true          |
	And I am viewing requests
	Then I should see my existing shift trade request with status waiting for your approval

Scenario: Show received shift trade request
	Given I am an agent
	And I have received a shift trade request
	| Field			| Value         |
	| Subject		| swap with me  |
	| From			| Ashley Andeen |
	When I view requests
	Then I should see my existing shift trade request with subject 'swap with me'

	@NotKeyExample
Scenario: Requests tab
	Given I am an agent
	When I am viewing an application page
	Then I should be able to see requests link

	@NotKeyExample
Scenario: No access to requests tab 
	Given I have a role with
         | Field                          | Value |
         | Access To Text Requests        | False |
         | Access To Absence Requests     | False |
         | Access To Shift Trade Requests | False |
         | Access To Overtime Requests    | False |
	When I am viewing an application page
	Then I should not be able to see requests link

	@NotKeyExample
Scenario: No access to requests page
	Given I have a role with
         | Field                          | Value |
         | Access To Text Requests        | False |
         | Access To Absence Requests     | False |
         | Access To Shift Trade Requests | False |
		 | Access To Overtime Requests    | False |
	And I am signed in
	When I navigate to the requests page
	Then I should see an error message

	@NotKeyExample
Scenario: No requests
	Given I am an agent
	And I have no existing requests
	When I view requests
	Then I should see a user-friendly message explaining that no requests exists

	@NotKeyExample
Scenario: Default sorting
	Given I am an agent
	And I have 2 existing request changed on different times
	When I view requests
	Then I should see that the list is sorted on changed date and time

	@NotKeyExample
Scenario: Show single page
	Given I am an agent
	And I have more than one page of requests
	When I view requests
	Then I should only see one page of requests

	@NotKeyExample
Scenario: Indicate that there are more items to load
	Given I am an agent
	And I have more than one page of requests
	When I view requests
	Then I should see an indication that there are more requests

	@NotKeyExample
Scenario: Hide indication that there are more items to load if no more items
	Given I am an agent
	And I have an existing absence request
	When I view requests
	Then I should not see an indication that there are more requests

	@NotKeyExample
Scenario: Show auto denied shift trade request for sender
	Given I am an agent
	And I have created a shift trade request
	| Field      | Value         |
	| Subject    | swap with me  |
	| To         | Ashley Andeen |
	| AutoDenied | true          |
	And I am viewing requests
	Then I should see my existing shift trade request with subject 'swap with me'

Scenario: Do not show auto denied shift trade request for recipient
	Given I am an agent
	And I have received a shift trade request
	| Field      | Value         |
	| Subject    | swap with me  |
	| From       | Ashley Andeen |
	| AutoDenied | true          |
	And I am viewing requests
	Then I should not see any request