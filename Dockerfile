# Build and run the solution's tests with the Ubuntu-based .NET 10 SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

COPY RMS.RoadManagementSystem.slnx ./
COPY RMS.Api/RMS.Api.csproj RMS.Api/
COPY RMS.Application/RMS.Application.csproj RMS.Application/
COPY RMS.Client/RMS.Client.csproj RMS.Client/
COPY RMS.Domain/RMS.Domain.csproj RMS.Domain/
COPY RMS.Infrastructure/RMS.Infrastructure.csproj RMS.Infrastructure/
COPY RMS.Shared/RMS.Shared.csproj RMS.Shared/
RUN dotnet restore RMS.RoadManagementSystem.slnx

COPY . .
RUN dotnet build RMS.RoadManagementSystem.slnx --no-restore --configuration Release

FROM build AS test
RUN dotnet test RMS.RoadManagementSystem.slnx --no-build --no-restore --configuration Release
