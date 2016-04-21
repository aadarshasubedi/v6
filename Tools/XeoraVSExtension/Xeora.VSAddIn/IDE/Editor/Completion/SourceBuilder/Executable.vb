﻿Imports Microsoft.VisualStudio.Language

Namespace Xeora.VSAddIn.IDE.Editor.Completion.SourceBuilder
    Public Class Executable
        Inherits BuilderBase

        Public Overrides Function Build() As Intellisense.Completion()
            Dim CompList As New Generic.List(Of Intellisense.Completion)()
            Dim ExecutablesPath As String =
                IO.Path.GetFullPath(IO.Path.Combine(PackageControl.IDEControl.DTE.ActiveDocument.Path, "..", "Executables"))

            Me.Fill(CompList, ExecutablesPath)

            If PackageControl.IDEControl.GetActiveItemDomainType() = Globals.ActiveDomainTypes.Child Then
                Dim DomainID As String = String.Empty
                Dim SearchDI As New IO.DirectoryInfo(ExecutablesPath)

                Do
                    SearchDI = SearchDI.Parent
                    If Not SearchDI Is Nothing Then
                        DomainID = SearchDI.Name
                        SearchDI = SearchDI.Parent
                    End If

                    If Not SearchDI Is Nothing AndAlso
                        (
                            String.Compare(SearchDI.Name, "Domains", True) = 0 OrElse
                            String.Compare(SearchDI.Name, "Addons", True) = 0
                        ) Then

                        Me.Fill(CompList, IO.Path.Combine(SearchDI.FullName, DomainID, "Executables"))

                        If String.Compare(SearchDI.Name, "Domains", True) = 0 Then SearchDI = Nothing
                    End If
                Loop Until SearchDI Is Nothing
            End If

            CompList.Sort(New CompletionComparer())

            Return CompList.ToArray()
        End Function

        Public Overrides Function Builders() As Intellisense.Completion()
            Return New Intellisense.Completion() {
                    New Intellisense.Completion("Create New Executable", String.Empty, String.Empty, Nothing, Nothing)
                }
        End Function

        Private Sub Fill(ByRef Container As Generic.List(Of Intellisense.Completion), ByVal ExecutablesPath As String)
            Try
                If VSAddIn.Executable.Cache.Instance.IsLatest(ExecutablesPath, VSAddIn.Executable.Cache.QueryTypes.None) Then
                    For Each AssemblyID As String In VSAddIn.Executable.Cache.Instance.GetIDs(ExecutablesPath)
                        Container.Add(
                            New Intellisense.Completion(AssemblyID, String.Format("{0}?", AssemblyID), String.Empty, Me.ProvideImageSource(IconResource._assembly), Nothing)
                        )
                    Next
                Else
                    For Each AssemblyID As String In ExecutableLoaderHelper.ExecutableLoader.GetAssemblies(ExecutablesPath)
                        Container.Add(
                            New Intellisense.Completion(AssemblyID, String.Format("{0}?", AssemblyID), String.Empty, Me.ProvideImageSource(IconResource._assembly), Nothing)
                        )

                        ' Cache the dll for quick access
                        VSAddIn.Executable.Cache.Instance.AddInfo(
                            IO.Path.GetFullPath(ExecutablesPath),
                            AssemblyID
                        )
                    Next
                End If
            Catch ex As Exception
                ' Just Handle Exceptions
            End Try
        End Sub
    End Class
End Namespace