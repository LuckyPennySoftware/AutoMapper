# API Changes

Every Windows build of the AutoMapper package runs [package validation](https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview), which compares the public API against the last major version release and against the package's other target frameworks.
Intentional breaking changes are recorded in [CompatibilitySuppressions.xml](https://github.com/LuckyPennySoftware/AutoMapper/blob/main/src/AutoMapper/CompatibilitySuppressions.xml).
From the [releases page](https://github.com/LuckyPennySoftware/AutoMapper/releases) you can reach the source code for a release, and the version of CompatibilitySuppressions.xml in that tree will tell you what changed.
A major version release is compared with the previous major version release (so 17.0.0 with 16.0.0) and a minor version release with the current major version release (so 16.2.0 with 16.0.0).

For releases 9.0 through 14.0, the same information is in `src/AutoMapper/ApiCompatBaseline.txt` in that release's source tree.

## Maintaining the suppressions

To accept an intentional breaking change, regenerate the file on Windows:

```
dotnet pack src/AutoMapper/AutoMapper.csproj -c Release -p:ApiCompatGenerateSuppressionFile=true
```

When starting a new major version, set `PackageValidationBaselineVersion` in `AutoMapper.csproj` to the previous major release (for example 16.0.0 while developing 17.0.0) and regenerate the file.
After the major release ships, set the baseline to the new release (17.0.0) and regenerate the file again, which leaves only the suppressions that compare target frameworks.
