# Publishing the UniqueIdGenerator to GitHub and NuGet

Here are the steps to complete the publishing process for the UniqueIdGenerator source generator.

## Pushing to GitHub

1. Create a new repository on GitHub named "UniqueIdGenerator"
2. Add the GitHub repository as a remote:

```powershell
git remote add origin https://github.com/yourusername/UniqueIdGenerator.git
```

3. Push your code to GitHub:

```powershell
git push -u origin master
```

4. Push the tag to GitHub:

```powershell
git push origin v1.0.0
```

## Publishing to NuGet.org

1. Create an account on [NuGet.org](https://www.nuget.org/) if you don't have one
2. Generate an API key in your NuGet account settings
3. Push the package to NuGet.org:

```powershell
dotnet nuget push UniqueIdGenerator.SourceGen\bin\Release\UniqueIdGenerator.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Configuring CI/CD with GitHub Actions

The GitHub Actions workflow has already been set up in `.github/workflows/build.yml`. It will:
- Build the project
- Run tests
- Generate the NuGet package
- Upload the package as an artifact
- Publish to NuGet.org when a tag is pushed

To make it work correctly:
1. Add the NuGet API key as a secret in your GitHub repository:
   - Go to your repository on GitHub
   - Click on "Settings" > "Secrets and variables" > "Actions"
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet API key

2. Push a tag to trigger a release:
```powershell
git tag -a v1.0.1 -m "Fix NuGet packaging"
git push origin v1.0.1
```

## Future Releases

For future releases:
1. Make your code changes
2. Update version numbers in the project file if needed (or let GitVersion handle it)
3. Create a new tag with semantic versioning
4. Push the tag to GitHub
5. The GitHub Actions workflow will handle the rest

Remember that with GitVersion configured, your version numbers will be determined by your Git tags and commit history.
