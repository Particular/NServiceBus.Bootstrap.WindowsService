param($installPath, $toolsPath, $package, $project)

function Uninstall()
{
	Write-Host "Uninstalling " + $package.Id
	uninstall-package $package.Id -ProjectName $project.Name
}

function RemoveFromPackageNode
{
	Write-Host "Removing from package node " + $package.Id

	$projectDir = (Get-Item $project.FullName).Directory
	$packagesFile = $projectDir.FullName + "\packages.config"
	[xml]$xml = Get-Content $packagesFile
	$node = $xml.SelectSingleNode("/packages/package[@id='NServiceBus.Bootstrap.WindowsService']")
	[Void]$node.ParentNode.RemoveChild($node)
	$xml.Save($packagesFile)
}

function DeleteProgram
{
	$program = $project.ProjectItems | where { $_.Name -eq "Program.cs" }

	if ($program){
		Write-Host "Deleting Program.cs"
		$program.Delete()
	}
}

function IsExeProject
{
	$outputType = $project.Properties.Item("OutputType").Value
	return $outputType -eq 1
}

if (IsExeProject)
{
	Write-Host "Project is an exe so continuing"
	DeleteProgram
	RemoveFromPackageNode
}
else
{
	Write-Error "Project is not an exe and hence bootstrap package cannot be applied"
	Uninstall
}
