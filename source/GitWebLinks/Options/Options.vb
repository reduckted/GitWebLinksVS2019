Imports GitWebLinks
Imports Microsoft.VisualStudio.Shell
Imports System.Threading.Tasks
Imports System.Xml.Linq


Public Class Options
    Implements IAsyncInitializable
    Implements IOptions


    Private Const GitHubEnterpriseServersProperty As String = "GitHubEnterpriseServers"
    Private Const BitbucketServerServersProperty As String = "BitbucketServerServers"
    Private Const LinkTypeProperty As String = "LinkType"


    Private Shared ReadOnly DefaultGitHubEnterpriseServers() As ServerUrl = {
        New ServerUrl("http://my-github-server", "git@my-github-server")
    }


    Private Shared ReadOnly DefaultBitbucketServerServers() As ServerUrl = {
        New ServerUrl("http://my-bitbucket-server:7990/bitbucket", "git@my-bitbucket-server:7999")
    }


    Private ReadOnly cgGitHubEnterpriseServers As New List(Of ServerUrl)(DefaultGitHubEnterpriseServers)
    Private ReadOnly cgBitbucketServerServers As New List(Of ServerUrl)(DefaultBitbucketServerServers)
    Private cgSettingsManager As ISettingsManager


    Public Async Function InitializeAsync(provider As IAsyncServiceProvider) As Threading.Tasks.Task _
        Implements IAsyncInitializable.InitializeAsync

        cgSettingsManager = Await provider.GetServiceAsync(Of ISettingsManager)
        LoadSettings()
    End Function


    Public Property GitHubEnterpriseUrls As IEnumerable(Of ServerUrl) _
        Implements IOptions.GitHubEnterpriseUrls

        Get
            Return cgGitHubEnterpriseServers
        End Get

        Set
            cgGitHubEnterpriseServers.Clear()

            If Value IsNot Nothing Then
                cgGitHubEnterpriseServers.AddRange(Value)
            End If
        End Set
    End Property


    Public Property BitbucketServerUrls As IEnumerable(Of ServerUrl) _
        Implements IOptions.BitbucketServerUrls

        Get
            Return cgBitbucketServerServers
        End Get

        Set
            cgBitbucketServerServers.Clear()

            If Value IsNot Nothing Then
                cgBitbucketServerServers.AddRange(Value)
            End If
        End Set
    End Property


    Public Property LinkType As LinkType _
        Implements IOptions.LinkType


    Private Sub LoadSettings()
        Try
            If cgSettingsManager.Contains(GitHubEnterpriseServersProperty) Then
                GitHubEnterpriseUrls = DecodeServers(cgSettingsManager.GetString(GitHubEnterpriseServersProperty))
            End If

            If cgSettingsManager.Contains(BitbucketServerServersProperty) Then
                BitbucketServerUrls = DecodeServers(cgSettingsManager.GetString(BitbucketServerServersProperty))
            End If

            If cgSettingsManager.Contains(LinkTypeProperty) Then
                Dim value As Integer


                If Integer.TryParse(cgSettingsManager.GetString(LinkTypeProperty), value) Then
                    LinkType = DirectCast(value, LinkType)
                End If
            End If

        Catch ex As Exception
            Diagnostics.Debug.WriteLine(ex.Message)
        End Try
    End Sub


    Public Sub Save() _
        Implements IOptions.Save

        Try
            cgSettingsManager.SetString(GitHubEnterpriseServersProperty, EncodeServers(cgGitHubEnterpriseServers))
            cgSettingsManager.SetString(BitbucketServerServersProperty, EncodeServers(cgBitbucketServerServers))
            cgSettingsManager.SetString(LinkTypeProperty, CInt(LinkType).ToString())

        Catch ex As Exception
            Diagnostics.Debug.WriteLine(ex.Message)
        End Try
    End Sub


    Private Shared Function EncodeServers(servers As IEnumerable(Of ServerUrl)) As String
        Return (
            <servers>
                <%=
                    From server In servers
                    Select <server base=<%= server.BaseUrl %> ssh=<%= server.SshUrl %>/>
                %>
            </servers>
        ).ToString()
    End Function


    Private Shared Iterator Function DecodeServers(value As String) As IEnumerable(Of ServerUrl)
        For Each server In XElement.Parse(value).Elements
            Yield New ServerUrl(server.@base, server.@ssh)
        Next server
    End Function

End Class
