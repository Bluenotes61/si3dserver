// $Date: 2013-03-04 15:15:20 +0100 (m√•, 04 mar 2013) $    $Revision: 9371 $ 
namespace NFN {

  using System;
  using System.Web;
  using System.Data;
  using System.Collections;

  /********************************************************************************
  *
  * Class GridUtils
  *
  *********************************************************************************/

  public class GridUtils {

    private static int GetInt(String s, int def) {
      int res = def;
      try { res = Convert.ToInt32(s); }
      catch { res = def; }
      return res;
    }

    private static String GetJson(String id, String[] elems) {
      String res = "{'id':'" + id + "', 'cell':[";
      for (int i=0; i < elems.Length; i++) {
        if (i > 0) res += ",";
        res += "'" + elems[i].Replace("\"", "&quote;") + "'";
      }
      res += "]}";
      return res;
    }

    private static String GetCondition(String item) {
      //"field":"title","op":"eq","data":"ss" 
      String[] elems = item.Replace("}","").Replace("{","").Split(',');
      String field = elems[0].Split(':')[1].Replace("\"","");
      String op = elems[1].Split(':')[1].Replace("\"","");
      String val = elems[2].Split(':')[1].Replace("\"","");

      String res = field + " ";
      if (op == "eq") res += "= '" + val + "'";
      else if (op == "ne") res += "<> '" + val + "'";
      else if (op == "lt") res += "< " + val;
      else if (op == "le") res += "<= " + val;
      else if (op == "gt") res += "> " + val;
      else if (op == "ge") res += ">= " + val;
      else if (op == "bw") res += "like '" + val + "%'";
      else if (op == "bn") res += "not like '" + val + "%'";
      else if (op == "in") res += "in ('" + val + "')";
      else if (op == "ni") res += "not in ('" + val + "')";
      else if (op == "ew") res += "like '%" + val + "'";
      else if (op == "en") res += "not like '%" + val + "'";
      else if (op == "cn") res += "like '%" + val + "%'";
      else if (op == "nc") res += "not like '%" + val + "%'";

      return res;
    }

    public static String GetConditions(String json) {
      //{"groupOp":"AND","rules":[{"field":"title","op":"eq","data":"ss"},{"field":"undefined","op":"eq","data":"dd"}]}
      String res = "";
      json = json.Substring(12);
      String gop = (json.StartsWith("AND") ? " and " : " or ");
      json = json.Substring(json.IndexOf("[")+1);
      json = json.Substring(0, json.Length-2);
      json = json.Replace("},","}|");
      if (json.Length > 0) {
        String[] items = json.Split('|');
        for (int i=0; i < items.Length; i++) {
          if (res.Length > 0) res += gop;
          res += GetCondition(items[i]);
        }
      }
      else
        res = "1=1";
      return res;
    }

    public static DataSet GetGridDS(HttpRequest request, String condition) {
      String sql = request["sql"];
      return GetGridDS(request, sql, condition);
    }

    public static DataSet GetGridDS(HttpRequest request, String sql, String condition) {
      String sidx = request["sidx"];  // get index row - i.e. user click to sort 
      String sord = request["sord"];  // get the direction 
      String filters = request["filters"];

      if (!sql.EndsWith(" ")) sql += " ";
      if (!sql.ToLower().Contains(" where ")) sql += "where 1=1 ";
      if (filters != null && filters.Length > 0) 
        sql += "and " + GetConditions(filters) + " ";
      if (condition.Length > 0)
        sql += "and " + condition + " ";
      if (sidx != null && sidx.Length > 0)
        sql += "order by " + sidx + " " + sord;

      return DB.GetDS(sql);
    }

    public static String GetGridData(HttpRequest request, String condition, Hashtable funcs) {
      return GetGridData(GetGridDS(request, condition), request, funcs);
    }

    public static String GetGridData(HttpRequest request, String condition) {
      return GetGridData(GetGridDS(request, condition), request, null);
    }

    public delegate String ConvertColFunc(String value);

    public static String GetGridData(DataSet ds, HttpRequest request, Hashtable funcs) {
      String idcol = request["idcol"];
      String[] cols = request["cols"].Split(',');
      bool editable = request["editable"] == "true";
      int page = GetInt(request["page"], 1);  // get the requested page 
      int maxnof = GetInt(request["rows"], 0);   // get how many rows we want to have into the grid 

      int rowcount = DB.GetRowCount(ds);
      int start = maxnof*(page - 1);
      int last = start+maxnof;
      if (last > rowcount) last = rowcount;

      int totpages = (rowcount > 0 ? Convert.ToInt32(Math.Floor(Convert.ToDouble(rowcount/maxnof)))+1 : 0);

      String data = "";
      for (int i=start; i < last; i++) {
        if (data.Length > 0) data += ",";
        String id = (idcol.Length > 0 ? DB.GetString(ds, i, idcol) : i.ToString());
        int nofcols = (editable ? cols.Length+1 : cols.Length);
        String[] coldata = new String[nofcols];
        if (editable) coldata[0] = "";
        int j0 = (editable ? 1 : 0);
        for (int j=0; j < cols.Length; j++) {
          coldata[j+j0] = DB.GetString(ds, i, cols[j]).Replace("'","¥");
          if (funcs != null && funcs.ContainsKey(cols[j])) {
            ConvertColFunc f = (ConvertColFunc)funcs[cols[j]];
            coldata[j+j0] = f(coldata[j+j0]);
          }
        }
        data += GetJson(id, coldata);
      }
      String res = "{'page':'" + page + "','total': '" + totpages + "','records':'" + rowcount.ToString() + "','rows' : [" + data + "]}";
      return res.Replace("'","\"");
    }

  }
}