# Message Services 
Consumers using the generic host provided by ASP.NET Core.

# Getting Started
* Start Docker
* Open CMD and navigate to the path to "docker-compose.yml"
* Run "docker compose up", which creates a docker container which has a RabbitMQ service, and start it.
* Run this Console App project.

# RabbitMQ binding workflow for publish
Here is an example of how binding works in RabbitMQ:

Message contract (ISubmitOrder) --> Exchange (ISubmitOrder) --> Exchange (submit-order) --> Queue (submit-order) --> Consumer (dynamic name per instance)

# Competing Consumers
It is possible to start multiple instances of this consumer, and the multiple instances shall all consume the messages in the queue. 
RabbitMQ will handle the load balancing and distribute messages to connected services. Nice!

# Message timeout
1. Command type messages that create/update orders should not timeout, since they contain sensitive information.
1. Query type messages that retrieve order state can timeout, since it won't hurt...

# Application Insights
1. Mass Transit internally uses Microsoft Diagnostic Source library for telemetry data, such as events, sagas, consumers, handlers. These telemetry events create activities using Diagnositc Source and are chained together through a series of activity ids in the message header and are sent to Application Insight.

