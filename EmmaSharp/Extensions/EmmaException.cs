using System;
using RestSharp;

namespace EmmaSharp
{
    /// <summary>
    /// 
    /// </summary>
    public class EmmaException : Exception
    {
        private string message;

        /// <summary>
        /// 
        /// </summary>
        public RestResponse Response;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        public EmmaException(RestResponse response)
        {
            Response = response;
            message = "Unexpected response status " + ((int)response.StatusCode).ToString() + " with body:\n" + response.Content;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Message
        {
            get { return message; }
        }
    }
}
