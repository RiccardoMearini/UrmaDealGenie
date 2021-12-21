# Build lambda package
dotnet lambda package UrmaDealGenieAWSLambdaPackage.zip -pl ../src/UrmaDealGenie

# Upload lambda package to S3
aws s3 cp UrmaDealGenieAWSLambdaPackage.zip s3://urmagurd/
aws s3 cp deploy-urmadealgenie.yml s3://urmagurd/
aws s3 cp ../src/UrmaDealGenieApp/dealrules.json s3://urmagurd/

# Deploy package from S3 with CloudFormation
# aws cloudformation deploy --region eu-west-1 --capabilities CAPABILITY_NAMED_IAM \
#   --stack-name urma-deal-genie \
#   --template-file deploy-urmadealgenie.yml \
#   --s3-bucket urmagurd \
#   --parameter-overrides file://parameters.json \
  