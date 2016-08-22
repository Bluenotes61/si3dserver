using System;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Data.SqlClient;

public partial class DefaultPage : Page {

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    if (Request["x"] != "nollfyranoll") return;
    
    String sql = @"
      select s.*, c.name
      from si3d s 
      left outer join countrycodes c on
      c.code = s.countrycode 
      where s.deleted=0 order by s.gendate desc
    ";
    DataSet ds = DB.GetDS(sql);
    String js = "var images = [";
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      if (i > 0) js += ",";
      js += "{id:" + DB.GetInt(ds, i, "id") + ", ip:'" + DB.GetString(ds, i, "ip") + "', country:'" + DB.GetString(ds, i, "name") + "', filename:'" + DB.GetString(ds, i, "filename") + "', date:'" + DB.GetDate(ds, i, "gendate").ToString("yyyy-MM-dd HH:mm") + "'}";
    }
    js += "];";
    ImageJs.Text = js;
  }
  
  protected override void Render(HtmlTextWriter writer) {
    if (Request["sql"] != null) {
      Response.AddHeader("Content-type","text/js;charset=utf-8");
      writer.Write(NFN.GridUtils.GetGridData(Request, ""));
    }
    else 
      base.Render(writer);
  }
  
}

