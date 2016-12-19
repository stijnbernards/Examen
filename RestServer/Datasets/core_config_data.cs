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
    public class core_config_data
    {
        [PrimaryKey]
        [Column(DataTypes.INT, Defaults.AUTO_INCREMENT)]
        public int config_id { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string config_name { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string config_value { get; set; }
    }
}
