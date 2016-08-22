using System;
using System.Web;
using System.Web.UI;
using System.Data;
using System.IO;
using System.Data.SqlClient;

public class BertilBase : Page {

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    HttpContext.Current.Response.AddHeader("p3p","CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");  
  }
  
  protected String GetJSData(int tavlingid) {
    DataSet ds = DB.GetDS("select * from bertil_games where id=" + tavlingid);
    String js = "var tavling = {'shortname':'" + DB.GetString(ds, 0, "tavling") + "', 'rubrik':'" + DB.GetString(ds, 0, "rubrik") + "', 'logo':'" + DB.GetString(ds, 0, "logo") + "', 'intro':'" + DB.GetString(ds, 0, "intro") + "', 'info':'" + DB.GetString(ds, 0, "info") + "', 'descript':'" + DB.GetString(ds, 0, "descript") + "', 'tips':'" + DB.GetString(ds, 0, "tips") + "'};";

    ds = DB.GetDS("select * from bertil_questions where tavlingid=" + tavlingid + " order by orderno");
    js += "var questions = [";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      if (i > 0) js += ",";
      js += "{q1:'" + DB.GetString(ds, i, "fraga1") + "', q2:'" + DB.GetString(ds, i, "fraga2") + "', a1:'" + DB.GetString(ds, i, "svar1").Replace("'","\\'") + "', a2:'" + DB.GetString(ds, i, "svar2") + "'}";
    }
    js += "];";
    return js;
  }
  
}

