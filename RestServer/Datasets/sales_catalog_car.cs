using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rest.Database;
using Rest.Database.Constants;
using Rest.Web;

namespace RestServer.Datasets
{
    [DataBaseEntry]
    [AutoGenerate]
    [Form(Constants.METHOD_PUT, "/salescatalogcar")]
    public class sales_catalog_car : DataSet
    {
        [PrimaryKey]
        [Column(DataTypes.INT, Defaults.AUTO_INCREMENT)]
        public int car_id { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Auto merk")]
        public string car_brand { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Auto kenteken")]
        public string car_license_plate { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Auto type")]
        public string car_type { get; set; }

        [Column(DataTypes.DECIMAL)]
        [Field(FieldTypes.TYPE_TEXT, "Auto dag prijs")]
        public decimal car_day_price { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Auto foto")]
        public string car_image { get; set; }

        [Column(DataTypes.VARCHAR)]
        [Field(FieldTypes.TYPE_TEXT, "Auto beschrijving")]
        public string car_description { get; set; }


        [Field(FieldTypes.TYPE_HIDDEN, "")]
        public string _REDIRECT => "";
    }
}
