param($installPath, $toolsPath, $package, $project)

$program = $project.ProjectItems | where { $_.Name -eq "Program.cs" }

if ($program){
	$program.Delete()
}

$projectDir = (Get-Item $project.FullName).Directory
$packagesFile = $projectDir.FullName + "\packages.config"


[xml]$xml = Get-Content $packagesFile
$node = $xml.SelectSingleNode("/packages/package[@id='NServiceBus.SelfHostStarter']")
[Void]$node.ParentNode.RemoveChild($node)
$xml.Save($packagesFile)