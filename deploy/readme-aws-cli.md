# Using AWS CLI to Deploy and Test Lambda Function in AWS
These steps are for the more advanced user who wants to deploy this function with the AWS CLI. It's not necessary though and is partly here for useful information about AWS CLI and Lambda.

# Pre-Reqs and AWS CLI Configuration
## AWS CLI
Follow the [Pre-requisites to use the AWS CLI 2](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-prereqs.html) which includes
- signing up to AWS account
- creating an IAM user account
- creating an access key
- [downloading the CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)

## AWS Config
Add the following sections to the AWS config and credentials files which are in your user folder (typically `C:\Users\yourname\.aws` on window).

Substitute `your_profile` with profile name of your choosing. I can be anything you like to idenfity this AWS account configuration.

By default this profile will deploy to Ireland region, you can change to your preferred, it doesn't really make a difference.
### .aws\config
```
[profile your_profile]
region = eu-west-1
output = json
```
### .aws\credentials
```
[your_profile]
aws_access_key_id = YOUR_IAM_ACCESS_KEY
aws_secret_access_key = YOUR_IAM_SECRET_ACCESS_KEY
region = eu-west-1
```

## Set default AWS CLI profile
For all CMD, Powershell or WSL sessions, set the default profile to `your_profile`.
```
set AWS_DEFAULT_PROFILE=your_profile
```
# Deploy Lambda Function in AWS
This is the CLI way of deploying AWS. The preference is to use CloudFormation.
### Create a role and attach basic execution policy to it
```
aws iam create-role --role-name urma-deal-genie --assume-role-policy-document file://./trust-policy.json
aws iam attach-role-policy --role-name urma-deal-genie --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole
```
## Create the Lambda Function
You can skip this and deploy the zip file instead.

See [README-CREATE-LAMBDA.md](./README-CREATE-LAMBDA.md) for reference, but this only needs doing if you want to modify/build the function yourself. 
# Build and deploy using lambda role (created above)
## ***TODO this really needs the AWS CLI deploy zip instead of this dotnet call***
```
dotnet lambda deploy-function UrmaAutoDealUpdater –function-role urma-deal-genie
```

# Set API key
This uploads the 3Commas API key and secret to the AWS Lambda function's environment variables.
```
aws lambda update-function-configuration --function-name UrmaAutoDealUpdater --environment file://apikey.json --query "Environment"
```
# Test the deployed Lambda function
This tests the function without updating any deals.
```
aws lambda invoke --function-name UrmaAutoDealUpdater --payload fileb://test-payload-update-false.json output.txt
```
This executes the function and updates any deals that need updating.
```
aws lambda invoke --function-name UrmaAutoDealUpdater --payload fileb://test-payload-update-true.json output.txt
```
