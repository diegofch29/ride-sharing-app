# Multi-stage build for AWS Lambda
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Runtime image for AWS Lambda
FROM public.ecr.aws/lambda/dotnet:8
WORKDIR ${LAMBDA_TASK_ROOT}

# Copy the published application
COPY --from=build /app/publish/ ./

# Set the CMD to the handler
CMD ["UserAPI"]
