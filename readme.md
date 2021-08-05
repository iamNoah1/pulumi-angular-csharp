# Pulumi Angular C#

This repo contains a walkthrough for getting started with [Pulumi](https://www.pulumi.com) on Azure, deploying an Angular App  utilizing CSharp.

Pulumi is a Infrastructure As Code Framework, that allows to build, deploy and manage modern cloud applications and infrastructure using familiar languages.  

## Prerequisites
* [Install Pulumi](https://www.pulumi.com/docs/get-started/install/)
* [Signup to Pulumi](https://app.pulumi.com/signup)
* Install C# Runtime (.NET Core 3.1 SDK or later)
* Install [Node.js](https://nodejs.org/en/download/)
* Install [Azure CLI](https://docs.microsoft.com/de-de/cli/azure/install-azure-cli) and login using `az login`

## Deploy stack
* `pulumi login`
* `cd webapp && npm run build`
* `cd .. && cd infra && pulumi up`

## Destroy stack
* `pulumi destroy`