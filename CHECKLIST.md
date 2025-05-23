# Release Checklist

Use this checklist when preparing a new release of the Praefixum.

## Before Release

- [ ] Update version number in project file (if not using GitVersion)
- [ ] Update CHANGELOG.md with new features, fixes, and changes
- [ ] Check that all unit tests pass: `dotnet test`
- [ ] Build in Release configuration: `dotnet build -c Release`
- [ ] Generate NuGet package: `dotnet pack -c Release`
- [ ] Test the NuGet package in a test project
- [ ] Make sure README.md is up-to-date with latest features
- [ ] Ensure all code comments and documentation are up-to-date
- [ ] Create a new Git tag with the version number: `git tag -a v1.x.x -m "Version 1.x.x"`

## For Release

- [ ] Push all changes to GitHub: `git push`
- [ ] Push the tag to GitHub: `git push origin v1.x.x`
- [ ] Wait for GitHub Actions to complete the build
- [ ] Verify that the NuGet package is pushed to NuGet.org (if using automated publishing)
- [ ] Manually publish the package if not using automated publishing:
      `dotnet nuget push UniqueIdGenerator.SourceGen\bin\Release\UniqueIdGenerator.1.x.x.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json`

## After Release

- [ ] Create a GitHub Release with release notes from CHANGELOG.md
- [ ] Verify that the package is available on NuGet.org
- [ ] Test the published package in a new test project
- [ ] Announce the release (if applicable)
- [ ] Plan for next version features and improvements
