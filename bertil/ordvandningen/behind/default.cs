using System;
using System.Web;
using System.Web.UI;
using System.Data;
using System.IO;
using System.Data.SqlClient;

public partial class DefaultPage : BertilBase {

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    ServerJs.Text = GetJSData(7);;
  }
  
}

