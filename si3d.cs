using System;
using System.Web;
using System.Web.UI;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Net.Mail;

public partial class DefaultPage : Page {

  private double amplifyDepth = 0.1;

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);

    int nofFields;
    try { nofFields = Convert.ToInt32(Request["noffields"]); }
    catch { nofFields = 6; }
    
    String userId = Request["userid"];
    if (userId == null || userId.Length == 0) 
      userId = System.Guid.NewGuid().ToString();

    String toDelete = Request["todelete"];
    if (toDelete == null || toDelete.Length == 0) 
      toDelete = "1";

    try {    
      Bitmap drawing = new Bitmap(Request.Files["drawing"].InputStream);
      Bitmap template = null;
      if (Request.Files.Count > 1 && Request.Files["template"] != null && Request.Files["template"].InputStream.Length > 0)
        template = new Bitmap(Request.Files["template"].InputStream);

      Bitmap si3d = CreateSi3d(drawing, nofFields, template);
      String filename = "si3d/" + System.Guid.NewGuid().ToString() + ".jpg";
      SaveSi3d(si3d, filename, template != null, toDelete != "1");

      String ip = Request.UserHostAddress;
      String country = GetCountry(ip);
      DB.ExecSql("insert into si3d (userid, ip, countrycode, filename, gendate, todelete, deleted) values('" + userId + "', '" + ip + "', '" + country + "', '" + filename + "', GETDATE(), " + toDelete + ", 0)");

      drawing.Dispose();
      if (template != null) template.Dispose();
      si3d.Dispose();
      
      DeleteUnregisteredImages();
      
      Response.Write("http://" + Request.ServerVariables["HTTP_HOST"] + "/" + filename + ";" + userId);
    }
    catch(Exception ex) {
//      Response.Write("ERROR: " + ex.Message);
      Response.Write("It it currently not possible to generate SI3Ds due to a server problem. Please try later and sorry for the inconvenience...");
    }
  }
  
  private void SaveSi3d(Bitmap si3d, String filename, bool useTemplate, bool hasLicense) {
    int imageQuality = 100;
    if (hasLicense)
      imageQuality = (useTemplate ? 90 : 10);
    else
//      imageQuality = (useTemplate ? 10 : 1);
      imageQuality = (useTemplate ? 60 : 10);
    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
    EncoderParameters myEncoderParameters = new EncoderParameters(1);
    EncoderParameter qualityParam = new EncoderParameter (System.Drawing.Imaging.Encoder.Quality, imageQuality);       
    myEncoderParameters.Param[0] = qualityParam;      
    si3d.Save(Server.MapPath(filename), jpgEncoder, myEncoderParameters);
  }
  
  private ImageCodecInfo GetEncoder(ImageFormat format) {
    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
    foreach (ImageCodecInfo codec in codecs) {
      if (codec.FormatID == format.Guid)
        return codec;
    }
    return null;
  }

  private String GetCountryOld(String ip) {
    try {
      String url = "http://api.hostip.info/country.php?ip=" + ip;
      Uri uri = new Uri(url);
      HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
      WebResponse response = myRequest.GetResponse();

      Stream stream = response.GetResponseStream();

      Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

      StreamReader readStream = new StreamReader( stream, encode );
      Char[] read = new Char[5];
      readStream.Read( read, 0, 5 );
      String res = new String(read);

      stream.Close();
      response.Close(); 

      return res.Trim();
    }
    catch {
      return "";
    }
  }

  private String GetCountry(String ip) {
    try {
      String url = "http://api.ipinfodb.com/v3/ip-country/?key=8d497b124eec47eacf0f70849f2620822e2ef21a5ed6b968693d275c6b8d3ca8&ip=" + ip;
      Uri uri = new Uri(url);
      HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
      WebResponse response = myRequest.GetResponse();

      Stream stream = response.GetResponseStream();

      Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

      StreamReader readStream = new StreamReader( stream, encode );
      Char[] read = new Char[126];
      readStream.Read( read, 0, 126 );
      String res = new String(read);
      String[] ares = res.Split(';');

      stream.Close();
      response.Close(); 

      return ares[3];
    }
    catch {
      return "";
    }
  }
  
  private void DeleteUnregisteredImages() {
    DataSet ds = DB.GetDS("select * from si3d where todelete=1 and deleted=0 and DATEDIFF(minute, gendate, GETDATE()) > 10000");
    for (int i=0; i < DB.GetRowCount(ds); i++) {
      try { 
        File.Delete(Server.MapPath(DB.GetString(ds, i, "filename")));
        DB.ExecSql("update si3d set deleted=1 where id=" + DB.GetString(ds, i, "id"));
      }
      catch{}
    }
  }
  
  private Bitmap GetRandomImg(int width, int height, int fieldWidth) {
    Random rnd = new Random();
    Bitmap bmp = new Bitmap(width, height);
    for (int x=0; x < fieldWidth; x++) {
      for (int y=0; y < height; y++) {
        if (rnd.Next(2) == 0) bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
        else bmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
      }
    }
    for (int x=fieldWidth; x < width; x++) 
      for (int y=0; y < height; y++) 
        bmp.SetPixel(x, y, bmp.GetPixel(x-fieldWidth, y));
    return bmp;
  }
  
  private Bitmap GetTemplateImg(int totWidth, Bitmap template) {
    Bitmap res = new Bitmap(totWidth, template.Height);
    using (Graphics g = Graphics.FromImage(res)) {
      int x = 0;
      while (x < totWidth) {
        g.DrawImage(template, new Rectangle(x, 0, template.Width, template.Height), 0, 0, template.Width, template.Height, GraphicsUnit.Pixel);
        x += template.Width;
      }
    }
    return res;
  }

  private Bitmap CreateSi3d(Bitmap drawing, int nofFields, Bitmap template) {
    int fieldWidth = (int)Math.Ceiling(1.0*drawing.Width/nofFields);
    Bitmap si3d = (template == null ? GetRandomImg(drawing.Width, drawing.Height, fieldWidth) : GetTemplateImg(drawing.Width, template));
    
    for (int y=0; y < drawing.Height; y++) {
      for (int x=drawing.Width-1; x >= 0; x--) {
        int copyx = x + fieldWidth + GetDepth(drawing.GetPixel(x, y));
        if (copyx >= 0 && copyx < drawing.Width) 
          si3d.SetPixel(x, y, si3d.GetPixel(copyx, y));
      }
    }
    return si3d;
  }
  
  private int GetDepth(Color col) {
    return (int)Math.Round((128 - col.R)*amplifyDepth);
  }

  public static void Debug(String txt) {
    HttpServerUtility server = HttpContext.Current.Server;
    StreamWriter sw = File.AppendText(server.MapPath("/debug.txt"));
    sw.WriteLine(txt);
    sw.Close();    
  }

}

