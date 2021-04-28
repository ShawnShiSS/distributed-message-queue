# State machines
A collection of state machine engines, aka sagas, that allows us to track the state of an event.

## Terminology
1. A state machine defines the behaviour.
1. A state machine instance contains the actual data portion of the saga and the instance gets persisted to disk. Each state machine instance has a correlation id. The instance is correlated to the messages by the correlation id. This allows multiple messages to be correlated to a Saga state in the Saga repository. 

Related topics:
- optimistic concurrency
- pessimistic concurrency