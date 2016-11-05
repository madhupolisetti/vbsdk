Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace SMSCountryApi
    Public Class SendSmsResult
        Inherits GenericResult
        Private _MessageUUID As String = String.Empty

        Public Property MessageUUID() As String
            Get
                Return Me._MessageUUID
            End Get
            Set(value As String)
                Me._MessageUUID = value
            End Set
        End Property

    End Class





End Namespace
