﻿Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports System.Web.Script.Serialization
Imports Newtonsoft.Json
Imports System.IO

Namespace SMSCountryApi
    Public Class ApiClient
        Implements IApiActions
        Private _authKey As String = String.Empty
        Private _authToken As String = String.Empty
        Private _version As String = ApiVersion.V_0_1
        Private _httpClient As New HttpClient()
        Private _domain As String = "https://restapi.smscountry.com/"
        Private _baseUrl As String = String.Empty
        Public Sub New()
            If System.Configuration.ConfigurationManager.AppSettings("AuthKey") Is Nothing OrElse System.Configuration.ConfigurationManager.AppSettings("AuthToken") Is Nothing Then
                Throw New KeyNotFoundException("Please setup AuthKey and AuthToken values in application configuration file under appsettings")
            End If
            If String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings("AuthKey")) OrElse String.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings("AuthKey")) OrElse String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings("AuthToken")) OrElse String.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings("AuthToken")) Then
                Throw New ArgumentNullException("AuthKey and AuthToken must be a non-empty string")
            End If
            Me._authKey = System.Configuration.ConfigurationManager.AppSettings("AuthKey")
            Me._authToken = System.Configuration.ConfigurationManager.AppSettings("AuthToken")
        End Sub
        Public Sub New(authKey As String, authToken As String, Optional version As String = Nothing)
            If authKey Is Nothing OrElse String.IsNullOrEmpty(authKey) OrElse String.IsNullOrWhiteSpace(authKey) OrElse authToken Is Nothing OrElse String.IsNullOrEmpty(authToken) OrElse String.IsNullOrWhiteSpace(authToken) Then
                Throw New ArgumentNullException("AuthKey and AuthToken must be a non-empty string")
            End If
            Me._authKey = authKey
            Me._authToken = authToken
            If version IsNot Nothing Then
                Me._version = version
            End If
        End Sub


        'Send Text SMS to a number
        Public Function SendSms(smsObj As Sms) As SendSmsResult Implements IApiActions.SendSms
            Dim result As New SendSmsResult()
            Dim httpResponse As JObject = Nothing
            If smsObj.Text Is Nothing OrElse String.IsNullOrEmpty(smsObj.Text) OrElse String.IsNullOrWhiteSpace(smsObj.Text) Then
                Throw New ArgumentNullException("Text property of SMS should not be empty")
            End If
            If smsObj.Number Is Nothing OrElse String.IsNullOrEmpty(smsObj.Number) OrElse String.IsNullOrWhiteSpace(smsObj.Number) Then
                Throw New ArgumentNullException("Number property of SMS should not be empty")
            End If
            Dim payload As New JObject()
            payload.Add(New JProperty("Text", smsObj.Text))
            payload.Add(New JProperty("Number", smsObj.Number))
            If smsObj.SenderId IsNot Nothing AndAlso smsObj.SenderId.Length > 0 Then
                payload.Add(New JProperty("SenderId", smsObj.SenderId))
            End If
            If smsObj.DRNotifyUrl IsNot Nothing AndAlso smsObj.DRNotifyUrl.Length > 0 Then
                payload.Add(New JProperty("DRNotifyUrl", smsObj.DRNotifyUrl))
            End If
            If smsObj.DRNotifyMethod IsNot Nothing AndAlso smsObj.DRNotifyMethod.Length > 0 Then
                payload.Add(New JProperty("DRNotifyHttpMethod", smsObj.DRNotifyMethod))
            End If
            If smsObj.Tool IsNot Nothing AndAlso smsObj.Tool.Length > 0 Then
                payload.Add(New JProperty("Tool", smsObj.Tool))
            End If
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendData(Me.BaseUrl & "SMSes/", "POST", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            If httpResponse.SelectToken("Success").ToString() = "True" Then
                result.MessageUUID = httpResponse.SelectToken("MessageUUID").ToString()
            End If
            Return result
        End Function

        'Used to track the delivery status of an SMS
        Public Function GetSmsDetails(smsObj As Sms) As GetSmsCollectionDetailsResult Implements IApiActions.GetSmsDetails
            Dim result As New GetSmsCollectionDetailsResult()
            Dim httpResponse As JObject = Nothing
            If smsObj.MessageUUID Is Nothing OrElse String.IsNullOrEmpty(smsObj.MessageUUID) OrElse String.IsNullOrWhiteSpace(smsObj.MessageUUID) Then
                Throw New ArgumentNullException("Please Enter MessageUUID")
            End If
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "SMSes/" & Convert.ToString(smsObj.MessageUUID) & "/", "GET", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message <> "Invalid MessageUUID" Then
                Dim MessageDetails As String = httpResponse.SelectToken(("SMS")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(MessageDetails, GetType(GetSmsCollectionDetailsResult)), GetSmsCollectionDetailsResult)
                des.Message = ""
                Return des
            Else

                Return result
            End If

        End Function

        'Used to get a list of SMS objects based on certain filters

        Public Function GetSmsCollection(smsObj As Sms) As List(Of GetSmsCollectionDetailsResult) Implements IApiActions.GetSmsCollection
            Dim result As New GetSmsCollectionDetailsResult()
            Dim httpResponse As JObject = Nothing
            If smsObj.Startdate Is Nothing OrElse String.IsNullOrEmpty(smsObj.Startdate) OrElse String.IsNullOrWhiteSpace(smsObj.Startdate) Then
                Throw New ArgumentNullException("Please Enter Startdate")
            End If
            If smsObj.Enddate Is Nothing OrElse String.IsNullOrEmpty(smsObj.Enddate) OrElse String.IsNullOrWhiteSpace(smsObj.Enddate) Then
                Throw New ArgumentNullException("Please Enter Enddate")
            End If
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "SMSes/?FromDate=" & Convert.ToString(smsObj.Startdate) & "&ToDate=" & Convert.ToString(smsObj.Enddate) & "&Tool=API", "GET", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message = "Action completed" Then
                Dim MessageDetails As String = httpResponse.SelectToken(("SMSes")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(MessageDetails, GetType(List(Of GetSmsCollectionDetailsResult))), List(Of GetSmsCollectionDetailsResult))
                Return des
            Else
                Dim res As New List(Of GetSmsCollectionDetailsResult)()
                res.Add(result)
                Return res
            End If

        End Function

        ' Used to send SMS to more than one number in a single API call.

        Public Function SendBulkSms(smsObj As Sms) As SendSmsResult Implements IApiActions.SendBulkSms

            Dim result As New SendSmsResult()
            Dim httpResponse As JObject = Nothing
            If smsObj.Text Is Nothing OrElse String.IsNullOrEmpty(smsObj.Text) OrElse String.IsNullOrWhiteSpace(smsObj.Text) Then
                Throw New ArgumentNullException("Text property of SMS should not be empty")
            End If
            If smsObj.Numbers Is Nothing OrElse String.IsNullOrEmpty(smsObj.Numbers) OrElse String.IsNullOrWhiteSpace(smsObj.Numbers) Then
                Throw New ArgumentNullException("Number property of SMS should not be empty")
            End If
            Dim payload As New JObject()


            Dim names As New List(Of String)(smsObj.Numbers.Split(","c))
            'Dim names As List(Of String) = smsObj.Numbers.Split(","c).ToList(Of String)()
            payload.Add(New JProperty("Text", smsObj.Text))
            payload.Add(New JProperty("Numbers", names))
            If smsObj.SenderId IsNot Nothing AndAlso smsObj.SenderId.Length > 0 Then
                payload.Add(New JProperty("SenderId", smsObj.SenderId))
            End If
            If smsObj.DRNotifyUrl IsNot Nothing AndAlso smsObj.DRNotifyUrl.Length > 0 Then
                payload.Add(New JProperty("DRNotifyUrl", smsObj.DRNotifyUrl))
            End If
            If smsObj.DRNotifyMethod IsNot Nothing AndAlso smsObj.DRNotifyMethod.Length > 0 Then
                payload.Add(New JProperty("DRNotifyHttpMethod", smsObj.DRNotifyMethod))
            End If
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendGroupData(Me.BaseUrl & "BulkSMSes/", "POST", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function


        'Used to dial out a new call to a number. Calls Schema

        Public Function CreateCall(callObj As [Call]) As CreateCallResult Implements IApiActions.CreateCall
            Dim result As New CreateCallResult()
            Dim httpResponse As JObject = Nothing
            If callObj.Number Is Nothing OrElse String.IsNullOrEmpty(callObj.Number) OrElse String.IsNullOrWhiteSpace(callObj.Number) Then
                Throw New ArgumentNullException("Number property of Call should not be empty")
            End If
            If callObj.AnswerUrl Is Nothing OrElse String.IsNullOrEmpty(callObj.AnswerUrl) OrElse String.IsNullOrWhiteSpace(callObj.AnswerUrl) Then
                Throw New ArgumentNullException("AnswerUrl property of Call should not be empty")
            End If
            Dim payload As New JObject()
            payload.Add(New JProperty("Number", callObj.Number))
            payload.Add(New JProperty("AnswerUrl", callObj.AnswerUrl))
            Dim Xml As String = "<Response><Play>" & Convert.ToString(callObj.MessageUrl) & "</Play></Response>"
            payload.Add(New JProperty("Xml", Xml))
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendData(Me.BaseUrl & "Calls/", "POST", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function





        'Used to dial out a new call to a number. Calls Schema

        Public Function CreateBulkCall(callObj As [Call]) As CreateBulkCallResult Implements IApiActions.CreateBulkCall
            Dim result As New CreateBulkCallResult()
            Dim httpResponse As JObject = Nothing
            If callObj.Number Is Nothing OrElse String.IsNullOrEmpty(callObj.Number) OrElse String.IsNullOrWhiteSpace(callObj.Number) Then
                Throw New ArgumentNullException("Number property  should not be empty")
            End If
            If callObj.MessageUrl Is Nothing OrElse String.IsNullOrEmpty(callObj.MessageUrl) OrElse String.IsNullOrWhiteSpace(callObj.MessageUrl) Then
                Throw New ArgumentNullException("MessageUrl property  should not be empty")
            End If
            If callObj.AnswerUrl Is Nothing OrElse String.IsNullOrEmpty(callObj.AnswerUrl) OrElse String.IsNullOrWhiteSpace(callObj.AnswerUrl) Then
                Throw New ArgumentNullException("AnswerUrl property should not be empty")
            End If

            Dim payload As New JObject()
            Dim number As New List(Of String)(callObj.Number.Split(","c))


            payload.Add(New JProperty("Numbers", number))
            payload.Add(New JProperty("AnswerUrl", callObj.AnswerUrl))
            Dim Xml As String = "<Response><Play>" & Convert.ToString(callObj.MessageUrl) & "</Play></Response>"
            payload.Add(New JProperty("Xml", Xml))

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendData(Me.BaseUrl & "BulkCalls/", "POST", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()
            Dim sucess As [Boolean] = Convert.ToBoolean(httpResponse.SelectToken("Success").ToString())
            If sucess = True Then
                result.CallUUID = httpResponse.SelectToken("CallUUIDs").ToString()
            End If
            Return result
        End Function


        'Used to get a list of Calls objects based on certain filters

        Public Function GetCallList() As List(Of GetCallDetailsCollectionResult) Implements IApiActions.GetCallList
            Dim result As New GetCallDetailsCollectionResult()
            Dim httpResponse As JObject = Nothing
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "Calls/", "GET", payload.ToString(), Nothing)
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            Dim sucess As [Boolean] = Convert.ToBoolean(httpResponse.SelectToken("Success").ToString())
            result.Message = httpResponse.SelectToken("Message").ToString()
            Dim message As String = httpResponse.SelectToken("Message").ToString()

            If message = "Action Completed" Then
                Dim CallDetails As String = httpResponse.SelectToken(("Calls")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(CallDetails, GetType(List(Of GetCallDetailsCollectionResult))), List(Of GetCallDetailsCollectionResult))
                Return des
            Else
                Dim obj As New List(Of GetCallDetailsCollectionResult)()
                obj.Add(result)

                ' var des = (GetSmsCollectionDetailsResult)Newtonsoft.Json.JsonConvert.DeserializeObject(MessageDetails, typeof(GetSmsCollectionDetailsResult));

                Return obj
            End If

        End Function




        'Used to get the current status of the Call

        Public Function GetCallDetails(callObj As [Call]) As GetCallDetailsCollectionResult Implements IApiActions.GetCallDetails
            Dim result As New GetCallDetailsCollectionResult()
            Dim httpResponse As JObject = Nothing
            If callObj.callUUID Is Nothing OrElse String.IsNullOrEmpty(callObj.callUUID) OrElse String.IsNullOrWhiteSpace(callObj.callUUID) Then
                Throw New ArgumentNullException("Please Enter callUUID")
            End If

            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "Calls/" & Convert.ToString(callObj.callUUID) & "/", "GET", payload.ToString(), Nothing)
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            Dim Success As [Boolean] = Convert.ToBoolean(httpResponse.SelectToken("Success").ToString())
            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message <> "Call Not Found" Then
                Dim CallDetails As String = httpResponse.SelectToken(("Call")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(CallDetails, GetType(GetCallDetailsCollectionResult)), GetCallDetailsCollectionResult)

                Return des
            Else
                Return result
            End If




        End Function



        'Disconnect an on going call by specifying CallUUID

        Public Function DisconnectCall(callObj As [Call]) As DisconnectCallResult Implements IApiActions.DisconnectCall
            Dim result As New DisconnectCallResult()
            Dim httpResponse As JObject = Nothing
            If callObj.callUUID Is Nothing OrElse String.IsNullOrEmpty(callObj.callUUID) OrElse String.IsNullOrWhiteSpace(callObj.callUUID) Then
                Throw New ArgumentNullException("callUUID should not be empty")
            End If

            Dim payload As New JObject()


            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.DisconnectData(Me.BaseUrl & "Calls/" & Convert.ToString(callObj.callUUID) & "/", "PATCH", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()

            Return result
        End Function


        'Used to created your own Group
        Public Function CreateNewGroup(groupObj As Group) As GetCreateNewGroupResult Implements IApiActions.CreateNewGroup
            Dim result As New GetCreateNewGroupResult()
            Dim httpResponse As JObject = Nothing
            Dim payload As New JObject()


            Dim jsonArray As New JArray()



            For Each i In groupObj.Members

                Dim formDetailsJson As New JObject()
                formDetailsJson.Add("Name", i.Name)
                formDetailsJson.Add("Number", i.Number)
                jsonArray.Add(formDetailsJson)
            Next
            Dim s = groupObj.Members.ToList()
            payload.Add(New JProperty("Members", jsonArray))

            payload.Add(New JProperty("Name", groupObj.Name))
            If groupObj.TinyName IsNot Nothing AndAlso groupObj.TinyName.Length > 0 Then
                payload.Add(New JProperty("TinyName", groupObj.TinyName))
            End If
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendGroupData(Me.BaseUrl & "Groups/", "POST", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()

            'if (result.Success)
            '    result.Id = httpResponse.SelectToken("Id").ToString();
            Return result
        End Function



        'Used to get details of a specific group

        Public Function GetGroupDetails(groupObj As Group) As GetGroupDetails Implements IApiActions.GetGroupDetails
            Dim result As New GetGroupDetails()
            Dim httpResponse As JObject = Nothing
            If groupObj.groupId Is Nothing OrElse String.IsNullOrEmpty(groupObj.groupId) OrElse String.IsNullOrWhiteSpace(groupObj.groupId) Then
                Throw New ArgumentNullException("Please Enter GroupID")
            End If

            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "Groups/" & Convert.ToString(groupObj.groupId) & "/", "GET", payload.ToString(), Nothing)




            result.Message = httpResponse.SelectToken("Message").ToString()



            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            ' result.Success = Convert.ToBoolean(httpResponse.SelectToken("Success"));

            result.Message = httpResponse.SelectToken("Message").ToString()
            Dim message As String = result.Message
            If message = "Action Completed" Then
                Dim GroupDetails As String = httpResponse.SelectToken(("Group")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(GroupDetails, GetType(GetGroupDetails)), GetGroupDetails)


                Return des
            Else





                Return result
            End If
        End Function

        ' Used to list all your Groups
        Public Function GetGroupCollection() As List(Of GetGroupDetails) Implements IApiActions.GetGroupCollection
            Dim result As New GetGroupDetails()
            Dim httpResponse As JObject = Nothing
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "Groups/", "GET", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message = "Action Completed" Then
                Dim GroupDetails As String = httpResponse.SelectToken(("Groups")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(GroupDetails, GetType(List(Of GetGroupDetails))), List(Of GetGroupDetails))
                Return des
            Else
                Dim obj As New List(Of GetGroupDetails)()
                obj.Add(result)


                Return obj
            End If
        End Function

        'Update group details such as name, tinyname etc
        Public Function UpdateGroup(GroupObj As Group) As GetUpdateGroupResult Implements IApiActions.UpdateGroup
            Dim result As New GetUpdateGroupResult()
            Dim httpResponse As JObject = Nothing

            If GroupObj.groupId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.groupId) OrElse String.IsNullOrWhiteSpace(GroupObj.groupId) Then
                Throw New ArgumentNullException("Please Enter GroupID")
            End If
            Dim payload As New JObject()

            payload.Add(New JProperty("TinyName", GroupObj.TinyName))
            payload.Add(New JProperty("Name", GroupObj.Name))

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.UpdateData(Me.BaseUrl & "Groups/" & Convert.ToString(GroupObj.groupId) & "/", "PATCH", payload.ToString(), Nothing)
            If httpResponse IsNot Nothing Then
                result.Message = httpResponse.SelectToken("Message").ToString()
            Else
                result.Message = "Group Deleted Successfully"
            End If
            Return result
        End Function

        'Delete a group by using GroupId
        Public Function DeleteGroup(GroupObj As Group) As DeleteGroupDetails Implements IApiActions.DeleteGroup
            Dim result As New DeleteGroupDetails()
            Dim httpResponse As JObject = Nothing
            Dim payload As New JObject()
            If GroupObj.groupId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.groupId) OrElse String.IsNullOrWhiteSpace(GroupObj.groupId) Then
                Throw New ArgumentNullException("Please Enter GroupID")
            End If
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.DeleteData(Me.BaseUrl & "Groups/" & Convert.ToString(GroupObj.groupId) & "/", "DELETE", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function



        Public Function DeleteMemberfromGroup(GroupObj As Group) As GetDeleteGroupMemberDetails Implements IApiActions.DeleteMemberfromGroup
            Dim result As New GetDeleteGroupMemberDetails()
            Dim httpResponse As JObject = Nothing
            Dim payload As New JObject()
            If GroupObj.groupId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.groupId) OrElse String.IsNullOrWhiteSpace(GroupObj.groupId) Then
                Throw New ArgumentNullException("Please Enter GroupID")
            End If
            If GroupObj.MemberId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.MemberId) OrElse String.IsNullOrWhiteSpace(GroupObj.MemberId) Then
                Throw New ArgumentNullException("Please Enter MemberId")
            End If
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.DeleteData(Me.BaseUrl & "Groups/" & Convert.ToString(GroupObj.groupId) & "/Members/" & Convert.ToString(GroupObj.MemberId) & "/", "DELETE", payload.ToString(), Nothing)
            If httpResponse IsNot Nothing Then
                result.Message = httpResponse.SelectToken("Message").ToString()
            Else
                result.Message = "Member Deleted Successfully"
            End If
            Return result
        End Function



        'Get a particular member details by GroupId and MemberId

        Public Function GetMemberDetails(groupObj As Group) As GetGroupMemberDetailsResult Implements IApiActions.GetMemberDetails

            Dim result As New GetGroupMemberDetailsResult()
            Dim httpResponse As JObject = Nothing
            If groupObj.groupId Is Nothing OrElse String.IsNullOrEmpty(groupObj.groupId) OrElse String.IsNullOrWhiteSpace(groupObj.groupId) Then
                Throw New ArgumentNullException("Please Enter GroupId")
            End If
            If groupObj.MemberId Is Nothing OrElse String.IsNullOrEmpty(groupObj.MemberId) OrElse String.IsNullOrWhiteSpace(groupObj.MemberId) Then
                Throw New ArgumentNullException("Please Enter Memberid")
            End If
            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl + "Groups/" + groupObj.groupId + "/Members/" + groupObj.MemberId + "/", "GET", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()

            If result.Message = "Invalid MemberID" OrElse result.Message = "Invalid GroupID" Then
                result.Message = httpResponse.SelectToken("Message").ToString()
                Return result
            Else


                result.ApiId = httpResponse.SelectToken("ApiId").ToString()
                result.Message = httpResponse.SelectToken("Message").ToString()
                Dim GroupDetails As String = httpResponse.SelectToken(("Member")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(GroupDetails, GetType(GetGroupMemberDetailsResult)), GetGroupMemberDetailsResult)
                Return des
            End If
        End Function



        'Get the details of all the members belongs to a group by using GroupId


        Public Function GetallMemberodGroup(groupObj As Group) As List(Of GetGroupMemberDetailsResult) Implements IApiActions.GetallMemberodGroup
            Dim result As New GetGroupMemberDetailsResult()
            Dim httpResponse As JObject = Nothing
            If groupObj.groupId Is Nothing OrElse String.IsNullOrEmpty(groupObj.groupId) OrElse String.IsNullOrWhiteSpace(groupObj.groupId) Then
                Throw New ArgumentNullException("Please Enter GroupId")
            End If

            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "Groups/" & Convert.ToString(groupObj.groupId) & "/Members/" & Convert.ToString(groupObj.MemberId) & "/", "GET", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message = "Action Completed" Then
                result.Message = httpResponse.SelectToken("Message").ToString()
                Dim GroupDetails As String = httpResponse.SelectToken(("Members")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(GroupDetails, GetType(List(Of GetGroupMemberDetailsResult))), List(Of GetGroupMemberDetailsResult))
                Return des
            Else
                Dim obj As New List(Of GetGroupMemberDetailsResult)()
                obj.Add(result)
                Return obj
            End If
        End Function



        'Update specific member details by using GroupId and MemberId

        Public Function UpdateMemberDetails(GroupObj As Group) As GetUpdateMemberResult Implements IApiActions.UpdateMemberDetails
            Dim result As New GetUpdateMemberResult()
            Dim httpResponse As JObject = Nothing
            If GroupObj.groupId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.groupId) OrElse String.IsNullOrWhiteSpace(GroupObj.groupId) Then
                Throw New ArgumentNullException("Please Enter GroupId")
            End If
            If GroupObj.MemberId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.MemberId) OrElse String.IsNullOrWhiteSpace(GroupObj.MemberId) Then
                Throw New ArgumentNullException("Please Enter Memberid")
            End If
            Dim payload As New JObject()
            payload.Add(New JProperty("Number", GroupObj.Number))
            payload.Add(New JProperty("Name", GroupObj.Name))
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.UpdateData(Me.BaseUrl & "Groups/" & Convert.ToString(GroupObj.groupId) & "/Members/" & Convert.ToString(GroupObj.MemberId) & "/", "PATCH", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function





        'Add a member to an existing group by using GroupId

        Public Function AddMembertoExistingGroup(GroupObj As Group) As GetGroupMemberDetailsResult Implements IApiActions.AddMembertoExistingGroup
            Dim result As New GetGroupMemberDetailsResult()
            Dim httpResponse As JObject = Nothing

            If GroupObj.groupId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.groupId) OrElse String.IsNullOrWhiteSpace(GroupObj.groupId) Then
                Throw New ArgumentNullException("Please Enter GroupId")
            End If

            Dim payload As New JObject()

            payload.Add(New JProperty("Number", GroupObj.Number))
            payload.Add(New JProperty("Name", GroupObj.Name))

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendData(Me.BaseUrl & "Groups/" & Convert.ToString(GroupObj.groupId) & "/Members/", "POST", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()


            'if (result.Success)
            '    result.ApiId = httpResponse.SelectToken("ApiId").ToString();
            Return result
        End Function

        'Create a new group call by providing rth required information furnished below.

        Public Function CreateaGroupCall(groupObj As GroupCall) As GetCreateGroupCallResult Implements IApiActions.CreateaGroupCall
            Dim result As New GetCreateGroupCallResult()
            Dim httpResponse As JObject = Nothing
            Dim payload As New JObject()
            If groupObj.Name Is Nothing OrElse String.IsNullOrEmpty(groupObj.Name) OrElse String.IsNullOrWhiteSpace(groupObj.Name) Then
                Throw New ArgumentNullException("Please Enter Name of Group")
            End If
            If groupObj.AnswerUrl Is Nothing OrElse String.IsNullOrEmpty(groupObj.AnswerUrl) OrElse String.IsNullOrWhiteSpace(groupObj.AnswerUrl) Then
                Throw New ArgumentNullException("Please Enter Answerurl")
            End If

            If groupObj.Participatants Is Nothing Then
                Throw New ArgumentNullException("Please Enter Participatants")
            End If
            Dim jsonArray As New JArray()

            If groupObj.Name IsNot Nothing AndAlso groupObj.Name.Length > 0 Then
                payload.Add(New JProperty("Name", groupObj.Name))
            End If

            If groupObj.WelcomeSound IsNot Nothing AndAlso groupObj.WelcomeSound.Length > 0 Then
                payload.Add(New JProperty("WelcomeSound", groupObj.WelcomeSound))
            End If

            If groupObj.WaitSound IsNot Nothing AndAlso groupObj.WaitSound.Length > 0 Then
                payload.Add(New JProperty("WaitSound", groupObj.WaitSound))
            End If

            If groupObj.StartGropCallOnEnter IsNot Nothing AndAlso groupObj.StartGropCallOnEnter.Length > 0 Then
                payload.Add(New JProperty("StartGropCallOnEnter", groupObj.StartGropCallOnEnter))
            End If

            If groupObj.EndGroupCallOnExit IsNot Nothing AndAlso groupObj.EndGroupCallOnExit.Length > 0 Then
                payload.Add(New JProperty("EndGroupCallOnExit", groupObj.EndGroupCallOnExit))
            End If

            If groupObj.AnswerUrl IsNot Nothing AndAlso groupObj.AnswerUrl.Length > 0 Then
                payload.Add(New JProperty("AnswerUrl", groupObj.AnswerUrl))
            End If


            For Each i In groupObj.Participatants
                Dim formDetailsJson As New JObject()
                formDetailsJson.Add("Name", i.Name)
                formDetailsJson.Add("Number", i.Number)
                jsonArray.Add(formDetailsJson)
            Next

            Dim s = groupObj.Participatants.ToList()
            payload.Add(New JProperty("Participants", jsonArray))







            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendGroupData(Me.BaseUrl & "GroupCalls/", "POST", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()

            'if (result.Success)
            '    result.Message = httpResponse.SelectToken("Message").ToString();
            Return result
        End Function


        'Get group call list

        Public Function GetGroupCallList() As List(Of GetGroupCallListResult) Implements IApiActions.GetGroupCallList
            Dim result As New GetGroupCallListResult()
            Dim httpResponse As JObject = Nothing

            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "GroupCalls/", "GET", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message = "Action Completed" Then
                result.ApiId = httpResponse.SelectToken("ApiId").ToString()
                result.Message = httpResponse.SelectToken("Message").ToString()
                Dim GroupCalls As String = httpResponse.SelectToken(("GroupCalls")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(GroupCalls, GetType(List(Of GetGroupCallListResult))), List(Of GetGroupCallListResult))
                Return des
            Else

                Dim obj As New List(Of GetGroupCallListResult)()
                obj.Add(result)
                Return obj
            End If






        End Function



        'Get group call details by using GroupCallUUID

        Public Function GetGroupCallDetails(groupObj As GroupCall) As GetGroupCallListResult Implements IApiActions.GetGroupCallDetails
            Dim result As New GetGroupCallListResult()
            Dim httpResponse As JObject = Nothing
            If groupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(groupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(groupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If
            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(groupObj.GroupCallUUID) & "/", "GET", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message <> "Invalid GroupCallId" Then
                Dim GroupCalls As String = httpResponse.SelectToken(("GroupCall")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(GroupCalls, GetType(GetGroupCallListResult)), GetGroupCallListResult)
                Return des
            Else
                Return result
            End If
        End Function




        'Get a participant details like name, number, calls details by using GroupCallUUID and ParticipantId

        Public Function GetaParticipantDetailsFromGroupCall(groupObj As GroupCall) As GetaparticipantDetailsFromGroupCallResult Implements IApiActions.GetaParticipantDetailsFromGroupCall
            Dim result As New GetaparticipantDetailsFromGroupCallResult()
            Dim httpResponse As JObject = Nothing
            If groupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(groupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(groupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If
            If groupObj.ParticipantId Is Nothing OrElse String.IsNullOrEmpty(groupObj.ParticipantId) OrElse String.IsNullOrWhiteSpace(groupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  ParticipantId")
            End If
            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(groupObj.GroupCallUUID) & "/Participants/" & Convert.ToString(groupObj.ParticipantId) & "/", "GET", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message = "Action Completed" Then
                Dim Participant As String = httpResponse.SelectToken(("Participant")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(Participant, GetType(GetaparticipantDetailsFromGroupCallResult)), GetaparticipantDetailsFromGroupCallResult)
                Return des
            Else
                Return result
            End If

        End Function


        'Get all participant details like name, number, calls details by using GroupCallUUID

        Public Function GetAllParticipantDetailsFromGroupCall(groupObj As GroupCall) As List(Of GetAllParticipantsFromGroupCallDetailsResult) Implements IApiActions.GetAllParticipantDetailsFromGroupCall
            Dim result As New GetAllParticipantsFromGroupCallDetailsResult()
            Dim httpResponse As JObject = Nothing
            If groupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(groupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(groupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(groupObj.GroupCallUUID) & "/Participants/", "GET", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            result.Message = httpResponse.SelectToken("Message").ToString()
            If result.Message = "Action Completed" Then
                Dim Participants As String = httpResponse.SelectToken(("Participants")).ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(Participants, GetType(List(Of GetAllParticipantsFromGroupCallDetailsResult))), List(Of GetAllParticipantsFromGroupCallDetailsResult))
                Return des
            Else
                Dim obj As New List(Of GetAllParticipantsFromGroupCallDetailsResult)()
                obj.Add(result)
                Return obj
            End If

        End Function

        ' Play a sound file into a Group Call that can be hered by all the participants
        Public Function PlaySoundintoGroupCall(GroupObj As GroupCall) As PlaySoundintogroupCallResult Implements IApiActions.PlaySoundintoGroupCall
            Dim result As New PlaySoundintogroupCallResult()
            Dim httpResponse As JObject = Nothing

            If GroupObj.File Is Nothing OrElse String.IsNullOrEmpty(GroupObj.File) OrElse String.IsNullOrWhiteSpace(GroupObj.File) Then
                Throw New ArgumentNullException("Please Enter  File ")
            End If

            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If
            Dim payload As New JObject()

            payload.Add(New JProperty("File", GroupObj.File))


            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Play/", "POST", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function



        'Play a sound int oa Participant Call, without letting other participants here it by using GroupCallUUID and ParticipantId

        Public Function PlaySoundintoParticipantCallinGroupCall(GroupObj As GroupCall) As PlaySoundintoParticipantCallinGroupCallResult Implements IApiActions.PlaySoundintoParticipantCallinGroupCall
            Dim result As New PlaySoundintoParticipantCallinGroupCallResult()
            Dim httpResponse As JObject = Nothing

            If GroupObj.File Is Nothing OrElse String.IsNullOrEmpty(GroupObj.File) OrElse String.IsNullOrWhiteSpace(GroupObj.File) Then
                Throw New ArgumentNullException("Please Enter  File ")
            End If

            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If

            If GroupObj.ParticipantId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.ParticipantId) OrElse String.IsNullOrWhiteSpace(GroupObj.ParticipantId) Then
                Throw New ArgumentNullException("Please Enter  ParticipantId")
            End If
            Dim payload As New JObject()
            payload.Add(New JProperty("File", GroupObj.File))
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Participants/" & Convert.ToString(GroupObj.ParticipantId) & "/Play/", "POST", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function



        'Make all the participants Mute except Host (one who initiated the GroupCall) by using GroupCallUUID

        Public Function MuteAllParticipantsinaGroupCall(GroupObj As GroupCall) As GetMuteAllParticipantsinaGroupCallResult Implements IApiActions.MuteAllParticipantsinaGroupCall
            Dim result As New GetMuteAllParticipantsinaGroupCallResult()
            Dim httpResponse As JObject = Nothing
            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.PatchData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Mute/", "PATCH", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function



        'UnMute all participants who are in Mute state using GroupCallUUID

        Public Function UnmuteMuteAllParticipantsinaGroupCall(GroupObj As GroupCall) As GetUnmuteMuteAllParticipantsinaGroupCall Implements IApiActions.UnmuteMuteAllParticipantsinaGroupCall
            Dim result As New GetUnmuteMuteAllParticipantsinaGroupCall()
            Dim httpResponse As JObject = Nothing
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.PatchData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/UnMute/", "PATCH", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function




        'Mute only a specific participant by using GroupCallUUID and ParticipantId

        Public Function MuteParticipantsinaGroupCall(GroupObj As GroupCall) As GetMuteParticipantInaGroupcall Implements IApiActions.MuteParticipantsinaGroupCall
            Dim result As New GetMuteParticipantInaGroupcall()
            Dim httpResponse As JObject = Nothing

            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If

            If GroupObj.ParticipantId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.ParticipantId) OrElse String.IsNullOrWhiteSpace(GroupObj.ParticipantId) Then
                Throw New ArgumentNullException("Please Enter  ParticipantId")
            End If

            Dim payload As New JObject()


            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.PatchData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Participants/" & Convert.ToString(GroupObj.ParticipantId) & "/Mute/", "PATCH", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()


            'if (result.Success)
            '    result.ApiId = httpResponse.SelectToken("ApiId").ToString();
            Return result
        End Function



        'UnMute participant who is in Mute state using GroupCallUUID and ParticipantId

        Public Function UnmuteMuteParticipantsinaGroupCall(GroupObj As GroupCall) As GetUnmuteMuteParticipantsinaGroupCall Implements IApiActions.UnmuteMuteParticipantsinaGroupCall
            Dim result As New GetUnmuteMuteParticipantsinaGroupCall()
            Dim httpResponse As JObject = Nothing

            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If

            If GroupObj.ParticipantId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.ParticipantId) OrElse String.IsNullOrWhiteSpace(GroupObj.ParticipantId) Then
                Throw New ArgumentNullException("Please Enter  ParticipantId")
            End If
            Dim payload As New JObject()


            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.PatchData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Participants/" & Convert.ToString(GroupObj.ParticipantId) & "/UnMute/", "PATCH", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()


            'if (result.Success)
            '    result.ApiId = httpResponse.SelectToken("ApiId").ToString();
            Return result
        End Function



        'Record the call conversation by using GroupCallUUID

        Public Function StartRecordinginaGroupCall(GroupObj As GroupCall) As GetStartRecordinginaGroupCallResult Implements IApiActions.StartRecordinginaGroupCall
            Dim result As New GetStartRecordinginaGroupCallResult()
            Dim httpResponse As JObject = Nothing

            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If

            If GroupObj.FileFormat Is Nothing OrElse String.IsNullOrEmpty(GroupObj.FileFormat) OrElse String.IsNullOrWhiteSpace(GroupObj.FileFormat) Then
                Throw New ArgumentNullException("Please Enter  FileFormat")
            End If
            Dim payload As New JObject()



            payload.Add(New JProperty("FileFormat", GroupObj.FileFormat))

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.SendData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Recordings/", "POST", payload.ToString(), Nothing)

            result.Message = httpResponse.SelectToken("Message").ToString()

            If httpResponse.SelectToken("Message").ToString() = "Recording started" Then
                Dim a = httpResponse.SelectToken("Recording").ToString()
                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(a, GetType(GetStartRecordinginaGroupCallResult)), GetStartRecordinginaGroupCallResult)

                result.RecordingUUID = des.RecordingUUID

                result.Url = des.Url
                'if (result.Success)
                '    result.ApiId = httpResponse.SelectToken("ApiId").ToString();
                Return result
            Else
                Return result
            End If


        End Function



        'Stop all running recordings on a group call in a single api request by using GroupCallUUID

        Public Function StopRecordinginaGroupCall(GroupObj As GroupCall) As GetStopRecordinginaGroupCallResult Implements IApiActions.StopRecordinginaGroupCall
            Dim result As New GetStopRecordinginaGroupCallResult()
            Dim httpResponse As JObject = Nothing
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.PatchData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Recordings/", "PATCH", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function


        'Get recording files that are under this Group Call by using GroupCallUUID and RecordingUUID

        Public Function GetRecordingDetailsOfaGroupCall(groupObj As GroupCall) As GetRecordingDetailsOfaGroupCallRecord Implements IApiActions.GetRecordingDetailsOfaGroupCall
            Dim result As New GetRecordingDetailsOfaGroupCallRecord()
            Dim httpResponse As JObject = Nothing
            If groupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(groupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(groupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If

            If groupObj.RecordingUUID Is Nothing OrElse String.IsNullOrEmpty(groupObj.RecordingUUID) OrElse String.IsNullOrWhiteSpace(groupObj.RecordingUUID) Then
                Throw New ArgumentNullException("Please Enter  RecordingUUID")
            End If

            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(groupObj.GroupCallUUID) & "/Recordings/" & Convert.ToString(groupObj.RecordingUUID) & "/", "GET", payload.ToString(), Nothing)




            result.Message = httpResponse.SelectToken("Message").ToString()

            result.ApiId = httpResponse.SelectToken("ApiId").ToString()
            If httpResponse.SelectToken("Message").ToString() = "Action Completed" Then
                'result.Success = Convert.ToBoolean(httpResponse.SelectToken("Success"));

                Dim Recording As String = httpResponse.SelectToken(("Recording")).ToString()

                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(Recording, GetType(GetRecordingDetailsOfaGroupCallRecord)), GetRecordingDetailsOfaGroupCallRecord)

                Return des
            Else
                Return result
            End If
        End Function

        'Get all recording files that are under this Group Call by using GroupCallUUID

        Public Function GetAllRecordingDetailsOfaGroupCall(groupObj As GroupCall) As List(Of GetAllRecordingDetailsOfaGroupCallRecord) Implements IApiActions.GetAllRecordingDetailsOfaGroupCall
            Dim result As New GetAllRecordingDetailsOfaGroupCallRecord()
            Dim httpResponse As JObject = Nothing
            If groupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(groupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(groupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If

            Dim payload As New JObject()

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.GetData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(groupObj.GroupCallUUID) & "/Recordings/", "GET", payload.ToString(), Nothing)




            result.Message = httpResponse.SelectToken("Message").ToString()

            If httpResponse.SelectToken("Message").ToString() = "Action Completed" Then

                result.ApiId = httpResponse.SelectToken("ApiId").ToString()
                ' result.Success = Convert.ToBoolean(httpResponse.SelectToken("Success"));


                Dim Recordings As String = httpResponse.SelectToken(("Recordings")).ToString()

                Dim des = DirectCast(Newtonsoft.Json.JsonConvert.DeserializeObject(Recordings, GetType(List(Of GetAllRecordingDetailsOfaGroupCallRecord))), List(Of GetAllRecordingDetailsOfaGroupCallRecord))

                Return des
            Else
                Dim rec As New List(Of GetAllRecordingDetailsOfaGroupCallRecord)()
                rec.Add(result)
                Return rec
            End If
        End Function

        'Delete recording file by using GroupCallUUID and RecordingUUID

        Public Function DeleteRecordingOfGroupCall(GroupObj As GroupCall) As DeleteRecordingOfGroupCallResult Implements IApiActions.DeleteRecordingOfGroupCall
            Dim result As New DeleteRecordingOfGroupCallResult()
            Dim httpResponse As JObject = Nothing

            Dim payload As New JObject()
            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If
            If GroupObj.RecordingUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.RecordingUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.RecordingUUID) Then
                Throw New ArgumentNullException("Please Enter  RecordingUUID")
            End If

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.DeleteData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Recordings/" & Convert.ToString(GroupObj.RecordingUUID) & "/", "DELETE", payload.ToString(), Nothing)
            If httpResponse Is Nothing Then
                result.Message = "Deleted"
            Else
                result.Message = httpResponse.SelectToken("Message").ToString()
            End If
            Return result
        End Function



        'Delete recording file by using GroupCallUUID
        Public Function DeleteAllRecordingOfGroupCall(GroupObj As GroupCall) As DeleteAllRecordingOfGroupCallResult Implements IApiActions.DeleteAllRecordingOfGroupCall
            Dim result As New DeleteAllRecordingOfGroupCallResult()
            Dim httpResponse As JObject = Nothing

            Dim payload As New JObject()
            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If


            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.DeleteData(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Recordings/", "DELETE", payload.ToString(), Nothing)
            If httpResponse Is Nothing Then
                result.Message = "Deleted"
            Else
                result.Message = httpResponse.SelectToken("Message").ToString()
            End If

            Return result
        End Function



        'Disconnect all participants from a Group Call using GroupCallUUID

        Public Function DisconnectAllParticitantFromGroupCall(GroupObj As GroupCall) As GetAllDisconnectParticitantFromGroupCallResult Implements IApiActions.DisconnectAllParticitantFromGroupCall
            Dim result As New GetAllDisconnectParticitantFromGroupCallResult()
            Dim httpResponse As JObject = Nothing


            Dim payload As New JObject()
            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If

            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.PatchDataDisconnectparticipitant(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Hangup/", "PATCH", payload.ToString(), Nothing)


            result.Message = httpResponse.SelectToken("Message").ToString()

            Return result
        End Function


        ' Disconnect  participants from a Group Call using GroupCallUUID and ParticipantId
        Public Function DisconnectParticitantFromGroupCall(GroupObj As GroupCall) As GetDisconnectParticitantFromGroupCallResult Implements IApiActions.DisconnectParticitantFromGroupCall
            Dim result As New GetDisconnectParticitantFromGroupCallResult()
            Dim httpResponse As JObject = Nothing
            If GroupObj.GroupCallUUID Is Nothing OrElse String.IsNullOrEmpty(GroupObj.GroupCallUUID) OrElse String.IsNullOrWhiteSpace(GroupObj.GroupCallUUID) Then
                Throw New ArgumentNullException("Please Enter  GroupCallUUID")
            End If
            If GroupObj.ParticipantId Is Nothing OrElse String.IsNullOrEmpty(GroupObj.ParticipantId) OrElse String.IsNullOrWhiteSpace(GroupObj.ParticipantId) Then
                Throw New ArgumentNullException("Please Enter  ParticipantId")
            End If
            Dim payload As New JObject()
            Me._httpClient.AuthorizationHeader(Me._authKey, Me._authToken)
            httpResponse = Me._httpClient.PatchDataDisconnectparticipitant(Me.BaseUrl & "GroupCalls/" & Convert.ToString(GroupObj.GroupCallUUID) & "/Participants/" & Convert.ToString(GroupObj.ParticipantId) & "/Hangup/", "PATCH", payload.ToString(), Nothing)
            result.Message = httpResponse.SelectToken("Message").ToString()
            Return result
        End Function

#Region "PROPERTIES"
        Private ReadOnly Property BaseUrl() As String
            Get
                If Me._baseUrl.Length = 0 Then
                    Me._baseUrl = If(Me._domain.EndsWith("/"), Me._domain, Me._domain & "/")
                    Me._baseUrl += Me._version & "/Accounts/" & Me._authKey & "/"
                End If
                Return Me._baseUrl
            End Get
        End Property
#End Region
    End Class
End Namespace
