using UnityEngine;
using System;
using System.Collections;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ArenaNet.Medley.Pool;
using System.Xml.Serialization;
using ArenaNet.Sts.Client;

public class StsTest : MonoBehaviour {
    private static readonly Uri EcosystemUri = new Uri("wss://cligate-dfw-2002.ncplatform.net/websocket");
    private static readonly RemoteCertificateValidationCallback AllTrustingValidationCallback =
      (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
      { return true; };

    private static readonly ObjectPool<byte[]> BufferPool = new ObjectPool<byte[]>(() => { return new byte[1024]; });
    public string message = "Bloop";

    [XmlRoot("Request")]
    public class BlankRequest
    {
        public string Message;
    }

    [XmlRoot("Request")]
    public class AuthLoginRequest
    {
        public string Provider { set; get; }        // The provider of the given login
        public string LoginName { set; get; }       // The username
        public string Password { set; get; }        // The password encoded in Base64
    }

    [XmlRoot("Message")]
    public class GenericResponse
    {
        public string Message;
    }

    // Use this for initialization
    void Start() {
        using (StsWebSocketClient client = new StsWebSocketClient(EcosystemUri, true, AllTrustingValidationCallback)
                .Connect().WaitForValue(TimeSpan.FromSeconds(5)))
        {

            
            StsRequestMessage loginRequest = StsRequestMessage.Create(BufferPool, "P", "/Auth/Login");
            loginRequest.TransactionId = new StsTransactionId("1");
            loginRequest.SetBodyAs(new AuthLoginRequest()
            {
                Provider = "Portal",
                LoginName = "mpbtest@arena.net",
                Password = "bXBidGVzdA=="
            });

            client.SendRequest(loginRequest).WaitForValue(TimeSpan.FromSeconds(5));
            

            StsRequestMessage request = StsRequestMessage.Create(BufferPool, "P", "/Game.lot.MyJsProto/Command");
            request.TransactionId = new StsTransactionId("2");
            request.SetBodyAs(new BlankRequest()
            {
                Message = message
            });

            request.Headers["connType"] = new[] { "401" };
            request.Headers["buildId"] = new[] { "1000" };
            request.Headers["programId"] = new[] { "2002" };
            request.Headers["host"] = new[] { "@ExGameGate" };

            StsResponseMessage response = client.SendRequest(request).WaitForValue(TimeSpan.FromSeconds(5));
            if (response != null)
            {
                GenericResponse gotSomething = response.ReadBodyAs<GenericResponse>();
                Debug.Log(gotSomething.Message);
            } else
            {
                Debug.Log("Message timed out");
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
