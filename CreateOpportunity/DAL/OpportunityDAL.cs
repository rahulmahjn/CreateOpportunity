using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreateOpportunity.Entity;
using System.Data;
using System.Data.SqlClient;
using CreateOpportunity.Properties;

namespace CreateOpportunity.DAL
{
    public class OpportunityDAL
    {
        public List<OpportunityEntity> opportunityEntities = new List<OpportunityEntity>();

        public DataTable GetOpportunities()
        {
            DataTable dt = null;
            try
            {
                using (var conn = new SqlConnection(Settings.Default.DB))
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sf_GetOpportunities";
                    cmd.CommandTimeout = 90;
                    var adapter = new SqlDataAdapter(cmd);
                    conn.Open();
                    dt = new DataTable();
                    
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                throw ex ;
            }
            return dt;
        }
        public int UpdateSalesforceOpportunity(string OrderID,string RecordType,string ResponseMessage,DateTime? billingDate)
        {
            int result = 0;
            try
            {
                using (var conn = new SqlConnection(Settings.Default.DB))
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sf_UpdateOpportunity";
                    cmd.Parameters.Add("@OrderID", SqlDbType.VarChar).Value = OrderID;
                    cmd.Parameters.Add("@RecordType", SqlDbType.VarChar).Value = RecordType;
                    cmd.Parameters.Add("@Message", SqlDbType.VarChar).Value = ResponseMessage;
                    cmd.Parameters.Add("@BillingDate", SqlDbType.DateTime).Value = billingDate;
                    conn.Open();
                    result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
             return result;
        }
        public DateTime? GetLastDateInvoiced()
        {
            DateTime? lastInvoicedDate = null;
            try
            {
                using (var conn = new SqlConnection(Settings.Default.DB))
                {

                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "sf_GetLastInvoiceDate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if(reader["DateInvoiced"]==System.DBNull.Value)
                        {
                            lastInvoicedDate = DateTime.Today.AddDays(-1);
                        }
                        else
                        {
                            lastInvoicedDate = Convert.ToDateTime(reader["DateInvoiced"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lastInvoicedDate;
        }
      
    }
}
