using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Database.Constants
{
    public static class Defaults
    {
        public const string NOT_NULL = "NOT_NULL";
        public const string NULL = "DEFAULT NULL";
        public const string AUTO_INCREMENT = "AUTO_INCREMENT";
        public const string CURRENT_TIMESTAMP = "DEFAULT CURRENT_TIMESTAMP";
        public const string CURRENT_TIMESTAMP_UPDATE = "DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP";
    }
}
