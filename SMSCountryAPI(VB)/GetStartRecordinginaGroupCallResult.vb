
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace SMSCountryApi

    Public Class GetStartRecordinginaGroupCallResult
        Inherits GenericResult
        Public Property RecordingUUID() As String
            Get
                Return m_RecordingUUID
            End Get
            Set(value As String)
                m_RecordingUUID = Value
            End Set
        End Property
        Private m_RecordingUUID As String
        Public Property Url() As String
            Get
                Return m_Url
            End Get
            Set(value As String)
                m_Url = Value
            End Set
        End Property
        Private m_Url As String
    End Class


End Namespace


