using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rest.Database;
using Rest.Database.Constants;
using Server.Datasets;

namespace RestServer.Datasets
{
    [DataBaseEntry]
    [AutoGenerate]
    public class sales_flat_order
    {
        [PrimaryKey]
        [Column(DataTypes.INT, Defaults.AUTO_INCREMENT)]
        public int order_id { get; set; }

        [Column(DataTypes.INT)]
        [ForeignKey("FK_ORDER_CUSTOMER_ID_CUSTOMER_ID", typeof(core_customer), "customer_id")]
        public int customer_id { get; set; }

        [Column(DataTypes.TIMESTAMP, Defaults.NULL)]
        public DateTime date { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string treated_by { get; set; }
    }
}
