### ℹ️ The extension for Visual Studio 2022 can be found at https://github.com/reduckted/GitWebLinks

<hr>

# Git Web Links for Visual Studio 2019

Get it from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=reduckted.GitWebLinks).

Copy links to files in their online Git repositories from inside Visual Studio 2019.

Works with:
* GitHub
* GitHub Enterprise
* Bitbucket Server
* Bitbucket Cloud
* Azure DevOps

:information_source: For GitHub Enterprise and Bitbucket Server, there is some configuration required. [See below for more details](#github-enterprise-and-bitbucket-server).

## Copy Link to File

To copy a link to the file on GitHub (or Bitbucket), right-click on the file's tab and select _Copy Link to GitHub_.

![Copy Link to File](media/copy-file.png)

## Copy Link to Selection

To copy a link to a particular line in the file, right-click on the line in the editor and select _Copy Link to GitHub_.

If you want to copy a link to a range of lines, just select the lines first.

![Copy Link to Selection](media/copy-selection.png)

## Copy Link to File in Solution Explorer

You can also copy a link to a file or folder from Solution Explorer.

![Copy Link to Solution Explorer Item](media/copy-solution-explorer.png)

## Link Format

Links can be created using the current commit hash, or the current branch name. The current commit hash is the default format. This can be changed by going to _Tools -> Options -> Git Web Links_.

## GitHub Enterprise and Bitbucket Server

If you use GitHub Enterprise or Bitbucket Server, you will need to tell this extension the URLs of those servers. To do that, go to _Tools -> Options -> Git Web Links_.

![Options](media/options.png)

You will need to specify the **base URL** of the server (_not_ the URL of your Git repository). If you use SSH remote URLs, then you will also need to specify the SSH URL.


## Troubleshooting

### It creates a link to the wrong repository

The extension uses your repository's remote URLs to determine what the URL to the website should be. If your repository has multiple remotes, and there is an `origin` remote, that remote is used. If you don't have an `origin` remote, then the first remote, alphabetically, is used.
