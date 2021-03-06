﻿Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Editor
Imports Microsoft.VisualStudio.Language.Intellisense
Imports Microsoft.VisualStudio.OLE.Interop
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Text.Editor
Imports Microsoft.VisualStudio.TextManager.Interop
Imports Microsoft.VisualStudio.Utilities

Namespace Xeora.VSAddIn.IDE.Editor.Completion
    <Export(GetType(IVsTextViewCreationListener))>
    <ContentType(EditorExtension.ControlContentType)>
    <TextViewRole(PredefinedTextViewRoles.Editable)>
    Public NotInheritable Class ControlsCommandHandlerProvider
        Implements IVsTextViewCreationListener

        <Import()>
        Public Property AdapterService() As IVsEditorAdaptersFactoryService

        <Import()>
        Public Property CompletionBroker() As ICompletionBroker

        <Import()>
        Public Property ServiceProvider() As SVsServiceProvider

        Public Sub VsTextViewCreated(ByVal textViewAdapter As IVsTextView) Implements IVsTextViewCreationListener.VsTextViewCreated
            Dim textView As IWpfTextView = AdapterService.GetWpfTextView(textViewAdapter)
            If textView Is Nothing Then Exit Sub

            Dim filter As ControlsCommandHandler =
                New ControlsCommandHandler(CompletionBroker, textView)

            Dim NextCommandHandler As IOleCommandTarget = Nothing
            textViewAdapter.AddCommandFilter(filter, NextCommandHandler)
            filter.NextCommandHandler = NextCommandHandler

            Dim createCommandHandler As Func(Of ControlsCommandHandler) =
                Function() filter
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler)
        End Sub
    End Class
End Namespace