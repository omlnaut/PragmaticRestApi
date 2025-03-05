FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

EXPOSE 5000
EXPOSE 5001

# Install SSH client
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends openssh-client \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Make dotnet tools available
RUN export PATH="$PATH:/root/.dotnet/tools" \
    && dotnet tool restore