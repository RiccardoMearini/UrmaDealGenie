# TODO list
- one-click install with shortcuts for Windows and Mac
- github actions for a build pipeline
  https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net
- instructions
  - need default timeout to be longer
  - video of tests
  - split videos to smaller steps
  - links to Urma bots (with images of table, chart and settings)
  - config examples need to have sections - purpose/goal, how they work etc
  - how to stop - disable event bridge
- limit: 100 deals? Remove this limit, or call multiple times in batches (parallel?)  
- aws alert for error count or invocation missed
- store config in S3 or dynamodb and remove from eventbridge
- handle timeout or updating lots of deals in parallel?
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
  
  ![URMA-GURD!!](urmagurd.png)
