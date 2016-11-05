
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace SMSCountryApi
    Public Class GenericResult
        Private _apiId As String = String.Empty
        Private _success As Boolean = False
        Private _message As String = String.Empty
        Private _statusCode As System.Net.HttpStatusCode
        Private _subStatusCode As Byte = 0


        Public Property ApiId() As String
            Get
                Return Me._apiId
            End Get
            Set(value As String)
                Me._apiId = value
            End Set
        End Property
        Public Property Success() As Boolean
            Get
                Return Me._success
            End Get
            Set(value As Boolean)
                Me._success = value
            End Set
        End Property
        Public Property Message() As String
            Get
                Return Me._message
            End Get
            Set(value As String)
                Me._message = value
            End Set
        End Property
        Public Property StatusCode() As System.Net.HttpStatusCode
            Get
                Return Me._statusCode
            End Get
            Set(value As System.Net.HttpStatusCode)
                Me._statusCode = value
            End Set
        End Property
        Public Property SubStatusCode() As Byte
            Get
                Return Me._subStatusCode
            End Get
            Set(value As Byte)
                Me._subStatusCode = value
            End Set
        End Property
    End Class
End Namespace


