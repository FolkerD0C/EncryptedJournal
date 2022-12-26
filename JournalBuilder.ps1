#Set a default config that will replace the config file in the end
$ConfigTemplate = @('[InitializerConfig]', 'Password=pass', 'CharacterGroupsCount=4', '', '[JournalConfig]', 'WorkingDirectory=.')

#Get the config that will be set in the .cs files before building
$ActualConfig = Get-Content -Path 'JournalBuilderConfig.txt'
echo "The content of the config file has been read"

#Get the actual data from config
$PasswordToHash = ($ActualConfig[1] -Split '=')[1]
$CharacterGroupsCount = ($ActualConfig[2] -Split '=')[1]
$WorkingDirectory = ($ActualConfig[5] -Split '=')[1]

#Making an output directory
$OutputDir = "OutputDirectory"
if (Test-Path -Path $OutputDir)
{
	Remove-Item -Recurse -Force $OutputDir -erroraction 'silentlycontinue'
}
mkdir $OutputDir -erroraction 'silentlycontinue'
if ($?)
{
    echo "Output folder successfully created"
}

#Build helper project
dotnet publish JournalInitializerHelper --runtime win-x86 --self-contained true --output $OutputDir | out-null
if ($?)
{
    echo "Helper successfully builded"
}

#Get data from the helper
OutputDirectory\JournalInitializerHelper.exe $PasswordToHash $CharacterGroupsCount "$OutputDir\helperOutput.txt"
if ($?)
{
    echo "Helper successfully ran"
}
$HelperOutput = Get-Content -Path $OutputDir\helperOutput.txt
$NewKeyHash = $HelperOutput[0]
$NewCharGroups = $HelperOutput[1]

#Get Program.cs from Main project
$MainProj = Get-Content -Path "EncryptedJournal\Program.cs"

#Get default data from Main project
$DefaultKeyHash = $MainProj[23]
$DefaultWorkingDir = $MainProj[25]
$DefaultCharacterGroups = $MainProj[28]

#Set new data for main project
$MainProj[23] = $NewKeyHash
$MainProj[25] = "`t`tstatic readonly string WorkingDir = @`"$WorkingDirectory`";"
$MainProj[28] = $NewCharGroups
Set-Content -Path "EncryptedJournal\Program.cs" -Value $MainProj

$BuildSucceded = $false
#Build main project
dotnet publish EncryptedJournal --runtime win-x86 --self-contained true --output $OutputDir | out-null
if ($?)
{
    echo "Journal successfully builded"
    $BuildSucceded = $true
}

#Reset config file and EncryptedJournal\Program.cs and delete help output
Set-Content -Path JournalBuilderConfig.txt -Value $ConfigTemplate
$AfterBuildMainProj = Get-Content -Path "EncryptedJournal\Program.cs"
$AfterBuildMainProj[23] = $DefaultKeyHash
$AfterBuildMainProj[25] = $DefaultWorkingDir
$AfterBuildMainProj[28] = $DefaultCharacterGroups
Set-Content -Path "EncryptedJournal\Program.cs" -Value $AfterBuildMainProj
Remove-Item "$OutputDir\JournalInitializerHelper.exe"
Remove-Item "$OutputDir\helperOutput.txt"
if ($BuildSucceded)
{
    echo "`nYou can find EncryptedJournal.exe in $OutputDir"
}