using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace EmmaSharp
{
    /// <summary>
    /// Base Class for APIs
    /// </summary>
	public partial class EmmaApi
    {
        private const string BaseUrl = "https://api.e2ma.net";

        readonly string _publicKey;
        readonly string _secretKey;
        readonly string _accountId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmmaApi"/> class.
        /// </summary>
        /// <param name="publicKey">The account's public key.</param>
        /// <param name="secretKey">The account's private key.</param>
        /// <param name="accountId">The account id.</param>
        public EmmaApi(string publicKey, string secretKey, string accountId)
        {
            _publicKey = publicKey;
            _secretKey = secretKey;
            _accountId = accountId;
        }


        /// <summary>
        /// Execute the Call to the Emma API. All methods return this base method.
        /// </summary>
        /// <typeparam name="T">The model or type to bind the return response.</typeparam>
        /// <param name="request">The RestRequest request.</param>
        /// <param name="start">If more than 500 results, use these parameters to start/end pages.</param>
        /// <param name="end">If more than 500 results, use these parameters to start/end pages.</param>
        /// <returns>Response data from the API call.</returns>
        private T Execute<T>(RestRequest request, int start = -1, int end = -1) where T : new()
        {
            // Explicitly set requests to TLS 1.1 or higher per Emma Documentation
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            var clientOptions = new RestClientOptions(BaseUrl)
            {
                //client.BaseUrl = new Uri(BaseUrl);
                Authenticator = new HttpBasicAuthenticator(_publicKey, _secretKey)
            };

            request.AddParameter("accountId", _accountId, ParameterType.UrlSegment); // used on every request

            if (start >= 0 && end >= 0) {
                request.AddQueryParameter("start", start.ToString());
                request.AddQueryParameter("end", end.ToString());
            }

            request.RequestFormat = DataFormat.Json;

            //request.JsonSerializer = new EmmaJsonSerializer(serializer);

            Func<IRestSerializer> serializer = () => {
                Newtonsoft.Json.JsonSerializer jsonSerializer = new Newtonsoft.Json.JsonSerializer();
                jsonSerializer.Converters.Add(new StringEnumConverter());
                return new EmmaJsonSerializer(jsonSerializer);
            };
            //var sco = new SerializerConfig().UseOnlySerializer(serializer);

            using var client = new RestClient(clientOptions,null,
                configureSerialization: (cfg) => cfg.UseOnlySerializer(serializer));

                                                            
            RestResponse<T> response = client.Execute<T>(request);
            
            Trace.WriteLine(response.Data);
            checkResponse(response);
            
            //T response = JsonConvert.DeserializeObject<T>(execute.Content);
            return response.Data;
        }

        private void checkResponse(RestResponse response)
        {
            int code = (int)response.StatusCode;
            if (code >= 400)
            {
                throw new EmmaException(response);
            }
        }
    }
}