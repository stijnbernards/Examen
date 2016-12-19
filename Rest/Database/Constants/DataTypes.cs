using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Database.Constants
{
    public static class DataTypes
    {
        //Integer

        public const string TINYINT = "TINYINT";
        public const string SMALLINT = "SMALLINT";
        public const string MEDIUMINT = "MEDIUMINT";
        public const string INT = "INT";
        public const string BIGINT = "BIGINT";
        public const string BIT = "BIT";

        //Real

        public const string FLOAT = "FLOAT";
        public const string DOUBLE = "DOUBLE";
        public const string DECIMAL = "DECIMAL";

        //Text

        public const string CHAR = "CHAR";
        //TODO fix with implicit operator or something...
        public const string VARCHAR = "VARCHAR(50)";
        public const string TINYTEXT = "TINYTEXT";
        public const string TEXT = "TEXT";
        public const string MEDIUMTEXT = "MEDIUMTEXT";
        public const string LONGTEXT = "LONGTEXT";

        //Binary

        public const string BINARY = "BINARY";
        public const string VARBINARY = "VARBINARY";
        public const string TINYBLOB = "TINYBLOB";
        public const string BLOB = "BLOB";
        public const string MEDIUMBLOB = "MEDIUMBLOB";
        public const string LONGBLOB = "LONGBLOB";

        //Temporal

        public const string DATE = "DATE";
        public const string TIME = "TIME";
        public const string YEAR = "YEAR";
        public const string DATETIME = "DATETIME";
        public const string TIMESTAMP = "TIMESTAMP";

    }
}
