﻿Option Strict On

Imports Xeora.Web.Global
Imports Xeora.Web.Shared

Namespace Xeora.Web.Controller.Directive
    Public Class EncodedExecution
        Inherits DirectiveControllerBase
        Implements IParsingRequires

        Public Event ParseRequested(DraftValue As String, ByRef ContainerController As ControllerBase) Implements IParsingRequires.ParseRequested

        Public Sub New(ByVal DraftStartIndex As Integer, ByVal DraftValue As String, ByVal ContentArguments As ArgumentInfoCollection)
            MyBase.New(DraftStartIndex, DraftValue, DirectiveTypes.EncodedExecution, ContentArguments)
        End Sub

        Public Overrides Sub Render(ByRef SenderController As ControllerBase)
            If Me.IsUpdateBlockRequest AndAlso Not Me.InRequestedUpdateBlock Then
                Me.DefineRenderedValue(String.Empty)

                Exit Sub
            End If

            ' EncodedExecution does not have any ContentArguments, That's why it copies it's parent Arguments
            If Not Me.Parent Is Nothing Then _
                Me.ContentArguments.Replace(Me.Parent.ContentArguments)

            Dim matchXF As Text.RegularExpressions.Match =
                Text.RegularExpressions.Regex.Match(Me.InsideValue, "XF~\d+\:\{")

            If matchXF.Success Then
                ' Encode Direct Call Function
                If String.Compare(matchXF.Value.Split("~"c)(0), "XF") = 0 Then
                    Dim controlValueSplitted As String() =
                        Me.InsideValue.Split(":"c)
                    Dim BlockContent As String =
                        String.Join(":", controlValueSplitted, 1, controlValueSplitted.Length - 2)

                    If Not BlockContent Is Nothing AndAlso
                        BlockContent.Trim().Length >= 2 Then

                        BlockContent = BlockContent.Substring(1, BlockContent.Length - 2)

                        If Not BlockContent Is Nothing AndAlso
                            BlockContent.Trim().Length > 0 Then

                            RaiseEvent ParseRequested(BlockContent, Me)

                            BlockContent = Me.Create()

                            If Not BlockContent Is Nothing AndAlso
                                BlockContent.Trim().Length > 0 Then

                                ' Check for Parent UpdateBlock
                                Dim WorkingControl As ControllerBase = Me.Parent
                                Dim ParentUpdateBlockID As String = String.Empty

                                Do Until WorkingControl Is Nothing
                                    If TypeOf WorkingControl Is UpdateBlock Then
                                        ParentUpdateBlockID = CType(WorkingControl, UpdateBlock).ControlID

                                        Exit Do
                                    End If

                                    WorkingControl = WorkingControl.Parent
                                Loop
                                ' !--

                                If Not String.IsNullOrEmpty(ParentUpdateBlockID) Then
                                    Me.DefineRenderedValue(
                                        String.Format("javascript:__XeoraJS.doRequest('{0}', '{1}');", ParentUpdateBlockID, Manager.Assembly.EncodeFunction(Helpers.Context.Request.HashCode, BlockContent.Trim()))
                                    )
                                Else
                                    Me.DefineRenderedValue(
                                        String.Format("javascript:__XeoraJS.postForm('{0}');", Manager.Assembly.EncodeFunction(Helpers.Context.Request.HashCode, BlockContent.Trim()))
                                    )
                                End If
                            Else
                                Throw New Exception.EmptyBlockException()
                            End If
                        Else
                            Throw New Exception.EmptyBlockException()
                        End If
                    Else
                        Throw New Exception.EmptyBlockException()
                    End If
                Else ' Standart Value
                    Throw New Exception.DirectivePointerException()
                End If
            Else
                Throw New Exception.UnknownDirectiveException()
            End If
        End Sub
    End Class
End Namespace
