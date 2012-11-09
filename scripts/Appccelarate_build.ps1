
Properties {
  $projectName ="Appccelerate"
  
  $base_dir = Resolve-Path ..
  $binaries_dir = "$base_dir\binaries"
  $publish_dir = "$base_dir\publish"
  $source_dir = "$base_dir\source"
  $script_dir = "$base_dir\scripts"
  $NuGet_dir = "$publish_dir\NuGet"
  
  $sln_file = "$source_dir\$projectName.sln"
  $nuget_symbols_file = "$script_dir\nuget.symbols.txt"
  $license_file = "$source_dir\License.txt"
    
  $version_file_name = "Version.txt"
  $dependencies_file_name = "Dependencies.txt"
  $assembly_info_file_name = "VersionInfo.g.cs"

  
  $xunit_runner = "$source_dir\packages\xunit.runners.1.9.1\tools\xunit.console.clr4.x86.exe"
  $mspec_runner = "$source_dir\packages\Machine.Specifications.0.5.8\tools\mspec-clr4.exe"
  $nuget_console = "$source_dir\.nuget\nuget.exe"
  
  $publish = $false
  $parallelBuild = $true
  
  $build_config = "Release"
  $build_number = 0
  $preVersion = "-alpha"
  
  $allVersions = @{}
}

FormatTaskName (("-"*70) + [Environment]::NewLine + "[{0}]"  + [Environment]::NewLine + ("-"*70))

Task default –depends Clean, Init, WriteAssemblyInfo, Build, Test, CopyBinaries, ResetAssemblyInfo, Nuget

Task Clean { 
    #Delete all bin and obj folders within source directory
    Get-Childitem $source_dir -Recurse | 
    Where {$_.psIsContainer -eq $true -and ($_.name -eq "bin" -or $_.name -eq "obj") } | 
    Foreach-Object { 
        Write-Host "deleting" $_.fullname
        Remove-Item $_.fullname -force -recurse -ErrorAction SilentlyContinue
    }
    
    Remove-Item $publish_dir -force -recurse -ErrorAction SilentlyContinue
    Remove-Item $binaries_dir -force -recurse -ErrorAction SilentlyContinue
}

Task Init -depends Clean {
    Write-Host "creating binaries directory"
    New-Item $binaries_dir -type directory -force
    
    Write-Host "creating publish directory"
    New-Item $publish_dir -type directory -force
}

