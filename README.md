# Tests

This repository is used to host integration tests and overall test artefacts that span all Bürokratt components.

[![Tests CI Pipeline](https://github.com/buerokratt/Tests/actions/workflows/ci-pullrequest-main.yml/badge.svg)](https://github.com/buerokratt/Tests/actions/workflows/ci-pullrequest-main.yml)

## Running tests locally with Docker Compose

As a developer, you may want to run the tests in this repository on your local machine. This is simple to do using Docker and Docker Compose.

### 1 Clone repositories

In order to run the tests in this repository, please clone each of the following repositories to a working folder (for example `c:\git\buerokratt`). We'll use the reference `[working folder`] to describe this location for the rest of the guide.

1. Clone the `main` branch of https://github.com/buerokratt/Mock-Bot to `[working folder]/Mock-Bot`
2. Clone the `main` branch of https://github.com/buerokratt/Mock-Classifier to `[working folder]/Mock-Classifier`
3. Clone the `main` branch of https://github.com/buerokratt/DMR to `[working folder]/DMR`
4. Clone the `main` branch of https://github.com/buerokratt/CentOps to  `[working folder]/CentOps`
5. Clone the `main` branch of https://github.com/buerokratt/Tests to  `[working folder]/Tests`

### 2 Install .net CLI

The tests are written using .net so you need the .net CLI to run them.

1. Install the .net 6.0 (or newer) CLI from [Install .NET on Windows, Linux, and macOS](https://docs.microsoft.com/en-us/dotnet/core/install/)

### 3 Install Docker

When running tests locally, you will use Docker Desktop.

1. Install Docker Desktop from https://www.docker.com/products/docker-desktop/

### 4 Docker Network Bridge

In order to run the various services in containers concurrently, you will use Docker Compose. In order for the containers to be able to communicate with each other, you will need to create a Docker network bridge.

1. Open a terminal and navigating to `[working folder]`
2. Execute `docker network create -d bridge my-network`

### 5 Docker Compose

To run the containers, you will need a Docker Compose file.

1. Open a terminal and navigate cd to `[working folder]/Tests`
3. Execute `docker compose up --build` to build and run the containers specified in the `docker-compose.yml` file. This has completed when you see "created" next to each of the 4 containers and you see logs in the terminal

### 6 Run Test

You can run the test in a terminal or Visual Studio. These steps are for a terminal but you can get a more detailed view by running the tests in test Explorer in Visual Studio.

> The test will use the version of the code which is on your local device for each repository/container. Generally, you'll want to make sure you have pulled the latest changes on the `main` branch before running the test, although there may be scenarios where you want to test from other branches.

1. Open a terminal and navigate to `[working folder]/Tests/src/`
2. Execute `dotnet test`
3. If successful, you will see that 2 tests from "Tests.IntegrationTests.dll" has "Passed!" in the terminal.

## Running tests locally against Azure hosted environment

You may want to run this test against deployed component in Azure. This can be usefull to make sure that the deployed components have the correct settings and can communicate correctly.

In order to do this follow steps 1 and 2 from Running tests locally with Docker Compose and then follow these steps:

### 1 Configure secrets.json

In order to target a deployed set of resources, the settings in `appsettings.json` need to be over-written with values that correspond to the deployed components. It is not good practice to edit `appsettings.json` because that file is under source control and you may inadvertently commit secrets to Git. Instead you can use .net user secrets to create a local secrets.json which over-rides the settings in `appsettings.json`. See https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0

1. Open a terminal and navigate to `[working folder]/Tests/src/`
2. Execute `dotnet user-secrets init`
3. Manually add equivilent secrets from the Azure-hosted components for each key/value pair in `[working folder]/Tests/src/Tests.IntegrationTests/appsettings.json` using this command `dotnet user-secrets set "setting key" "setting value"`
   1. As an alternative, you can open `secrets.json` for this project in a text editor and add the secrets that way. By default, `secrets.json` will be located in `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json` for Windows or `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json` for Linux/Mac. You can find the value of `<user_secrets_id>` by opening the `[working folder]/Tests/src/Tests.IntegrationTests/Tests.IntegrationTests.csproj` in a text editor.

### 2 Run Test

You can run the test in a terminal or Visual Studio. These steps are for a terminal but you can get a more detailed view by running the tests in test Explorer in Visual Studio.

> The test will use the version of the code which is on your local device for each repository/container. Generally, you'll want to make sure you have pulled the latest changes on the `main` branch before running the test, although there may be scenarios where you want to test from other branches.

1. Open a terminal and navigate to `[working folder]/Tests/src/`
2. Execute `dotnet test`
3. If successful, you will see that 2 tests from "Tests.IntegrationTests.dll" has "Passed!" in the terminal.
