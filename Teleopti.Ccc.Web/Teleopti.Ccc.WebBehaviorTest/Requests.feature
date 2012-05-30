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

Scenario: Requests tab
	Given I am an agent
	When I sign in
	Then I should be able to see requests link

Scenario: No access to requests tab
	Given I am an agent without access to any requests
	When I sign in
	Then I should not be able to see requests link

Scenario: No access to requests page
	Given I am an agent without access to any requests
	When I sign in
	And I navigate to the requests page
	Then I should see an error message

Scenario: No requests
	Given I am an agent
	And I have no existing requests
	When I view requests
	Then I should see a user-friendly message explaining I dont have anything to view

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

Scenario: Paging
	Given I am an agent
	And I have more than one page of requests
	When I view requests
	And I scroll down to the bottom of the page
	Then I should see the page fill with the next page of requests