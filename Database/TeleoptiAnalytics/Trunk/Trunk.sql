
--------------------------------
-- Robin: Empty the queue tables to make sure that duplicates that were created are gone.
--------------------------------
truncate table Queue.Messages
truncate table Queue.Queues
truncate table Queue.SubscriptionStorage