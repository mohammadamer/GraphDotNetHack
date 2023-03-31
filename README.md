# GraphDotNetHack

# Meeting Recording Notifier

https://user-images.githubusercontent.com/19314043/227954837-09fda710-5461-48f3-ad69-b35d68c85ea2.mp4

This sample demonstrates how to use the Microsoft Graph .NET SDK to get a notification email about the meeting recording URL.

> **NOTE:** This sample is still has points to be imporved and it's for running locally.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [ngrok](https://ngrok.com/)

## App registrations

This sample requires an Azure AD application registration:

1. Open a browser and navigate to the [Azure Active Directory admin center](https://aad.portal.azure.com) and login using an Microsoft 365 tenant organization admin.

2. Select **Azure Active Directory** in the left-hand navigation, then select **App registrations** .


### Register an app for the Azure Function Notifier

1. go to **App Registrations**, and select **New registration**. On the **Register an application** page, set the values as follows.

    - Set **Name** to `Graph Azure Function Notifier`.
    - Set **Supported account types** to **Accounts in this organizational directory only**.
    - Leave **Redirect URI** blank.

1. Select **Register**. On the **Graph Azure Function Notifier** page, copy the value of the **Application (client) ID** and save it, you will need it in the next step.

1. Select **Certificates & secrets** under **Manage**. Select the **New client secret** button. Enter a value in **Description** and select one of the options for **Expires** and select **Add**.

1. Copy the client secret value before you leave this page. You will need it in the next step.

1. Select **API Permissions** under **Manage**. Choose **Add a permission**.

1. Select **Microsoft Graph**, then **Application Permissions**. Add **User.Read.All** and **Mail.ReadWrite** and **Mail.send** and **Chat.Read.All** and **Chat.ReadWrite.All**, **ChatMessage.Read.All** then select **Add permissions**.

1. In the **Configured permissions**, remove the delegated **User.Read** permission under **Microsoft Graph** by selecting the **...** to the right of the permission and selecting **Remove permission**. Select **Yes, remove** to confirm.

1. Select the **Grant admin consent for...** button, then select **Yes** to grant admin consent for the configured application permissions. The **Status** column in the **Configured permissions** table changes to **Granted for ...**.

## Configure the sample
1. Create Self Signed Certificate using the Powershell script [Create-SelfSignedCertificate.ps1](MSGraph.MeetingRecordingNotifier/MeetingRecordingNotifier/Scripts/Create-SelfSignedCertificate.ps1)
1. copy the the generated CertificateThumbprint. You will need it in the next step.

1. Use `dotnet user-secrets init` and `dotnet user-secrets set` in the **MSGraph.MeetingRecordingNotifier** directory to set the following values.

    - dotnet user-secrets init --project "project path"
    - dotnet user-secrets set --project "project path" "webhookClientId" "webhookClientIdValue"
    - dotnet user-secrets set --project "project path" "webhookClientSecret" "webhookClientSecretValue"
    - dotnet user-secrets set --project "project path" "tenantId" "tenantIdIdValue"
    - dotnet user-secrets set --project "project path" "ngrokUrl" "ngrokUrlIdValue"
    - dotnet user-secrets set --project "project path" "CertificateThumbprint" "CertificateThumbprintValue"
    - dotnet user-secrets set --project "project path" "ClientState" "ClientStateValue"
    - dotnet user-secrets set --project "project path" "MessageSubject" "MessageSubjectValue"
    - dotnet user-secrets set --project "project path" "MessageBody" ```"<p>Hello!</p><p><span> Please find the {0} meeting recording <a href=\"{1}\">URL.</a></span></p>"```


1. Run ngrok using the following command.

    ```bash
    ngrok http 7071
    ```

> **NOTE**
> If you restart ngrok, you will need to update the `ngrokUrl` value in user secrets with the new ngrok URL and restart the Azure Function project.

## Run the sample

The following command (run in the **GraphSampleFunctions** directory) will start the Azure Function project locally on your machine.
1. run the azure function from visual studio or from VS.
```bash
func start
```

2. run a post request using SetSubacription with a payload in the body then a new subscription vaild for one hour will be created.

```{"requestType": "subscribe"}```

