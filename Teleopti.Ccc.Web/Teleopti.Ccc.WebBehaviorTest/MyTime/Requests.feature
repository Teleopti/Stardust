Feature: Requests
	In order to review my requests made
	As an agent
	I want to be able to view my requests

Scenario: View request list
	Given I am an agent
	And I have an existing text request
	When I view requests
	Then I should see a requests list

Scenario: See text request
	Given I am an agent
	And I have an existing text request
	When I view requests
	Then I should see my existing text request

Scenario: See absence request
	Given I am an agent
	And I have an existing absence request
	When I view requests
	Then I should see my existing absence request

Scenario: Show created shift trade request
	Given I am an agent
	And I have created a shift trade request
	| Field			| Value				|
	| Subject		| swap with me		|
	When I view requests
	Then I should see my existing shift trade request with subject 'swap with me'


Scenario: Show status of a created shift trade request
	Given I am an agent
	And I have created a shift trade request
	| Field   | Value        |
	| Subject | swap with me |
	| Pending | true         |
	And I am viewing requests
	Then I should see my existing shift trade request with status waiting for other part

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

Scenario: Requests tab
	Given I am an agent
	When I am viewing an application page
	Then I should be able to see requests link

Scenario: No access to requests tab 
	Given I am an agent without access to any requests
	When I am viewing an application page
	Then I should not be able to see requests link

Scenario: No access to requests page
	Given I am an agent without access to any requests
	And I am signed in
	When I navigate to the requests page
	Then I should see an error message

Scenario: No requests
	Given I am an agent
	And I have no existing requests
	When I view requests
	Then I should see a user-friendly message explaining that no requests exists

Scenario: Default sorting
	Given I am an agent
	And I have 2 existing request changed on different times
	When I view requests
	Then I should see that the list is sorted on changed date and time

Scenario: Show single page
	Given I am an agent
	And I have more than one page of requests
	When I view requests
	Then I should only see one page of requests
@Ignore
Scenario: Paging
	Given I am an agent
	And I have more than one page of requests
	When I view requests
	And I scroll down to the bottom of the page
	Then I should see the page fill with the next page of requests

Scenario: Indicate that there are more items to load
	Given I am an agent
	And I have more than one page of requests
	When I view requests
	Then I should see an indication that there are more requests

Scenario: Hide indication that there are more items to load if no more items
	Given I am an agent
	And I have an existing absence request
	When I view requests
	Then I should not see an indication that there are more requests

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