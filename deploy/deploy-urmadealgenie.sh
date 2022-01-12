# Run from deploy folder
#  source ./deploy-urmadealgenie.sh

# Restore all project dependencies
dotnet restore ..

# Build lambda package (from ./deploy folder)
dotnet lambda package UrmaDealGenieAWS-localbuild.zip -pl ../src/UrmaDealGenie

# Upload lambda package to S3
aws s3 cp UrmaDealGenieAWS-localbuild.zip s3://urmagurd/
aws s3 cp deploy-urmadealgenie.yml s3://urmagurd/
aws s3 cp ../src/UrmaDealGenieApp/dealrules.json s3://urmagurd/

# Delete old stack and wait for it to be deleted
aws cloudformation delete-stack --stack-name urma-deal-genie --region eu-west-1
aws cloudformation wait stack-delete-complete --stack-name urma-deal-genie --region eu-west-1

# Deploy package changeset from S3 with CloudFormation
aws cloudformation deploy --region eu-west-1 --capabilities CAPABILITY_NAMED_IAM \
  --stack-name urma-deal-genie \
  --template-file deploy-urmadealgenie.yml \
  --s3-bucket urmagurd \
  --parameter-overrides file://parameters.json \
  