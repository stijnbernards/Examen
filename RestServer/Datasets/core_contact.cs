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
    public class core_contact : DataSet
    {
        [PrimaryKey]
        [Column(DataTypes.INT, Defaults.AUTO_INCREMENT)]
        public int contact_id { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string contact_name { get; set; }

        [Column(DataTypes.VARCHAR)]
        public string contact_email{ get; set; }

        [Column(DataTypes.VARCHAR)]
        public string contact_message { get; set; }
    }
}
