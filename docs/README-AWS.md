# Run in the Cloud using AWS Lambda Function
It's actually easier than you think, and my CloudFormation template makes it much easier to install in AWS now. In summary this is what you do:
1. Create a [new account in  AWS (free)](https://aws.amazon.com/free/)
1. Create an “S3 bucket” to upload files to (easy)
1. Upload 3 files from my release:
    - `UrmaDealGenieAWS-{ver}.zip` release package file
    - `dealrules.json` file, modified with your rules
    - `deploy-urmadealgenie.yml` CloudFormation template file which tells AWS what how to deploy Urma Deal Genie
1. Deploy Urma Deal Genie with my CloudFormation template file (it's really easy), specifying your 3c API key & secret

And that's it. Detailed steps below, and feel free to DM me on Discord if you get stuck or just want some guidance. I'm always happy to help people set this up.

## 1. Create an AWS Account
Amazon Web Services (AWS) has a free tier that allows several services to run free of charge. One of those free services is "Lambda functions". These are serverless functions that allow code to run in the cloud in your own AWS account, without being logged in or needing a PC running 24/7. 
1. Go to [AWS Free Tier](https://aws.amazon.com/free/) and sign up
1. You will need a credit card, but this is to prevent fraud and bots (see [AWS Free Tier FAQs](https://aws.amazon.com/free/registration-faqs/) )
1. Make sure you secure your account with 2FA

## 2. Create an S3 Bucket
An S3 bucket is like cloud file storage. You create a "bucket" which is a bit like a folder, and you upload files to it. Each bucket has a globally unique name (whatever you name it, there won't be another with the same name in the world).
1. Go to [S3 Management Console](https://s3.console.aws.amazon.com) and click "Create bucket"
1. Give the bucket a name (e.g. `urmadealgenie-somerandomnumbers`)
1. Choose a region (doesn't really matter, but just remember which region you choose, e.g. `eu-west-2` London)
1. Leave everything else default (it will be private and secure) and click "Create bucket" 

## 3. Upload Urma Deal Genie files to S3 Bucket
An AWS Lambda function is code that runs in the cloud, but first you need to download Urma Deal Genie files and upload them to your S3 bucket.
1. Download the Urma Deal Genie files from my [GitHub releases](https://github.com/UrmaGurd/UrmaDealGenie/releases) to your PC:
    - `UrmaDealGenieAWS-x.x.zip`
    - `dealrules.json`
    - `deploy-urmadealgenie.yml`
    </br>**NOTE:** Make sure you download the latest "Urma Deal Genie AWS Lambda" files from the Github release page.
1. Go to [S3 Management Console](https://s3.console.aws.amazon.com/s3) and click on your bucket that you created above.
1. Click "Upload" and choose the 3 files above that you downloaded to the S3 bucket (drag and drop them, or browse for them), and click "Upload"
1. Click on the `deploy-urmadealgenie.yml` file and copy the `Object URL` 


## 4. Deploy Urma Deal Genie with CloudFormation
An AWS CloudFormation stack is a bit like install kits for AWS. You create a "stack" from a template file, and AWS creates all the services and wires them up as per the template file.
1. Go to [CloudFormation](https://console.aws.amazon.com/cloudformation) and click "Create stack"
1. Enter the S3 URL to the `deploy-urmadealgenie.yml` file, or just upload the file you downloaded
1. Enter a `Stack name` (e.g. UrmaDealGenie, or make up whatever you want)
1. Enter Parameters:
    - `ApiKey` (that you [created in 3Commas](/README.md#create-a-3commas-api-key-and-secret))</br>API key must have **BotRead**, **BotWrite** and **AccountRead**
    - `ApiSecret` (ditto)
    - **Optional** `CmcApiKey` (API key for CoinMarketCap in the [LunarCrush rule](ExampleConfigs-LunarCrushPairRule.md))
    - `LambdaFunctionName` (name of function, you can change it if you want, otherwise leave it as default)
    - `S3Bucket` (name of the bucket you created)
    - `S3PackageFilename` (name of the UrmaDealGenie zip file you uploaded to the bucket)</br>(**Note:** Make sure you copy this exactly right, case sensitive and .zip extension)
    - Optional `StatusEmailAddress` (an email address that you'd like error statuses to be sent, not required as you can use the dashboard instead to monitor)
1. Click "Next" twice, review the stack settings
1. Tick "I acknowledge that AWS CloudFormation..." and click "Create stack"
1. It takes a few minutes, so wait until it says "CREATE_COMPLETE".

And that's it! The Urma Deal Genie will run every 5 minutes and use the `dealrules.json` file in your bucket as the rule configuration.
## Monitoring Urma Deal Genie
This is how you can see if Urma Deal Genie is running and is successful or has errors. Remember it will take 5 minutes at least before you can see any results.
1. Go to [CloudWatch Dashboards](https://console.aws.amazon.com/cloudwatch/home#dashboards:)
1. Click on the "UrmaDealGenie" (or similar name) dashboard
    - Watch the Duration, Errors and Invocations alarms and metrics charts
    - Green dots/line (Success) count should be 1
    - Red dots/line (Error) count should be 0
1. Click on [Log Groups](https://console.aws.amazon.com/cloudwatch/home#logsV2:log-groups) on the left navigation window
1. Click on the "UrmaDealGenie" (or similar name) log group
    - Click on a recent log file
    - Scroll down the log file and see if the logging shows the function is running and finding deals

![image](https://user-images.githubusercontent.com/13062477/147366597-484f3835-7448-4041-802a-be563b1edff2.png)

## Modifying Deal Rules Configuration
The deal rules configuration is kept in the S3 bucket file `dealrules.json`. It's not possible to edit the file in place, so you need to delete it and upload your modified rules:
1. In a text editor on your local machine, edit the dealrules.json file that you downloaded before installation
1. Go to [S3 Management Console](https://s3.console.aws.amazon.com/s3) and click the S3 bucket you created for your deployment files
1. Select and delete the `dealrules.json` file (confirm as AWS ask you to)
1. Upload the modified `dealrules.json` file from your local machine

The easiest way to upload updated rules to your bucket is using the [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-getting-started.html) command:
```
    aws s3 cp dealrules.json s3://yourbucketname/
```

The next time UrmaDealGenie runs, it will pick up your new configuration automatically.

## Testing Rules
It's useful to test rules without updating your deals. Here's how:
1. Go to [Lambda services](https://console.aws.amazon.com/lambda)
1. Click on the "UrmaDealGenie" (or similar name) function
1. Click on “Test” tab, create new Test event and save it
    - Saved event name = `deal-genie-update-false` (or similar)
    - Copy / past body of `test-config-update-false.json`
    - Modify this JSON document - e.g. delete rules you don’t want, change deal rules include/exclude terms to be relative to your bot names
    - See [ExampleConfigs.md](ExampleConfigs.md) for example configs with detailed explanations of rule settings
    - Click Save changes
    - Click Test and expand the Details and hopefully it runs!!
      - If it ran successfully, you can change `Update` to “true” and see if your deals get updated. 
      - It is strongly recommended to modify the rules’ include/exclude terms fields so that the number of deals that need updating is just a few to start with.

![image](https://user-images.githubusercontent.com/13062477/147366727-0626c45f-b4d8-4614-b5cc-5c47bcd3a18b.png)

## Troubleshooting
- **Stack fails to create due to LogGroup already existing**</br>![image](https://user-images.githubusercontent.com/13062477/147569385-d5b9b506-abde-41a1-99e0-eb0ec1ee7289.png)</br>This happens when you already have a CloudWatch LogGroup. This is a known issue when upgrading from 2.3 to 2.4+ of DealGenie.</br>**Resolution** - Delete the failed stack (see below), then go to [CloudWatch LogGroups](https://console.aws.amazon.com/cloudwatch/home#logsV2:log-groups), then select and delete the existing LogGroup.
- **Stack fails to create due to S3 object not found**</br>This happens when you are trying to deploy the stack in a region that is different from where the S3 bucket is. It's a requirement for lambda functions to run in the same region as the S3 bucket that contains the source (there is a solution that I'm working on).</br>**Resolution** - Go to [S3 Management Console](https://s3.console.aws.amazon.com/s3) and check the region where your bucket was created, then go to [CloudFormation](https://console.aws.amazon.com/cloudformation) and [switch region](https://docs.aws.amazon.com/awsconsolehelpdocs/latest/gsg/select-region.html) to the same as the S3 bucket and then deploy stack again.

## Uninstall / Delete 
To remove UrmaDealGenie (permanently, or before installing a newer version), you simply delete the CloudFormation stack:
1. Go to [CloudFormation](https://console.aws.amazon.com/cloudformation)
1. Select the UrmaDealGenie (or similar name) stack 
1. Click Delete and then Delete Stack to confirm

That's it, takes a few minutes to complete deletion.

## Billing
UrmaDealGenie uses the free tier of AWS. To confirm you are not paying a penny:
1. Go to [Cost Explorer](https://console.aws.amazon.com/cost-management/home#/custom)
1. The graph and table below it should show $0

