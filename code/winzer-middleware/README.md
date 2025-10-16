# Winzer Shopify Middleware #

## What is this repository for? ##

This is a middleware library built / intended for execution in AWS Lambdas.
The functions will be used to integrate Winzer's Shopify storefront with third party and backoffice systems, including:
* Inventory import feed
* Order Export/OMS connector
* Salsify/PIM connector
* etc.

## How do I get set up? ##

* Install Visual Studio 2022 (probably future versions too, but 2022 is required for .NET 6)
    * Optional - Install the .NET 6.0 SDK https://dotnet.microsoft.com/en-us/download/visual-studio-sdks
* Install the AWS Toolkit Visual Studio extension, which you can find here: https://aws.amazon.com/visualstudio/
* Clone this repo.
* Open the solution.
* Build, Run Tests, Start Debugging.

### Alternate Setup with VSCode ###

*Note: I have not been able to get the Mock Lambda Test Tool to work with launch config, but it _should_ be possible.*

* Install VSCode
    * Install CSharp extension `code --install-extension ms-dotnettools.csharp`
* Install the .NET 6.0 SDK https://dotnet.microsoft.com/en-us/download/visual-studio-sdks
* Clone this repo.
* Open the folder in VSCode.
* Install dotnet tool extensions:
```
dotnet tool restore
```
* Useful commands:
    * `dotnet build`
    * `dotnet test`
    * `dotnet-lambda-test-tool-6.0` _doesn't work for me_

### Serverless Framework Setup ###

*This is required for deployments, not development*

*Prerequisites:*
* NodeJS
* AWS Credentials
    * Optional - AWS CLI for managing credentials/profiles: http://docs.aws.amazon.com/cli/latest/userguide/installing.html

Setup
* `npm install -g serverless`
* `cd src/Winzer.ShopifyMiddleware.AWS`
* Useful commands:
    * `serverless invoke local -f inventoryFeedHandler -p events/inventoryFeedHandlerRequest.json`
    * `serverless deploy`
    * `serverless remove`

### Debugging ###

Example payloads are stored in the `events/` folder of the Winzer.ShopifyMiddleware.AWS project.

#### Mock Lambda Test Tool ####

* Set any breakpoints you need and Start Debugging
* Use the Test Function tab
* Select the Function you are testing
* Put a test payload in the Function Input
* Click Execute Function

For Local testing, you can easily set up a local SFTP server with docker:
```
`docker run -p 22:22 -d emberstack/sftp --name sftp`
```
To connect to it, use sftp://demo@localhost with password demo

#### Serverless ####

*local invoke:*
```
cd src/Winzer.ShopifyMiddleware.AWS
serverless invoke local -f inventoryFeedHandler -p events/inventoryFeedHandlerRequest.json
```
*remote invoke:*
```
cd src/Winzer.ShopifyMiddleware.AWS
./build.sh
serverless deploy --stage dev
serverless invoke -f inventoryFeedHandler -p events/inventoryFeedHandlerRequest.json
serverless remove
```

## Deployments ##
This project is intended to deploy to AWS with the Serverless Framework.  There is a serverless.yml file that defines the resources that will deployed, and serverless transforms the config into a CDK script to deploy to AWS.
There are also images that are pushed up to ECR that are used for running long-running jobs on Fargate.

### Automatic Deployments ###
Deployments are integrated into the bitbucket build pipeline as a manual step.  After the tests pass, press the button to run the "Deploy Staging" step.  Staging is pointed at CQL's AWS instance.
To deploy to production, create a new tag with the format YYYY-MM-DD and it will trigger a pipeline run.  After the tests pass, press run the button to run the "Deploy Production" step.  Production is pointed at Winzer's AWS instance.

### AWS Setup ###
Before Deploying the Product Feed or Inventory Feed images to ECR, you must create the ECR repository to hold them.  This is only required the first time.

#### ECR Setup ####
Create the ECR repository to hold the images.
*Create new Repos* (First Time Only):
```
aws --profile Winzer ecr create-repository --repository-name inventory-feed-repository
aws --profile Winzer ecr create-repository --repository-name salsify-inventory-feed-repository
aws --profile Winzer ecr create-repository --repository-name product-feed-repository
```

