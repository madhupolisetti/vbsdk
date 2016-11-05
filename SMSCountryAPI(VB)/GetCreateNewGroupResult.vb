
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace SMSCountryApi
    Public Class GetCreateNewGroupResult
        Inherits GenericResult
        Private _Id As String = String.Empty

        Private _Name As String = String.Empty

        Private _Number As String = String.Empty

        Public Property Id() As String
            Get
                Return Me._Id
            End Get
            Set(value As String)
                Me._Id = value
            End Set
        End Property


        Public Property Name() As String
            Get
                Return Me._Name
            End Get
            Set(value As String)
                Me._Name = value
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




    End Class
End Namespace
