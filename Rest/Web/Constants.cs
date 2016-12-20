using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Web
{
    public class Constants
    {
        public const string STATUS_TRUE = "{ \"status\" : \"true\" }";
        public const string STATUS_FALSE = "{ \"status\" : \"false\" }";
        public const string STATUS_UNAUTHORIZED = "{ \"status\" : \"401 Not Authorized\" }";

        public const string METHOD_POST = "POST";
        public const string METHOD_PUT = "PUT";
        public const string METHOD_PATCH = "PATCH";
        public const string METHOD_DELETE = "DELETE";
        public const string METHOD_GET = "GET";

        public const string CONTENT_JAVASCRIPT = "application/javascript";
        public const string CONTENT_PDF = "application/pdf";
        public const string CONTENT_JSON = "application/json";
        public const string CONTENT_CSS = "text/css";
        public const string CONTENT_HTML = "Content-Type: text/html; charset=utf-8";

        public static class StatusCodes
        {
            public const int UNAUTHORIZED = 401;
            public const int OK = 200;
        }
    }
}
