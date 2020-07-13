using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DataCommon
{
    public class Gensql
    {
        public static string InsertSql(object entity, string tableName)
        {
            var props = entity.GetType().GetProperties();
            var sb = new StringBuilder();
            sb.Append(string.Format("insert into {0} ", tableName));
            string insertparams = "";
            string insertvalues = "";
            object ob = null;
            for (var i = 0; i < props.Count(); i++)
            {
                var property = props.ElementAt(i);
                if (property.GetCustomAttributes(true).Where(a => a is KeyAttribute).Any()) continue;
                ob = property.GetValue(entity, null);
                if (ob == null) continue;
                if (property.PropertyType.Name.ToLower() == "datetime")
                {
                    var dt = (DateTime)ob;
                    if (dt.Year < 1900)
                        continue;
                }
                insertparams = insertparams + property.Name + ",";
                insertvalues = insertvalues + "@" + property.Name + ",";
            }
            sb.Append(string.Format("({0}) values ({1})", insertparams.TrimEnd(','), insertvalues.TrimEnd(',')));
            return sb.ToString();
        }
        public static string UpdateSql(object entity, string tableName, string where)
        {
            var props = entity.GetType().GetProperties();
            var sb = new StringBuilder();
            sb.Append(string.Format("update {0} ", tableName));
            string updateparams = "";
            object ob = null;
            for (var i = 0; i < props.Count(); i++)
            {
                var property = props.ElementAt(i);
                if (property.GetCustomAttributes(true).Where(a => a is KeyAttribute).Any()) continue;
                ob = property.GetValue(entity, null);
                if (ob == null) continue;
                if (property.PropertyType.Name.ToLower() == "datetime")
                {
                    var dt = (DateTime)ob;
                    if (dt.Year < 1900)
                        continue;
                }
                updateparams = updateparams + property.Name + "=@" + property.Name + ",";
            }
            sb.Append(string.Format(" set {0} where {1}", updateparams.TrimEnd(','), where));
            return sb.ToString();
        }
    }
}
