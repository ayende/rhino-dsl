properties { 
  $base_dir  = resolve-path .
  $lib_dir = "$base_dir\SharedLibs"
  $build_dir = "$base_dir\build" 
  $buildartifacts_dir = "$build_dir\" 
  $sln_file = "$base_dir\Rhino.DSL-vs2008.sln" 
  $version = "1.0.0.0"
  $humanReadableversion = "1.0"
  $tools_dir = "$base_dir\Tools"
  $release_dir = "$base_dir\Release"
  $uploadCategory = "Rhino-DSL"
  $uploadScript = "C:\Builds\Upload\PublishBuild.build"
} 

task default -depends Release

task Clean { 
  remove-item -force -recurse $buildartifacts_dir -ErrorAction SilentlyContinue 
  remove-item -force -recurse $release_dir -ErrorAction SilentlyContinue 
} 

task Init -depends Clean { 
	. .\psake_ext.ps1
	Generate-Assembly-Info `
		-file "$base_dir\Rhino.DSL\Properties\AssemblyInfo.cs" `
		-title "Rhino DSL $version" `
		-description "DSL Library for Boo" `
		-company "Hibernating Rhinos" `
		-product "DSL Library for Boo $version" `
		-version $version `
		-clsCompliant "false" `
		-copyright "Hibernating Rhinos & Ayende Rahien 2007 - 2009"
		
	Generate-Assembly-Info `
		-file "$base_dir\Rhino.DSL.Tests\Properties\AssemblyInfo.cs" `
		-title "Rhino DSL Tests $version" `
		-description "DSL Library for Boo" `
		-company "Hibernating Rhinos" `
		-product "DSL Library for Boo $version" `
		-version $version `
		-copyright "Hibernating Rhinos & Ayende Rahien 2007 - 2009"
			
	new-item $release_dir -itemType directory 
	new-item $buildartifacts_dir -itemType directory 
	cp $tools_dir\MbUnit\*.* $build_dir
} 

task Compile -depends Init { 
  exec msbuild "/p:OutDir=""$buildartifacts_dir "" $sln_file"
} 

task Test -depends Compile {
  $old = pwd
  cd $build_dir
  exec ".\MbUnit.Cons.exe" "$build_dir\Rhino.DSL.Tests.dll"
  cd $old
}

task Release -depends Test {
	& $tools_dir\zip.exe -9 -A -j `
		$release_dir\Rhino.DSL-$humanReadableversion-Build-$env:ccnetnumericlabel.zip `
		$build_dir\Rhino.DSL.dll `
		$build_dir\Rhino.DSL.xml `
		license.txt `
		acknowledgements.txt `
		$build_dir\Boo.*.dll
	if ($lastExitCode -ne 0) {
        throw "Error: Failed to execute ZIP command"
    }
}

task Upload -depends Release {
	Write-Host "Starting upload"
	if (Test-Path $uploader) {
		$log = $env:push_msg 
    if($log -eq $null -or $log.Length -eq 0) {
      $log = git log -n 1 --oneline		
    }
		&$uploader "$uploadCategory" "$release_dir\Rhino.DSL-$humanReadableversion-Build-$env:ccnetnumericlabel.zip" "$log"
		
		if ($lastExitCode -ne 0) {
      write-host "Failed to upload to S3: $lastExitCode"
			throw "Error: Failed to publish build"
		}
	}
	else {
		Write-Host "could not find upload script $uploadScript, skipping upload"
	}
}
