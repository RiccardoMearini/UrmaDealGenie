aws cloudformation deploy \
  --template-file deploy-urmadealgenie.yml \
  --stack-name urma-deal-genie \
  --parameter-overrides file://parameters.json \
  --capabilities CAPABILITY_NAMED_IAM --region eu-west-1