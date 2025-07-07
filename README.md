# EPSLT Challenge

# DiscountManager WebApi

A .NET 8 WebApi project with WebSocket support, ready to run with Docker Compose!

## Prerequisites

- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/)
- [Postman](https://www.postman.com/downloads/) (or any WebSocket client)

## How to Run Locally

1. **Clone the Repository**

   ```bash
   git clone https://github.com/BrunoBSCCardoso/EPSLTChallenge.git 
   cd EPSLTChallenge

2. **Build and Start with Docker Compose**

   ```bash
   docker compose up --build
   ```
   This will:
   
   . Build the project image
   
   . Run the container exposing port 8080

3. **Check if the Application is Running**

   You should see in the terminal:
      ```nginx
      Now listening on: http://0.0.0.0:8080
      ````
4. ***Testing the WebSocket with Postman***
   
   . Open Postman

   . Click on New > WebSocket Request

   . Use the following URL:
   ```bash
   ws://localhost:8080/ws
   ```
   . Click Connect
   . Send messages in the format your backend expects, for example:
   ```json
   {
     "Action": "GenerateCodes",
     "Payload": {
       "Count": 1,
       "Length": 8
     }
   }
   ```
   . And to consume the server, for example:
   ```json
   {
     "Action": "UseCode",
     "Payload": {
       "Code": "10QFTLPA"
     }
   }
   ```
## Project Structure
   DiscountManager.WebApi/ - Main API and WebSocket handler
   
   DiscountManager.Application/ - Application services and interfaces
   
   DiscountManager.Domain/ - Domain entities and contracts
   
   DiscountManager.Infrastructure/ - Persistence and logging infrastructure

## Notes
   Storage files are mapped to the local Storage/ folder (via Docker volume).
   
   The default exposed port is 8080 (HTTP).
   
   If you want to change the port, edit docker-compose.yml and the ASPNETCORE_URLS environment variable.

## Troubleshooting
   If you cannot connect to the WebSocket:
   
   Make sure the container is running: docker ps
   
   Check if any firewall is blocking port 8080

   Inspect the container logs: docker logs <container-name>

## Author
- [Bruno Cardoso](https://www.linkedin.com/in/bruno-cardoso-737088154/)
- [CV](https://drive.google.com/file/d/11XTCwZ3DdftHNVVZe5QL6XK8cNywijSH/view?usp=drive_link)

