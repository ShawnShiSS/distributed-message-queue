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
