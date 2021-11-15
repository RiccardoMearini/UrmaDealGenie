# TODO list
- instructions need default timeout to be longer
- complete documentation and install/setup commands 
- handle timeout or updating lots of deals in parallel?
- store config in S3 or dynamodb and remove from eventbridge
- support different rule type (fixed TPs?)
- log out account name after connecting to API
- error handling
  - inavlid api key or secret
  - 3c URL unreachable
- find an easy way to stop with simple command (script or API or just instructions?)
- cloud formation, AWS CLI upload zip to bucket:
  - role
  - lambda
  - event
  https://docs.aws.amazon.com/AmazonCloudWatch/latest/events/RunLambdaSchedule.html