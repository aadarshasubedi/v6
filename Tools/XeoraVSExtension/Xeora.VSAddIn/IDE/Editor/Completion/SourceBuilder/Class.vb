﻿Imports Microsoft.VisualStudio.Language

Namespace Xeora.VSAddIn.IDE.Editor.Completion.SourceBuilder
    Public Class [Class]
        Inherits BuilderBase

        Private _SelectionExists As Boolean = False

        Public Property WorkingExecutableInfo As String
        Public Property IsClientExecutable As Boolean

        Public Overrides Function Build() As Intellisense.Completion()
            Dim CompList As New Generic.List(Of Intellisense.Completion)()

            Me.Fill(CompList)

            Me._SelectionExists = (CompList.Count > 0)

            CompList.Sort(New CompletionComparer())

            Return CompList.ToArray()
        End Function

        Public Overrides Function Builders() As Intellisense.Completion()
            Dim ExecutableInfo_s As String() = Me.WorkingExecutableInfo.Split("?"c)
            If ExecutableInfo_s.Length > 1 AndAlso Not Me._SelectionExists Then _
                Return Nothing

            Return New Intellisense.Completion() {
                    New Intellisense.Completion("Create New Class/Method", String.Empty, String.Empty, Nothing, Nothing)
                }
        End Function

        Private Sub Fill(ByRef Container As Generic.List(Of Intellisense.Completion))
            Try
                Dim ExecutableInfo_s As String() = Me.WorkingExecutableInfo.Split("?"c)
                Dim AssemblyID As String = ExecutableInfo_s(0)
                Dim ClassIDs As String() = New String() {}
                If Not String.IsNullOrEmpty(ExecutableInfo_s(1)) Then
                    Dim ClassIDs_t As New Generic.List(Of String)
                    ClassIDs = ExecutableInfo_s(1).Split("."c)

                    For Each ClassID As String In ClassIDs
                        If Not String.IsNullOrEmpty(ClassID) Then _
                            ClassIDs_t.Add(ClassID)
                    Next
                    ClassIDs = ClassIDs_t.ToArray()
                End If

                Dim SearchPath As String =
                    Me.LocateAssembly(AssemblyID)
                Dim WorkingCacheInfo As VSAddIn.Executable.Cache.CacheInfo =
                    VSAddIn.Executable.Cache.Instance.GetInfo(SearchPath, AssemblyID)

                If Not WorkingCacheInfo Is Nothing Then
                    If VSAddIn.Executable.Cache.Instance.IsLatest(SearchPath, VSAddIn.Executable.Cache.QueryTypes.Classes, ClassIDs) Then
                        Dim WorkingClassInfo As VSAddIn.Executable.Cache.CacheInfo.ClassInfo =
                            WorkingCacheInfo.Find(ClassIDs)

                        If Not WorkingClassInfo Is Nothing Then
                            For Each cI As VSAddIn.Executable.Cache.CacheInfo.ClassInfo In WorkingClassInfo.Classes
                                Container.Add(
                                    New Intellisense.Completion(cI.ID, String.Format("{0}.", cI.ID), String.Empty, Me.ProvideImageSource(IconResource.classPublic), Nothing)
                                )
                            Next
                        End If
                    Else
                        Dim QueryDll As String = IO.Path.Combine(SearchPath, String.Format("{0}.dll", AssemblyID))

                        Dim WorkingClassInfo As VSAddIn.Executable.Cache.CacheInfo.ClassInfo = WorkingCacheInfo.BaseClass
                        For Each ClassID As String In ClassIDs
                            For Each ClassInfo As VSAddIn.Executable.Cache.CacheInfo.ClassInfo In WorkingClassInfo.Classes
                                If String.Compare(ClassInfo.ID, ClassID) = 0 Then
                                    WorkingClassInfo = ClassInfo

                                    Exit For
                                End If
                            Next
                        Next

                        For Each ClassID As String In ExecutableLoaderHelper.ExecutableLoader.GetAssemblyClasses(QueryDll, ClassIDs)
                            Container.Add(
                                New Intellisense.Completion(ClassID, String.Format("{0}.", ClassID), String.Empty, Me.ProvideImageSource(IconResource.classPublic), Nothing)
                            )

                            WorkingClassInfo.AddClassInfo(ClassID)
                        Next
                    End If

                    If VSAddIn.Executable.Cache.Instance.IsLatest(SearchPath, VSAddIn.Executable.Cache.QueryTypes.Methods, ClassIDs) Then
                        Dim WorkingClassInfo As VSAddIn.Executable.Cache.CacheInfo.ClassInfo =
                            WorkingCacheInfo.Find(ClassIDs)

                        If Not WorkingClassInfo Is Nothing Then
                            For Each mI As VSAddIn.Executable.Cache.CacheInfo.ClassInfo.MethodInfo In WorkingClassInfo.Methods
                                If mI.Params.Length > 0 Then
                                    Container.Add(
                                        New Intellisense.Completion(mI.ID, String.Format("{0},{1}{2}", mI.ID, String.Join("|", mI.Params), IIf(Me.IsClientExecutable, String.Empty, "$")), String.Empty, Me.ProvideImageSource(IconResource.methodPublic), Nothing)
                                    )
                                Else
                                    Container.Add(
                                        New Intellisense.Completion(mI.ID, String.Format("{0}{1}", mI.ID, IIf(Me.IsClientExecutable, String.Empty, "$")), String.Empty, Me.ProvideImageSource(IconResource.methodPublic), Nothing)
                                    )
                                End If
                            Next
                        End If
                    Else
                        Dim QueryDll As String = IO.Path.Combine(SearchPath, String.Format("{0}.dll", AssemblyID))

                        Dim WorkingClassInfo As VSAddIn.Executable.Cache.CacheInfo.ClassInfo = WorkingCacheInfo.BaseClass
                        For Each ClassID As String In ClassIDs
                            For Each ClassInfo As VSAddIn.Executable.Cache.CacheInfo.ClassInfo In WorkingClassInfo.Classes
                                If String.Compare(ClassInfo.ID, ClassID) = 0 Then
                                    WorkingClassInfo = ClassInfo

                                    Exit For
                                End If
                            Next
                        Next

                        For Each item As Object() In ExecutableLoaderHelper.ExecutableLoader.GetAssemblyClassFunctions(QueryDll, ClassIDs)
                            If CType(item(1), String()).Length > 0 Then
                                Container.Add(
                                    New Intellisense.Completion(CType(item(0), String), String.Format("{0},{1}{2}", CType(item(0), String), String.Join("|", CType(item(1), String())), IIf(Me.IsClientExecutable, String.Empty, "$")), String.Empty, Me.ProvideImageSource(IconResource.methodPublic), Nothing)
                                )
                            Else
                                Container.Add(
                                    New Intellisense.Completion(CType(item(0), String), String.Format("{0}{1}", CType(item(0), String), IIf(Me.IsClientExecutable, String.Empty, "$")), String.Empty, Me.ProvideImageSource(IconResource.methodPublic), Nothing)
                                )
                            End If

                            WorkingClassInfo.AddMethodInfo(CType(item(0), String), CType(item(1), String()))
                        Next
                    End If
                End If
            Catch ex As Exception
                ' Just Handle Exceptions
            End Try
        End Sub
    End Class
End Namespace