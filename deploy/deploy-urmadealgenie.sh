# Set version
export ver=2.2

# Build lambda package
dotnet lambda package UrmaDealGenieAWS-${ver}.zip -pl ../src/UrmaDealGenie

# Upload lambda package to S3
aws s3 cp UrmaDealGenieAWS-${ver}.zip s3://urmagurd/

# Deploy package from S3 with CloudFormation
aws cloudformation deploy --region eu-west-1 --capabilities CAPABILITY_NAMED_IAM \
  --stack-name urma-deal-genie \
  --template-file deploy-urmadealgenie.yml \
  --s3-bucket urmagurd \
  --parameter-overrides file://parameters.json \
  