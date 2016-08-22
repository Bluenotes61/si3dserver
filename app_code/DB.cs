/* $Date: 2010-10-04 12:38:42 +0200 (må, 04 okt 2010) $    $Revision: 7006 $ */
using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;

public class DB {

  public static String FixApostrophe(String value) {
    int idx1 = value.IndexOf("'");
    while (idx1 >= 0 && value.Length > idx1+1) {
      int idx2 = value.IndexOf("'", idx1+1);
      if (idx2 == idx1+1) 
        idx1 = idx2+1;
      else {
        value = value.Insert(idx1, "'");
        idx1 += 2;
      }
      if (idx1 < value.Length) idx1 = value.IndexOf("'", idx1);
      else idx1 = -1;
    }
    return value;
  }

  public static DataSet GetDS(String sqlStr) {
    return GetDS(sqlStr, 0, 0, "", System.Configuration.ConfigurationManager.AppSettings["connString"]);
  }

  public static DataSet GetDS(String sqlStr, String connStr) {
    return GetDS(sqlStr, 0, 0, "", connStr);
  }

  public static DataSet GetDS(String sqlStr, int startRow, int nOfRows, String table) {
    return GetDS(sqlStr, startRow, nOfRows, table, System.Configuration.ConfigurationManager.AppSettings["connString"]);
  }

  public static DataSet GetDS(String sqlStr, int startRow, int nOfRows, String table, String connStr) {
    DataSet ds = new DataSet();
    SqlConnection conn = new SqlConnection(connStr);
    try {
      SqlCommand sql = new SqlCommand(sqlStr, (SqlConnection)conn);
      SqlDataAdapter adapter = new SqlDataAdapter(sql);
      if ((startRow > 0 || nOfRows > 0) && table.Length > 0)
        adapter.Fill(ds, startRow, nOfRows, table);
      else
        adapter.Fill(ds);
    }
    catch (Exception ex) {
      throw new Exception(ex.Message);
    }
    finally {
      conn.Close();
    }
    return ds;
  }


  public static void ExecSql(String sqlStr) {
    ExecSql(sqlStr, System.Configuration.ConfigurationManager.AppSettings["connString"]);
  }

  public static void ExecSql(String sqlStr, String connStr) {
    SqlConnection conn = new SqlConnection(connStr);
    conn.Open();
    try {
      SqlCommand comm = new SqlCommand(sqlStr, (SqlConnection)conn);
      comm.ExecuteNonQuery();
    }
    catch (Exception ex) {
      throw new Exception(sqlStr + ":" + ex.Message);
    }
    finally {
      conn.Close();
    }
  }

  public static DataRow GetRow(DataSet ds, int row) {
    return ds.Tables[0].Rows[row];
  }

  public static bool IsNull(DataSet ds, int row, String col) {
    return ds.Tables[0].Rows[row][col] == DBNull.Value;
  }

  public static String GetString(DataSet ds, int row, String col) {
    return ds.Tables[0].Rows[row][col].ToString();
  }

  public static String GetString(DataSet ds, int row, int col) {
    return ds.Tables[0].Rows[row].ItemArray[col].ToString();
  }

  public static String GetValAsString(DataSet ds, int row, String col) {
    if (IsNull(ds, row, col)) return "null";
    String typename = ds.Tables[0].Columns[col].DataType.FullName.ToLower();
    if (typename.Contains("decimal") || typename.Contains("double") || typename.Contains("single") || typename.Contains("int"))
      return ds.Tables[0].Rows[row][col].ToString();
    else if (typename.Contains("bool"))
      return Convert.ToInt32(ds.Tables[0].Rows[row][col]).ToString();
    else
      return "'" + ds.Tables[0].Rows[row][col].ToString() + "'";
  }

  public static String GetString(String sqlStr, String col, String connStr) {
    DataSet ds = GetDS(sqlStr, 0, 0, "", connStr);
    if (GetRowCount(ds) == 0) return "";
    else return GetString(ds, 0, col);
  }

  public static String GetString(String sqlStr, String col) {
    DataSet ds = GetDS(sqlStr);
    if (GetRowCount(ds) == 0) return "";
    else return GetString(ds, 0, col);
  }

  public static DateTime GetDate(DataSet ds, int row, String col) {
    try {
      return ((DateTime)ds.Tables[0].Rows[row][col]);
    }
    catch {
      return DateTime.MinValue;
    }
  }

  public static DateTime GetDate(String sqlStr, String col, DateTime defaultval) {
    DataSet ds = GetDS(sqlStr);
    try {
      DateTime aDate = GetDate(ds, 0, col);
      if (aDate == DateTime.MinValue) return defaultval;
      else return aDate;
    }
    catch { return defaultval; }
  }

  public static String GetDateString(DataSet ds, int row, String col, bool withtime) {
    try {
      DateTime dt = ((DateTime)ds.Tables[0].Rows[row][col]);
      if (dt.Equals(DateTime.MinValue)) return "";
      if (withtime) return dt.ToString("yyyy-MM-dd HH:mm:ss");
      else return dt.ToString("yyyy-MM-dd");
    }
    catch {
      return "";
    }
  }

  public static bool GetBoolean(DataSet ds, int row, String col) {
    Object o = ds.Tables[0].Rows[row][col];
    if (o == DBNull.Value)
      return false;
    else
      return Convert.ToBoolean(o);
  }


  public static bool GetBoolean(String sqlStr, String col, bool defaultval) {
    DataSet ds = GetDS(sqlStr);
    if (GetRowCount(ds) > 0) return GetBoolean(ds, 0, col);
    else return defaultval;
  }

  public static int GetInt(DataSet ds, int row, String col) {
    Object o = ds.Tables[0].Rows[row][col];
    if (o == DBNull.Value)
      return 0;
    else
      return Convert.ToInt32(o);
  }

  public static double GetDouble(DataSet ds, int row, String col) {
    Object o = ds.Tables[0].Rows[row][col];
    if (o == DBNull.Value)
      return 0;
    else {
      try { return Convert.ToDouble(o); }
      catch { return 0; }
    }
  }

  public static int GetInt(DataSet ds, int row, String col, int defaultval) {
    try {
      Object o = ds.Tables[0].Rows[row][col];
      if (o == DBNull.Value)
        return defaultval;
      else
        return Convert.ToInt32(o);
    }
    catch { return defaultval; }
  }


  public static int GetInt(String sqlStr, String col) {
    DataSet ds = GetDS(sqlStr);
    return GetInt(ds, 0, col, 0);
  }

  public static double GetDouble(String sqlStr, String col) {
    DataSet ds = GetDS(sqlStr);
    return GetDouble(ds, 0, col);
  }

  public static bool GetBoolean(String sqlStr, String col) {
    DataSet ds = GetDS(sqlStr);
    return GetBoolean(ds, 0, col);
  }

  public static int GetRowCount(DataSet ds) {
    return (ds.Tables.Count > 0 ? ds.Tables[0].Rows.Count : 0);
  }

  public static bool RowExists(String sqlStr) {
    DataSet ds = GetDS(sqlStr);
    return (GetRowCount(ds) > 0);
  }
}
