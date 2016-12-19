using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rest.Database;
using Rest.Database.Constants;
using Rest.Web;

namespace Server.Datasets
{
    [DataBaseEntry]
    [AutoGenerate]
    [Form(Constants.METHOD_PUT, "corecustomer")]
    class core_customer : DataSet
    {
        [PrimaryKey]
        [Column(DataTypes.INT, Defaults.AUTO_INCREMENT)]
        public int customer_id { get; set; }

        [Column(DataTypes.VARCHAR, Default="NULL")]
        [Field(FieldTypes.TYPE_TEXT, "Username")]
        public string customer_name { get; set; }

        [Hidden]
        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_PASSWORD, "Password")]
        public string customer_password { get; set; }

        [UniqueKey]
        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Email address")]
        public string customer_email { get; set; }

        [Column(DataTypes.TINYINT)]
        public int is_admin { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Woonplaats")]
        public string address_city { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Straatnaam")]
        public string address_street { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Huisnummer")]
        public string address_number { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Postcode")]
        public string address_postal { get; set; }

        [Column(DataTypes.TIMESTAMP, Defaults.CURRENT_TIMESTAMP)]
        public DateTime created { get; set; }

        [Column(DataTypes.TIMESTAMP, Defaults.CURRENT_TIMESTAMP_UPDATE)]
        public DateTime updated { get; set; }
    }
}
