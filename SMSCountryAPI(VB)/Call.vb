Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace SMSCountryApi
    Public Class [Call]
        Private _MessageUrl As String = String.Empty

        Private _Number As String = String.Empty

        Private _AnswerUrl As String = String.Empty

        Private _callUUID As String = String.Empty
        Public Property MessageUrl() As String
            Get
                Return Me._MessageUrl
            End Get
            Set(value As String)
                Me._MessageUrl = value
            End Set
        End Property

        Public Property Number() As String
            Get
                Return Me._Number
            End Get
            Set(value As String)
                Me._Number = value
            End Set
        End Property



        Public Property AnswerUrl() As String
            Get
                Return Me._AnswerUrl
            End Get
            Set(value As String)
                Me._AnswerUrl = value
            End Set
        End Property

        Public Property callUUID() As String
            Get
                Return Me._callUUID
            End Get
            Set(value As String)
                Me._callUUID = value
            End Set
        End Property


    End Class
End Namespace