Task WriteAssemblyInfo -precondition { return $publish } -depends Clean, Init {
    $assemblyVersionPattern = 'AssemblyVersionAttribute\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
	$fileVersionPattern = 'AssemblyFileVersionAttribute\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'

    Get-CoreProjects | 
    Foreach-Object { 
       $versionFile = $_.fullname + "\" + $version_file_name
       $assemblyInfoFile = $_.fullname + "\Properties\" + $assembly_info_file_name
       $versionNumbers = (Get-Content $versionFile).split(".")
       $major = $versionNumbers[0]
       $minor = $versionNumbers[1]
       $assemblyVersion = 'AssemblyVersionAttribute("' + $major + '.' + $minor + '.0.0")'
       $fileVersion = 'AssemblyFileVersionAttribute("' + $major + '.' + $minor + '.'+ $build_number +'.0")'

    	(Get-Content $assemblyInfoFile) | ForEach-Object {
    		% {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
    		% {$_ -replace $fileVersionPattern, $fileVersion }
    	} | Set-Content $assemblyInfoFile
        
        $versions = @(($major + "." + $minor + ".0.0"), ("" + (1+$major) + "." + $minor + ".0.0"), ($major + "." + $minor + "." + $build_number + ".0"))
        $allVersions.Add($_.name, $versions)
    }
}

Task Build -depends Clean, WriteAssemblyInfo {
    Write-Host "building" $sln_file 
    if($parallelBuild){
    
        if($Env:MAX_CPU_COUNT){
            $maxcpucount = ":$Env:MAX_CPU_COUNT"
        }

        Exec { msbuild $sln_file "/p:Configuration=$build_config" "/p:Platform=Any CPU" "/verbosity:minimal" "/fileLogger" "/fileLoggerParameters:LogFile=$base_dir/msbuild.log" "/m$maxcpucount" }
    }else{
        Exec { msbuild $sln_file "/p:Configuration=$build_config" "/p:Platform=Any CPU" "/verbosity:minimal" "/fileLogger" "/fileLoggerParameters:LogFile=$base_dir/msbuild.log" }
    }
}

Task Test -depends Clean, Init, Build {
    RunUnitTest
    RunMSpecTest
}

Task CopyBinaries -precondition { return $publish } { #-depends Clean, WriteAssemblyInfo, Build, Test {
    #copy core binaries
    Get-CoreProjects |  
    Foreach-Object { 
        $project = $_.fullname
        $projectBinaries = $project + "\bin\$build_config\" 
        $projectName = $_.name
        $dependencies_file = $project +"\"+ $dependencies_file_name
        
        Get-Childitem $projectBinaries -Recurse | 
        Where{
            $_.name -eq "$projectName.dll" -or 
            $_.name -eq "$projectName.xml" -or
            $_.name -eq "$projectName.pdb" } |
        Foreach-Object {
            $endpath = $_.fullname.Replace($projectBinaries, "").Replace($_.name, "")
            $destination = $binaries_dir+"\"+$build_config+"\"+$projectName+"\"+$endpath
            Write-Host "copy" $_.fullname "to" $destination
            if (!(Test-Path -path $destination)) {New-Item $destination -Type Directory}
            Copy-Item $_.fullname $destination -force
        }
        
         #copy additionall binaries
         if(Test-Path $dependencies_file){
            (Get-Content $dependencies_file) | ForEach-Object {
                $name = $_
        		Get-Childitem $projectBinaries -Recurse |
                 Where{ $_.name -like $name } |
                Foreach-Object {
                    $endpath = $_.fullname.Replace($projectBinaries, "").Replace($_.name, "")
                    $destination = $binaries_dir+"\"+$build_config+"\"+$projectName+"\"+$endpath
                    Write-Host "copy" $_.fullname "to" $destination 
                    if (!(Test-Path -path $destination)) {New-Item $destination -Type Directory}
                    Copy-Item $_.fullname $destination -force
                }
        	}
         }
    }

}

Task ResetAssemblyInfo -precondition { return $publish } { #-depends Clean, WriteAssemblyInfo, Build, Test, CopyBinaries {
    Get-CoreProjects | 
    Foreach-Object { 
       $assemblyInfoFile = $_.fullname + "\Properties\" + $assembly_info_file_name
       Write-Host "reseting" $assemblyInfoFile
       exec { cmd /c "git checkout $assemblyInfoFile" }
    }
    
}

Task Nuget -precondition { return $publish } { #-depends Clean, WriteAssemblyInfo, Build, Test, CopyBinaries {
    Write-Host "create NuGet directory"
    New-Item $NuGet_dir -type directory -force
    
    
    Get-CoreProjects | 
    Foreach-Object { 
        $project = $_.fullname
        $projectName = $_.name
        $nuspecFile =  $project + "\" + $projectName + ".nuspec"
        $destination = $binaries_dir+"\"+$build_config+"\"+$projectName+"\"
        $newNuspecFile = $destination+$projectName + ".nuspec"
        $versions = $allVersions.Get_Item($projectName)
        $fileVersion = $versions[2].Substring(0,$versions[2].Length-2)+$preVersion
        
        Write-Host "copying and updating" $nuspecFile
        
        $newNuspecContent = (Get-Content $nuspecFile) | ForEach-Object {$_ -replace ("%"+$projectName+"FileVersion%"), $fileVersion }
        $isBinaryPackage = -not ($newNuspecContent | Select-String "/files" -quiet)
        
        if($isBinaryPackage){
            $symbols = (Get-Content $nuget_symbols_file) | ForEach-Object {
        		% {$_ -replace "%sourcefolder%", $projectName }
        	}
            
            $newNuspecContent = $newNuspecContent -replace "</package>", ($symbols+"</package>")
        }
        
        $allVersions.Keys | Foreach-Object {
            $newNuspecContent = $newNuspecContent -replace ("%"+$_+"%"), ("[" + $allVersions.Get_Item($_)[0] + "," + $allVersions.Get_Item($_)[1] + ")")
        }
        
        $newNuspecContent | Set-Content $newNuspecFile
        
        Copy-Item $license_file $destination -force 
        
        if($isBinaryPackage){
            exec { cmd /c "$nuget_console pack $newNuspecFile -symbols" }
        }else{
            exec { cmd /c "$nuget_console pack $newNuspecFile" }
        }
    }

    Get-Childitem $script_dir -Filter *.nupkg | 
    Foreach-Object{
       Write-Host "moving" $_.fullname "to" $NuGet_dir
       Move-Item $_.fullname $NuGet_dir
    }

}

Function RunUnitTest(){
    Get-Childitem $source_dir -Recurse |
    Where{$_.fullname -like "*.Test\bin\$build_config\*Test.dll" } |
    Foreach-Object {
            $testFile = $_.fullname
            Write-Host "testing" $testFile 
            exec { cmd /c "$xunit_runner $testFile" }
    }
    
}

Function RunMSpecTest(){
    Get-Childitem $source_dir -Recurse |
    Where{$_.fullname -like "*.Specification\bin\$build_config\*Specification.dll" } |
    Foreach-Object {
            $testFile = $_.fullname
            $htmlPath = $binaries_dir +"\"+ $_.name + ".html"
            Write-Host "testing" $testFile 
            exec { cmd /c "$mspec_runner --html $htmlPath --teamcity $testFile" }
    }
}

Function Get-CoreProjects(){
    return Get-Childitem $source_dir | 
    Where{$_.psIsContainer -eq $true `
    -and $_.name -like "$projectName.*" `
    -and $_.name -notlike "$projectName.*.Test" `
    -and $_.name -notlike "$projectName.*.Specification" `
    -and $_.name -notlike "$projectName.*.Sample" `
    -and $_.name -notlike "$projectName.*.Performance" `
    -and $_.name -notlike "\.*"}
}