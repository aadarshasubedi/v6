﻿Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities

Namespace Xeora.VSAddIn.IDE.Editor.Highlighter
    <Export(GetType(IClassifierProvider))>
    <ContentType("xeora")>
    Public NotInheritable Class SyntaxProvider
        Implements IClassifierProvider

        <Import()>
        Private _ClassificationRegistry As IClassificationTypeRegistryService

        Public Function GetClassifier(ByVal buffer As ITextBuffer) As IClassifier Implements IClassifierProvider.GetClassifier
            Return buffer.Properties.GetOrCreateSingletonProperty(Of Syntax)(
                Function() New Syntax(Me._ClassificationRegistry))
        End Function
    End Class
End Namespace