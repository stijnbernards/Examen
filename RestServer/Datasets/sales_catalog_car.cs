using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rest.Database;
using Rest.Database.Constants;

namespace RestServer.Datasets
{
    [DataBaseEntry]
    [AutoGenerate]
    public class sales_catalog_car : DataSet
    {
        [PrimaryKey]
        [Column(DataTypes.INT, Defaults.AUTO_INCREMENT)]
        public int car_id { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string car_brand { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string car_license_plate { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string car_type { get; set; }

        [Column(DataTypes.DECIMAL)]
        public decimal car_day_price { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string car_image { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string car_description { get; set; }
    }
}