#### ECS Setup ####
*Create Task Definition*:
In ECS
- Create Cluster if there's not one already (Winzer-shopify-middleware)
- Create Task Definition (e.g. inventory-feed-task)
  - linux
  - fargate/EC2
  - 512 mem (to start)
  - 256 cpu (to start)
- Run task to test

*Create Event Rule*
In Event Bridge, Create new Rule:
AWS Service
 - Your Cluster (Winzer-shopify-middleware)
 - Your Task (e.g. inventory-feed-task)
Network
 - find default VPC
 - find default subnet id
 - find default security group
Use Existing Role
 - AWSServiceRoleForECS

### Manual Lambda Deployment ###

The Lambdas are deployed with Serverless:
```
cd src/Winzer.ShopifyMiddleware.AWS
./build.sh
INVENTORY_ACCESS_TOKEN_VALUE=TOKEN ORDER_ACCESS_TOKEN_VALUE=TOKEN FULFILLMENT_ACCESS_TOKEN_VALUE=TOKEN SFTP_PASSWORD=PASSWORD KWI_SFTP_PASSWORD=PASSWORD CRM_AUTHORIZATION_HEADER=TOKEN serverless deploy --aws-profile Winzer --stage prod
```

### Manual Build and Deploy Inventory Feed Image

*Step 1* - Build image:

```
cd Winzer.ShopifyMiddleware.InventoryFeedConsoleApp
./build.sh
docker build -t inventory-feed -f Dockerfile .
```
to test locally:
```
docker run -it --rm inventory-feed
```

*Step 2* - Push image to Repo(Uri 023486447297.dkr.ecr.us-east-1.amazonaws.com/inventory-feed-repository):

```
docker tag inventory-feed 023486447297.dkr.ecr.us-east-1.amazonaws.com/inventory-feed-repository
aws --profile Winzer ecr get-login-password | docker login --username AWS --password-stdin 023486447297.dkr.ecr.us-east-1.amazonaws.com
docker push 023486447297.dkr.ecr.us-east-1.amazonaws.com/inventory-feed-repository:latest
docker push 023486447297.dkr.ecr.us-east-1.amazonaws.com/inventory-feed-repository:YYYY-MM-DD
```

### Manual Build and Deploy Salsify Inventory Feed Image

*Step 1* - Build image:

```
cd Winzer.ShopifyMiddleware.SalsifyInventoryFeedConsoleApp
./build.sh
docker build -t salsify-inventory-feed -f Dockerfile .
```
to test locally:
```
docker run -it --rm salsify-inventory-feed
```

*Step 2* - Push image to Repo(Uri 023486447297.dkr.ecr.us-east-1.amazonaws.com/salsify-inventory-feed-repository):

```
docker tag salsify-inventory-feed 023486447297.dkr.ecr.us-east-1.amazonaws.com/salsify-inventory-feed-repository
aws --profile Winzer ecr get-login-password | docker login --username AWS --password-stdin 023486447297.dkr.ecr.us-east-1.amazonaws.com
docker push 023486447297.dkr.ecr.us-east-1.amazonaws.com/salsify-inventory-feed-repository:latest
docker push 023486447297.dkr.ecr.us-east-1.amazonaws.com/salsify-inventory-feed-repository:YYYY-MM-DD
```

### Manual Build and Deploy Product Feed Image

*Step 1* - Build image:

```
cd Winzer.ShopifyMiddleware.ProductFeedConsoleApp
./build.sh
docker build -t product-feed -f Dockerfile .
```
to test locally:
```
docker run -it --rm product-feed
```

*Step 2* - Push image to Repo(Uri 023486447297.dkr.ecr.us-east-1.amazonaws.com/product-feed-repository):

```
docker tag product-feed 023486447297.dkr.ecr.us-east-1.amazonaws.com/product-feed-repository
aws --profile Winzer ecr get-login-password | docker login --username AWS --password-stdin 023486447297.dkr.ecr.us-east-1.amazonaws.com
docker push 023486447297.dkr.ecr.us-east-1.amazonaws.com/product-feed-repository:latest
docker push 023486447297.dkr.ecr.us-east-1.amazonaws.com/product-feed-repository:YYYY-MM-DD
```
