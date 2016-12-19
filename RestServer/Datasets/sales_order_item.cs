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
    public class sales_order_item
    {
        [PrimaryKey]
        [Column(DataTypes.INT, Defaults.AUTO_INCREMENT)]
        public int item_id { get; set; }

        [Column(DataTypes.INT)]
        [ForeignKey("FK_ITEM_ORDER_ID_ORDER_ID", typeof(sales_flat_order), "order_id")]
        public int order_id { get; set; }

        [Column(DataTypes.INT)]
        [ForeignKey("FK_ITEM_CAR_ID_CAR_ID", typeof(sales_catalog_car), "car_id")]
        public int car_id { get; set; }

        [Column(DataTypes.TIMESTAMP, Defaults.NULL)]
        public DateTime hired_from { get; set; }

        [Column(DataTypes.TIMESTAMP, Defaults.NULL)]
        public DateTime hired_to { get; set; }
    }
}
