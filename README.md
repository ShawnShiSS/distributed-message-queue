# Distributed Message Queue
A distributed message queue system that handle messages asynchronously.

## System Diagram
<img src="https://github.com/ShawnShiSS/distributed-message-queue/blob/main/Solution%20Items/System%20Diagram.jpg" width="100%">

## Projects
* DMQ.API
* DMQ.MessageServices, which is a hosted service to consume order messages, and serve as a saga state machine backed by either Redis or MongoDB
* Warehouse.MessageServices: which is hosted service to consume warehouse messages and also serve as a saga state machine backed by either Redis or MongoDB

## Resources
* [Distributed Messaging System — Setup Consumers and RabbitMQ with Docker in ASP.NET Core](https://shawn-shi.medium.com/distributed-messaging-system-setup-consumers-and-rabbitmq-with-docker-in-asp-net-core-6133ce666268)
* [Use a Single Docker Project to Host All Your Database Containers](https://shawn-shi.medium.com/use-a-single-docker-project-to-host-all-your-database-containers-9b85f2a5c2c3)

## This project is work in progress. Please watch for upcoming updates.
