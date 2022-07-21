# Tests

This repository is used to host integration tests and overall test artefacts that span all Bürokratt components.

[![Tests CI Pipeline](https://github.com/buerokratt/Tests/actions/workflows/ci-pullrequest-main.yml/badge.svg)](https://github.com/buerokratt/Tests/actions/workflows/ci-pullrequest-main.yml)

## Running tests locally

As a developer, you may want to run the tests in this repository on your local machine. This is simple to do using Docker and Docker Compose.

### Clone repositories

In order to run the tests in this repository, please clone each of the following repositories to a working folder (for example `c:\git\buerokratt`). We'll use the reference `[working folder]` to describe this location for the rest of the guide.

1. Clone the `main` branch of https://github.com/buerokratt/Mock-Bot to `[working folder]/Mock-Bot`
2. Clone the `main` branch of https://github.com/buerokratt/Mock-Classifier to `[working folder]/Mock-Classifier`
3. Clone the `main` branch of https://github.com/buerokratt/DMR to `[working folder]/DMR`
4. Clone the `main` branch of https://github.com/buerokratt/CentOps to  `[working folder]/CentOps`
5. Clone the `main` branch of https://github.com/buerokratt/Tests to  `[working folder]/Tests`

### Install .net CLI

The tests are written using .net so you need the .net CLI to run them.

1. Install the .net 6.0 (or newer) CLI from [Install .NET on Windows, Linux, and macOS](https://docs.microsoft.com/en-us/dotnet/core/install/)

### Install Docker

When running tests locally, you will use Docker Desktop.

1. Install Docker Desktop from https://www.docker.com/products/docker-desktop/

### Docker Network Bridge

In order to run the various services in containers concurrently, you will use Docker Compose. In order for the containers to be able to communicate with each other, you will create a Docker network bridge.

1. Open a terminal and navigating to `[working folder]`
2. Execute `docker network create -d bridge my-network`

### Docker Compose

To run the containers, you will need a Docker Compose file.

1. Open a terminal and navigate cd to `[working folder]/Tests`
3. Execute `docker compose up --build` to build and run the containers specified in the `docker-compose.yml` file. This has completed when you see "created" next to each of the 4 containers and you see logs in the terminal

### Run Test

You can run the test in a terminal or Visual Studio. These steps are for a terminal but you can get a more detailed view by running the tests in test Explorer in Visual Studio.

> The test will use the version of the code which is on your local device for each repository/container. Generally, you'll want to make sure you have pulled the latest changes on the `main` branch before running the test, although there may be scenarios where you want to test from other branches.

1. Open a terminal and navigate to `[working folder]/Tests/src/`
2. Execute `dotnet test`
3. If successful, you will see that 1 test from "Tests.IntegrationTests.dll" has "Passed!" in the terminal.
