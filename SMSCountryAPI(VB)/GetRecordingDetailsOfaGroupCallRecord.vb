
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace SMSCountryApi
    Public Class GetRecordingDetailsOfaGroupCallRecord
        Inherits GenericResult
        Public Property RecordingID() As String
            Get
                Return m_RecordingID
            End Get
            Set(value As String)
                m_RecordingID = Value
            End Set
        End Property
        Private m_RecordingID As String
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




