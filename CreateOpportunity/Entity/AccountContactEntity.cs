using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateOpportunity.Entity
{
    public class AccountContactEntity
    {
        public List<AccountEntity> accounts{ get; set; }
        public List<ContactEntity> contacts{ get; set; }
    }
}
