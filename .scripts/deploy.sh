#!/bin/bash

set -e
set -o pipefail

BUILD_CONFIG=$1
NUGET_API_KEY=$2
NUGET_SOURCE_URL=${3:-https://www.nuget.org/api/v2/package}

if [[ "$BUILD_CONFIG" == "Release" ]]; then

	if [ $# -eq 3 ]; then
		VERSION_SUFFIX="SNAPSHOT-$(git rev-list HEAD | wc -l)-$(git rev-parse --short HEAD)"
		dotnet restore /p:VersionSuffix=$VERSION_SUFFIX
		dotnet build -c ${BUILD_CONFIG} /p:VersionSuffix=$VERSION_SUFFIX
		dotnet pack -c ${BUILD_CONFIG} --no-build --version-suffix $VERSION_SUFFIX
	else
		dotnet restore /p:PackageVersion=$TRAVIS_TAG
		dotnet build -c ${BUILD_CONFIG} /p:PackageVersion=$TRAVIS_TAG
		dotnet pack -c ${BUILD_CONFIG} --no-build /p:PackageVersion=$TRAVIS_TAG
	fi

	dotnet nuget push Didstopia.FeatherJSON/bin/$BUILD_CONFIG/*.nupkg --api-key $NUGET_API_KEY --source $NUGET_SOURCE_URL || true
    dotnet nuget push Didstopia.FeatherJSON.Extensions/bin/$BUILD_CONFIG/*.nupkg --api-key $NUGET_API_KEY --source $NUGET_SOURCE_URL || true
fi
