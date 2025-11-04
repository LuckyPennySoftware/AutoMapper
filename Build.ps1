# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

$artifacts = ".\artifacts"

if(Test-Path $artifacts) { Remove-Item $artifacts -Force -Recurse }

if ($env:GITHUB_ACTIONS -eq 'true' -and $env:RUNNER_OS -eq 'Windows') {
    Write-Host "✅ Running inside GitHub Actions on a Windows runner"
    $solution = "./AutoMapper.WindowsCI.slnf"
}
else {
    Write-Host "🖥️ Running locally or on a different platform"
    $solution = "./AutoMapper.sln"
}

exec { & dotnet test $solution --configuration Release --results-directory $artifacts --logger trx }

# Only pack AutoMapper project on Windows runners in GitHub Actions
if ($env:GITHUB_ACTIONS -eq 'true' -and $env:RUNNER_OS -eq 'Windows') {
    exec { & dotnet pack .\src\AutoMapper\AutoMapper.csproj --configuration Release --output $artifacts --no-build }
}
