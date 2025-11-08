# Local Docker Build and Test Script
param(
    [string]$ImageName = "userapi",
    [string]$Tag = "latest",
    [int]$Port = 8080
)

Write-Host "Building Docker image..." -ForegroundColor Green
docker build -t "${ImageName}:${Tag}" .

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Starting container..." -ForegroundColor Green
    
    # Stop any existing container
    docker stop $ImageName 2>$null
    docker rm $ImageName 2>$null
    
    # Run the container
    docker run -d `
        --name $ImageName `
        -p ${Port}:8080 `
        -e ASPNETCORE_ENVIRONMENT=Development `
        -e "MongoDbSettings__ConnectionString=mongodb://54.235.41.240:27017" `
        -e "MongoDbSettings__DatabaseName=UserServiceDB" `
        -e "MongoDbSettings__UsersCollectionName=Users" `
        "${ImageName}:${Tag}"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Container started successfully!" -ForegroundColor Green
        Write-Host "API available at: http://localhost:${Port}/api/user" -ForegroundColor Yellow
        Write-Host "To view logs: docker logs $ImageName" -ForegroundColor Yellow
        Write-Host "To stop: docker stop $ImageName" -ForegroundColor Yellow
    } else {
        Write-Host "Failed to start container" -ForegroundColor Red
    }
} else {
    Write-Host "Build failed" -ForegroundColor Red
}