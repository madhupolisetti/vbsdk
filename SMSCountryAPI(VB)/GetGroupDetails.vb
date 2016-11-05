
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace SMSCountryApi
    Public Class GetGroupDetails
        Inherits GenericResult


        Public Property Id() As String
            Get
                Return m_Id
            End Get
            Set(value As String)
                m_Id = Value
            End Set
        End Property
        Private m_Id As String
        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(value As String)
                m_Name = Value
            End Set
        End Property
        Private m_Name As String
        Public Property TinyName() As String
            Get
                Return m_TinyName
            End Get
            Set(value As String)
                m_TinyName = Value
            End Set
        End Property
        Private m_TinyName As String
        Public Property StartGroupCallOnEnter() As String
            Get
                Return m_StartGroupCallOnEnter
            End Get
            Set(value As String)
                m_StartGroupCallOnEnter = Value
            End Set
        End Property
        Private m_StartGroupCallOnEnter As String
        Public Property EndGroupCallOnExit() As String
            Get
                Return m_EndGroupCallOnExit
            End Get
            Set(value As String)
                m_EndGroupCallOnExit = Value
            End Set
        End Property
        Private m_EndGroupCallOnExit As String

    End Class
End Namespace



