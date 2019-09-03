using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateOpportunity.Entity
{
    public class MissingRecordEntity
    {
        public int OrderID { get; set; }
        public List<RecordEntity> RecordEntities { get; set; }
    }
}
