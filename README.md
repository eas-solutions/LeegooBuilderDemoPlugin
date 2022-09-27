<h1>
Sample PlugIn for LEEGOO BUILDER G3
</h1>

- [Roundup](#roundup)
- [Getting Started](#getting-started)
- [Requirements](#requirements)
- [Adjustments](#adjustments)
  - [1. Change several namings](#1-change-several-namings)
  - [2. Source Code Adjustments](#2-source-code-adjustments)
    - [2.1 PluginViewModel.cs](#21-pluginviewmodelcs)
    - [2.2 AssemblyInfo.cs](#22-assemblyinfocs)
    - [2.3 PluginMainModuleController.cs](#23-pluginmainmodulecontrollercs)
    - [2.4 plugin_32x32.png](#24-plugin_32x32png)
    - [2.5 PlugIn.csproj](#25-plugincsproj)
    - [2.6 launchSettings.json](#26-launchsettingsjson)
  - [3. Assembly References](#3-assembly-references)
- [LEEGOO BUILDER integration](#leegoo-builder-integration)

## Roundup
This is a demonstration of how to create a custom PlugIn which can be integratet into LEEGOO BUILDER G3.
There are several examples already implement and more to come in future commitments.

DevExpress is been used in this sample. <br>
If you do not want to use this package, simply remove all references an usages containig "DevExpress".


## Getting Started
This Project can be found on GitHub.

url: https://github.com/eas-solutions/LeegooBuilderDemoPlugin

It is recommended to create a fork of this repository and store it in your own private repository on GitHub.


## Requirements
The following requirements must be met.
- A working installation of an up to date version of LEEGOO BUILDER G3
- MS Visual Studio or another development environment
- (optional) a personal GitHub account 


## Adjustments
There are several things to be adjusted to make this project working on a developers machine.


### 1. Change several namings
Change the following file names according to your requirements:

- PlugIn.csproj
- PlugInMainModuleController.cs
- PlugInModuleInit.cs
- PlugInViewModel.cs
- PlugInViewModelCommands.cs
- PlugInView.xaml

Do not forget to update the namespaces.

### 2. Source Code Adjustments

#### 2.1 PluginViewModel.cs
```c#
public override string Caption => "DemoPlugIn header"; // displayed in client area (upper left corner)
...
protected override void SetUpUI()
        {
            title = "TestTitel"; // displayed on page (bottom left)
        }
```


#### 2.2 AssemblyInfo.cs
Open AssemblyInfo.cs and update all relevant attributes.
```c#
[assembly: AssemblyTitle("DemoPlugIn")]
[assembly: AssemblyDescription("Sample PlugIn for LEEGOO BUILDER G3")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("EAS")]
[assembly: AssemblyProduct("DemoPlugIn")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
...
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
```
<br>

#### 2.3 PluginMainModuleController.cs
Open PluginMainModuleController.cs and find the method RegisterNavBarItem().<br>
Change the values of position and groupName to define, where the PlugIn icon should appear in the navigation panel on the left side.
Position is 0-based, groupName depends on your translations.<br> 
To find out the possible values of goupName you can start LEEGOO BUILDER in `'Terms Only'` mode and to determine the displayed goup names (e.g. "'Proposals", "ProductAdministratoion", "SystemAdministration"). 
```c#
const int position = 0;
var groupName = "Proposals"; // or "ProductAdministration" or "SystemAdministration"
```
<br>

#### 2.4 plugin_32x32.png
Replace the existing glyph by your own image.<br>
If you want to rename this file you must adapt the path in
PlugInMainModuleController.RegisterNavBarItem().<br>
If you add icons, do not forget to set `Build action` to `Resource`.
<br><br>


#### 2.5 PlugIn.csproj
Open PlugIn.csproj and change the following nodes according to your requirements:
- \<RootNamespace>
- \<AssemblyName>
- \<OutputPath>

OutputPath should point to `PlugIns` in your binaries folder (see above).
<br><br> 


#### 2.6 launchSettings.json
Open launchSettings.json and check, if both paths point to the correct folders.\

```json
{
  "profiles": {
    "Plugin": {
      "commandName": "Executable",
      "executablePath": "c:\\Quelltexte\\.Gitea\\LeegooBuilder\\Binaries\\Debug-AnyCPU\\EAS.LeegooBuilder.Client.GUI.Shell.exe",
      "workingDirectory": "c:\\Quelltexte\\.Gitea\\LeegooBuilder\\Binaries\\Debug-AnyCPU"
    }
  }
}
```


### 3. Assembly References
This PlugIns needs references to several LEEGOO BUILDER assemblies. 
The default destination path should be something like that:

**c:\Program files\EAS\LeegooG3\LeegooFiles\<LatestVersion>\Binaries**

Latest Version must be substituted by the correct value.
Normally it should be <Year.Month.0.0>.
 

Steps to adjust all references:
- open **PlugIn.csproj** in an editor
- find this node: **\<LeegooBinaries>**
- change the path to your binaries folder (see above).

```xml
<LeegooBinaries>..\..\LeegooBuilder\Binaries\$(Configuration)-$(Platform)\</LeegooBinaries>
```
Try to compile the project. No errors should occur.


## LEEGOO BUILDER integration
LEEGOO BUILDER needs to be configured to load the PlugIn.<br>
Go to the binaries folder (see above) and open `plugins.xaml`.<br>
Add the following nodes and make shure the path to your PlugIn is correct.
```xml
    <mod:ModuleGroup Name="Plugins" Description="User defined Plugins" BasePath=".\Plugins\">
        <mod:ModuleDefinition Path="DemoPlugin\EAS.LeegooBuilder.Client.GUI.Modules.Plugin.dll"/>
    </mod:ModuleGroup>
```

## Framework Migration LEEGOO BUILDER (.NET48 -> .NET6.0)

### .csproj
- `<TargetFramework>`: Change `net48` to `net6.0-windows`
- Add `<UseWPF>true</UseWPF>` to a `<PropertyGroup>`
- Add `<UseWindowsForms>true</UseWindowsForms>` to a `<PropertyGroup>`

\
Are Referenced implicitly and can be removed:
- `<Reference Include="PresentationCore" />`
- `<Reference Include="System.Web" />`


### launchSettings.json
`executablePath` needs to point to the DLL executable instead of the EXE file


### ViewModels
May ask for new `using`s like `EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Extensions;`