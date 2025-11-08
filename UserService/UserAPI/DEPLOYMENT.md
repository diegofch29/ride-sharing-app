# Build and Deployment Scripts for UserAPI

## Prerequisites

1. **AWS CLI configured** with appropriate permissions
2. **Docker installed** and running
3. **GitHub repository secrets configured**:
   - `AWS_ACCESS_KEY_ID`
   - `AWS_SECRET_ACCESS_KEY`
   - `MONGODB_CONNECTION_STRING`

## GitHub Secrets Setup

In your GitHub repository, go to Settings > Secrets and variables > Actions, and add:

```
AWS_ACCESS_KEY_ID=your_aws_access_key
AWS_SECRET_ACCESS_KEY=your_aws_secret_key
MONGODB_CONNECTION_STRING=mongodb://mongoadmin:mongopasswd!!@54.235.41.240:27017/?authSource=admin
```

## AWS Resources Required

### 1. ECR Repository

```bash
aws ecr create-repository --repository-name userapi --region us-east-1
```

### 2. Lambda Function

```bash
# Create execution role first
aws iam create-role --role-name lambda-execution-role \
  --assume-role-policy-document '{
    "Version": "2012-10-17",
    "Statement": [
      {
        "Effect": "Allow",
        "Principal": {
          "Service": "lambda.amazonaws.com"
        },
        "Action": "sts:AssumeRole"
      }
    ]
  }'

# Attach basic execution policy
aws iam attach-role-policy \
  --role-name lambda-execution-role \
  --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole

# Create Lambda function (after pushing first image)
aws lambda create-function \
  --function-name userapi-function \
  --role arn:aws:iam::YOUR_ACCOUNT_ID:role/lambda-execution-role \
  --code ImageUri=YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/userapi:latest \
  --package-type Image \
  --timeout 30 \
  --memory-size 512
```

### 3. API Gateway (Optional)

To expose the Lambda function via HTTP API:

```bash
# Create HTTP API
aws apigatewayv2 create-api \
  --name userapi-gateway \
  --protocol-type HTTP \
  --target arn:aws:lambda:us-east-1:YOUR_ACCOUNT_ID:function:userapi-function
```

## Manual Deployment

If you want to deploy manually without GitHub Actions:

```bash
# 1. Build the Docker image
docker build -t userapi .

# 2. Tag for ECR
docker tag userapi:latest YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/userapi:latest

# 3. Login to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com

# 4. Push to ECR
docker push YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/userapi:latest

# 5. Update Lambda function
aws lambda update-function-code \
  --function-name userapi-function \
  --image-uri YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/userapi:latest
```

## Environment Variables

The Lambda function needs these environment variables:

- `ASPNETCORE_ENVIRONMENT=Production`
- `MongoDbSettings__ConnectionString=mongodb://mongoadmin:mongopasswd!!@54.235.41.240:27017/?authSource=admin`
- `MongoDbSettings__DatabaseName=UserServiceDB`
- `MongoDbSettings__UsersCollectionName=Users`

## Testing

After deployment, you can test the endpoints:

```bash
# Get all users
curl https://your-api-gateway-url/api/user

# Create a user
curl -X POST https://your-api-gateway-url/api/user \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "testpassword123"
  }'
```
