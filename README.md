# Offline Website Viewer
It is a small tool for viewing local websites based on Chromium (using CefSharp). It can view both static html files with CSS and JS in a simple folder and packed files in .zip archive. Also this tool provides search indexing for all .html and .htm files based on `<title>` tag. Works perfectly with Wget or HTTrack website dumps. So you can grab entire website, pack it into zip and view it using this tool and even search for separate pages.

This tool was created as a replacement for ZIM archives for grabbing wikimedia-based websites. ZIM archive creation pipeline doesn't seem robust to me so i decided to make this tool. It's more transparent to developer than ZIM.

![Screenshot 1](https://raw.githubusercontent.com/bshishov/OfflineWebsiteViewer/master/doc/screenshots/screen1.png)

![Screenshot 2](https://raw.githubusercontent.com/bshishov/OfflineWebsiteViewer/master/doc/screenshots/screen2.png)

> Sorry for screenshots in russian, it uses default system locale.

## Download

[Downloads](https://github.com/bshishov/OfflineWebsiteViewer/releases/tag/v.1.0)

## Supported languages

Currently supported languages:

- English
- Russian

You can create your own localizations by creating your own `Resources.Language.<your lang code>.resx` files. See default [Language.resx](https://github.com/bshishov/OfflineWebsiteViewer/blob/master/src/OfflineWebsiteViewer/Resources/Language.ru.resx) file for reference (better open it in Visual Studio).

> **Note** there are different locale files for the installer.

## Requirements:
Requirements for user:
> All requirements for end user are included into installer.

- [.NET 4.5.2](https://www.microsoft.com/download/details.aspx?id=42642)
- [VC 2013 Redistributable](https://www.microsoft.com/download/details.aspx?id=40784) (for CEF libraries)

Requirements for development:

- Visual Studio 2015. ([VS 2015 express](https://www.visualstudio.com/vs/visual-studio-express/) is ok)
- [WIX Toolset](https://wix.codeplex.com/releases/view/624906). For building installer.


## How to create a searchable offline web archive

Let's assume that you want to create a redistributable offline copy of a website.

 1. Download the website using Wget or HTTrack.
 2. Open the output directory of the dump. Make sure that `index.html` is there. If it's not - create it.
 3. Open the Offline Website Viewer click "Open Folder" and select folder of the dump.
 4. Create search index by clicking the command in Main Menu. (this may take a while).
 5. Got to dump directory again. Make sure that there is a new folder called `SearchIndex`
 6. Pack the whole directory into `.zip` archive. *(`index.html` must be in the root of the `.zip` archive)*
 7. Rename archive from `.zip` to `.owr`

## .OWR Archive

There is a file extension `.owr` (**O**-ffline **W**-eb **R**-esource) associated with this tool. Well, it is renamed `.zip` actually. It was made to separate the web archive project from zip archives for end user and letting him open the project by double click.

You can easely create an `.owr` package by creating `.zip` archive and renaming it into `.owr`.

## Building

The project was built in Visual Studio 2015. Just clone the project and build it in VS 2015. Nore that you will need the requirements mentioned before.

There are 3 projects in the solution:

- **OfflineWebsiteViewer**. The application itself. If you will build it it will generate executables and dlls for x86 or x64 platform in `/bin` folder.
-  **Installer**. WIX Installer project for the application. It will generate an `.msi` package for the application. It includes installation to Program Files directory and all shortcut defenitions and file mappings. Note that it doesn't include any UI and requirement packages. It is configured to build only `x86 Release` version of application but you can change it manually.
- **InstallBootstrapper**. This is WIX bootstrapper project. It references installer's `.msi` package and requirements and builds into ready for user 'installer.exe'.

## Search indexing

Search indexing is based on [Lucene.Net](https://lucenenet.apache.org/) package. You can create (or delete) a search index using the command in the main menu. Tool looks for all `.html` and `.htm` files in project folder then extracts a `<title>`  tag contents. Then it creates a special search index stored in same project folder in folder called `SearchIndex`. So you can pack the whole project into archive without losing search index. Just create index once and then it is ready to use.

## Contribution
Feel free to contribute.