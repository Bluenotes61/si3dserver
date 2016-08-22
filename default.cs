using System;
using System.Web;
using System.Web.UI;
using System.Data;
using System.IO;
using System.Data.SqlClient;

public partial class DefaultPage : Page {

  private int INITIAL_RANK = 500;
  private int MAX_CHALLENGES = 5;

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    
    String op = Request["op"];
    String userid = Request["userid"];

    try {
      if (op == "checkconnection") {
        String sdevid = Request["deviceid"];
        int deviceId = 0;
        try { deviceId = Convert.ToInt32(sdevid); }
        catch {deviceId = 0;}
        String version = Request["version"];
        if (userid == "0")
          userid = CreateUser(version, deviceId).ToString();
          
        DataSet ds = GetDS("select * from users where id='" + userid + "'");
        if (GetRowCount(ds) == 0) {
          userid = CreateUser(version, deviceId).ToString();
          ds = GetDS("select * from users where id='" + userid + "'");
        }
        if (GetString(ds, 0, "version") != version)
          ExecSql("update users set version='" + version + "' where id='" + userid + "'");

        String fullversion = "1";
        if (version.StartsWith("TRIAL")) {
          int trialdays = Convert.ToInt32(Request["trialdays"]);
          TimeSpan diff = DateTime.Now - GetDate(ds, 0, "regdate");
          fullversion = (diff.Days <= trialdays ? "1" : "0");
        }
        Response.Write(userid + "," + GetString(ds, 0, "username") + "," + fullversion);
      }
      else if (op == "gettotalhighscores") {
        DataSet ds = GetDS("select top 100 s.score, u.username from scores s, users u where s.userid=u.id order by s.score desc");
        String res = "";
        for (int i=0; i < GetRowCount(ds); i++) {
          if (res.Length > 0) res += ";";
          res += GetString(ds, i, "score") + "," + GetString(ds, i, "username");
        }
        Response.Write(res);
      }
      else if (op == "getpersonalhighscores") {
        DataSet ds = GetDS("select top 100 score, scoredate from scores where userid='" + userid + "' order by score desc");
        String res = "";
        for (int i=0; i < GetRowCount(ds); i++) {
          if (res.Length > 0) res += ";";
          res += GetString(ds, i, "score") + "," + GetDate(ds, i, "scoredate").ToString("dd MMM yyyy");
        }
        Response.Write(res);
      }
      else if (op == "getrankings") {
        DataSet ds = GetDS("select top 100 username, rank from users order by rank desc");
        String res = "";
        for (int i=0; i < GetRowCount(ds); i++) {
          if (res.Length > 0) res += ";";
          res += GetString(ds, i, "username") + "," + GetString(ds, i, "rank");
        }
        Response.Write(res);
      }
      else if (op == "getmypendinggames") {
        DataSet ds = GetDS("select top 100 g.id, u.username, u.rank, g.finishedmaster, g.scoremaster, g.bonusmaster from games g, users u where g.usermaster=u.id and g.userslave=" + userid + " and g.finishedslave is null and not finishedmaster is null order by g.finishedmaster desc");
        String res = "";
        for (int i=0; i < GetRowCount(ds); i++) {
          if (res.Length > 0) res += ";";
          TimeSpan diff = DateTime.Now - GetDate(ds, i, "finishedmaster");
          String timeStr = "";
          if (diff.Days > 0) timeStr = (diff.Days*24 + diff.Hours).ToString() + " h";
          else if (diff.Hours > 0) timeStr = diff.Hours + " h";
          else if (diff.Minutes > 0) timeStr = diff.Minutes + " m";
          else timeStr = diff.Seconds + " s";
          String scoreStr = (GetInt(ds, i, "scoremaster") + GetInt(ds, i, "bonusmaster")).ToString();
          res += GetString(ds, i, "id") + "," + GetString(ds, i, "username") + "," + GetInt(ds, i, "rank") + "," + scoreStr + "," + timeStr;
        }
        Response.Write(res);
      }
      else if (op == "getmychallengedgames") {
        String sql = @"
          select top 100 
            g.id, u.username, u.rank, g.finishedmaster, g.scoremaster, g.bonusmaster 
          from 
            games g
          left outer join Users u on
            u.id = g.userslave
          where 
            g.usermaster={0} and 
            g.finishedslave is null and not finishedmaster is null 
          order by 
            g.finishedmaster desc";
        DataSet ds = GetDS(String.Format(sql, userid));
        String res = "";
        for (int i=0; i < GetRowCount(ds); i++) {
          if (res.Length > 0) res += ";";
          TimeSpan diff = DateTime.Now - GetDate(ds, i, "finishedmaster");
          String timeStr = "";
          if (diff.Days > 0) timeStr = (diff.Days*24 + diff.Hours).ToString() + " h";
          else if (diff.Hours > 0) timeStr = diff.Hours + " h";
          else if (diff.Minutes > 0) timeStr = diff.Minutes + " m";
          else timeStr = diff.Seconds + " s";
          String scoreStr = (GetInt(ds, i, "scoremaster") + GetInt(ds, i, "bonusmaster")).ToString();
          res += GetString(ds, i, "id") + "," + GetString(ds, i, "username") + "," + GetInt(ds, i, "rank") + "," + scoreStr + "," + timeStr;
        }
        Response.Write(res);
      }
      else if (op == "getfinishedgames") {
        DataSet ds = GetDS("select top 100 g.id, g.usermaster, u1.username as mastername, u1.rank as masterrank, u2.username as slavename, u2.rank as slaverank, g.finishedslave, ((select max(c) from (select finishedmaster as c union all select finishedslave) as cs)) as lastfinish, g.scoremaster, g.bonusmaster, g.scoreslave, g.bonusslave from games g, users u1, users u2 where (g.usermaster=" + userid + " or g.userslave=" + userid + ") and g.usermaster=u1.id and g.userslave=u2.id and not g.finishedslave is null and not finishedmaster is null order by lastfinish desc");
        String res = "";
        for (int i=0; i < GetRowCount(ds); i++) {
          if (res.Length > 0) res += ";";
          String timeStr = GetDate(ds, i, "finishedslave").ToString("d MMM HH:mm");
          
          bool immaster = (GetString(ds, i, "usermaster") == userid);
          String oppname = (immaster ? GetString(ds, i, "slavename") : GetString(ds, i, "mastername"));
          String opprank = (immaster ? GetString(ds, i, "slaverank") : GetString(ds, i, "masterrank"));
          int masterscore = GetInt(ds, i, "scoremaster") + GetInt(ds, i, "bonusmaster");
          int slavescore = GetInt(ds, i, "scoreslave") + GetInt(ds, i, "bonusslave");
          String winner = "";
          if (immaster)
            winner = (masterscore > slavescore ? "YOU WON" : (slavescore > masterscore ? "YOU LOST" : "DRAW"));
          else
            winner = (masterscore > slavescore ? "YOU LOST" : (slavescore > masterscore ? "YOU WON" : "DRAW"));

          res += GetString(ds, i, "id") + "," + oppname + "," + opprank + "," + winner + "," + timeStr;
        }
        Response.Write(res);
      }
      else if (op == "startrandomgame") {
        String myRank = Request["myrank"];
        ExecSql("delete from games where usermaster=" + userid + " and userslave is null");
        DataSet ds = GetDS("select top 1 g.id, u.id as oppid, u.username as oppname, g.orifreq, g.startfreq from games g, users u where g.userslave is null and g.usermaster <> " + userid + " and u.id=g.usermaster order by ABS(u.rank - " + myRank + ") asc");
        if (GetRowCount(ds) > 0) {
          int gameid = GetInt(ds, 0, "id");
          ExecSql("update games set userslave=" + userid + " where id=" + gameid);
          Response.Write("slave," + gameid + "," + GetInt(ds, 0, "oppid") + "," + GetString(ds, 0, "oppname") + "," + GetInt(ds, 0, "orifreq") + "," + GetInt(ds, 0, "startfreq"));
        }
        else {
          ExecSql("insert into games (usermaster, orifreq, startfreq, gametype) values(" + userid + ", " + Request["orifreq"] + ", " + Request["startfreq"] + ", 'Random')");
          int gameid = GetInt("select max(id) as maxid from games where usermaster=" + userid, "maxid");
          Response.Write("master," + gameid + ",0,Random player," + Request["orifreq"] + "," + Request["startfreq"]);
        }
      }
      else if (op == "startgameagainst") {
        ExecSql("delete from games where usermaster=" + userid + " and finishedmaster is null");
        String response = "";
        int oppid = 0;
        String oppusername = Request["oppusername"];

        DataSet ds = GetDS("select * from users where username='" + oppusername + "' and id <> '" + userid + "'");
        if (GetRowCount(ds) == 0) {
          response = "The user " + oppusername + " was not found.";
        }
        else {
          oppid = GetInt(ds, 0, "id");
          ds = GetDS("select * from games where usermaster=" + userid + " and userslave=" + oppid + " and (finishedmaster is null or finishedslave is null)");
          if (GetRowCount(ds) >= MAX_CHALLENGES) {
            response = "You already have the maximum of " + MAX_CHALLENGES + " pending challenges against " + oppusername;
          }
          else {
            ds = GetDS("select * from games where usermaster=" + oppid + " and userslave=" + userid + " and (finishedmaster is null or finishedslave is null)");
            if (GetRowCount(ds) > 0) {
              response = ",slave," + GetString(ds, 0, "id") + "," + oppid + "," + oppusername + "," + GetString(ds, 0, "orifreq") + "," + GetString(ds, 0, "startfreq");
            }
            else {
              ExecSql("insert into games (usermaster, userslave, orifreq, startfreq, gametype) values(" + userid + ", " + oppid + ", " + Request["orifreq"] + ", " + Request["startfreq"] + ", '" + Request["gametype"] + "')");
              int gameid = GetInt("select max(id) as maxid from games where usermaster=" + userid + " and finishedmaster is null", "maxid");
              response = ",master," + gameid + "," + oppid + "," + oppusername + "," + Request["orifreq"] + "," + Request["startfreq"];
            }
          }
        }
        Response.Write(response);
      }
      else if (op == "saveround") {
        String ut = Request["usertype"];
        String guessedcol = "guessed" + ut;
        String elapsedcol = "elapsed" + ut;
        String scorecol = "score" + ut;
        String bonuscol = "bonus" + ut;
        String finishedcol = "finished" + ut;
        
        int gf = Convert.ToInt32(Request["guessedfreq"]);
        int elapsed = Convert.ToInt32(Request["elapsed"]);
        int score = Convert.ToInt32(Request["score"]);
        int bonus = Convert.ToInt32(Request["bonus"]);
        int gameid = Convert.ToInt32(Request["gameid"]);
        
        ExecSql("update games set " + guessedcol + "=" + gf + ", " + elapsedcol + "=" + elapsed + ", " + scorecol + "=" + score + ", " + bonuscol + "=" + bonus + ", " + finishedcol + "=GETDATE() where id=" + gameid);
      }
      else if (op == "getroundresults") {
        String response = "";
        String gameid = Request["gameid"];
        String sql = @"
          select 
            g.*, u1.username as mastername, u1.rank as masterrank, u2.username as slavename, u2.rank as slaverank 
          from 
            games g
          inner join users u1 on
            u1.id=g.usermaster
          left outer join users u2 on
            u2.id=g.userslave
          where 
            g.id={0}";
        DataSet ds = GetDS(String.Format(sql, gameid));
        if (GetRowCount(ds) > 0) {
          String usertype = (GetString(ds, 0, "usermaster") == userid ? "master" : "slave");
          String oppusertype = (usertype == "master" ? "slave" : "master");
          
          int oppid = GetInt(ds, 0, "user" + oppusertype);
          String myname = GetString(ds, 0, usertype + "name");
          String oppname = GetString(ds, 0, oppusertype + "name");
          int orifreq = GetInt(ds, 0, "orifreq");
          int startfreq = GetInt(ds, 0, "startfreq");
          int myguessed = GetInt(ds, 0, "guessed" + usertype);
          int oppguessed = GetInt(ds, 0, "guessed" + oppusertype);
          int myelapsed = GetInt(ds, 0, "elapsed" + usertype);
          int oppelapsed = GetInt(ds, 0, "elapsed" + oppusertype);
          int myscore = GetInt(ds, 0, "score" + usertype);
          int oppscore = GetInt(ds, 0, "score" + oppusertype);
          int mybonus = GetInt(ds, 0, "bonus" + usertype);
          int oppbonus = GetInt(ds, 0, "bonus" + oppusertype);
          int myrank = GetInt(ds, 0, usertype + "rank");
          int opprank = GetInt(ds, 0, oppusertype + "rank");
          
          if (!GetBoolean(ds, 0, "ranksupdated") && myguessed > 0 && oppguessed > 0) {
            ExecSql("update games set ranksupdated=1 where id=" + gameid);
            int[] ranks = getRanks(myscore + mybonus, oppscore + oppbonus, myrank, opprank);
            myrank = ranks[0];
            opprank = ranks[1];
            ExecSql("update users set rank=" + myrank + " where id=" + userid);
            ExecSql("update users set rank=" + opprank + " where id=" + oppid);
          }
          response = userid + "," + oppid + "," + myname + "," + oppname + "," + orifreq + "," + startfreq + "," + myguessed + "," + oppguessed + "," + myelapsed + "," + oppelapsed + "," + myscore + "," + oppscore + "," + mybonus + "," + oppbonus + "," + myrank + "," + opprank;
        }
        Response.Write(response);
      }
      else if (op == "getmyrank") {
        String rank = GetString("select rank from users where id=" + userid, "rank");
        Response.Write(rank);
      }
      else if (op == "getgameinfo") {
        String ut = Request["usertype"];
        String opput = (ut == "master" ? "slave" : "master");
        DataSet ds = GetDS("select g.orifreq, g.startfreq, g.user" + opput + " as oppid, u.username as oppname from games g, users u where g.id=" + Request["gameid"] + " and user" + opput + "=u.id");
        Response.Write(GetString(ds, 0, "oppid") + "," + GetString(ds, 0, "oppname") + "," +  GetString(ds, 0, "orifreq") + "," + GetString(ds, 0, "startfreq"));
      }
      else if (op == "deletegame") {
        ExecSql("delete from games where id=" + Request["gameid"]);
      }
      else if (op == "setusername") {
        String mess = checkUsername(userid, Request["newusername"], Request["deviceid"]);
        if (mess.Length > 0)
          Response.Write(mess);
        else 
          ExecSql("update users set username='" + Request["newusername"] + "' where id=" + userid);
      }
      else if (op == "savescore") {
        ExecSql("insert into scores (scoredate, score, userid) values(GETDATE(), " + Request["score"] + ", '" + userid + "')");
        int pers = GetInt("select count(*) as nof from scores where score > " + Request["score"] + " and userid='" + userid + "'", "nof") + 1;
        int tot = GetInt("select count(*) as nof from scores s, users u where u.id=s.userid and s.score > " + Request["score"], "nof") + 1;
        Response.Write(pers.ToString() + "," + tot.ToString());
      }
      else if (op == "logerror") {
        ExecSql("insert into errorlog (errdate, userd, operation, error) values(GETDATE(), '" + userid + "', '" + Request["oper"] + "', '" + Request["error"] + "')");
      }

    }
    catch (Exception ex) {
      try { ExecSql("insers into errorlog (errdate, userid, operation, error) values(GETDATE(), '" + op + "', '" + userid + "', '" + ex.Message + "')"); }
      catch {}
      Response.Write("Error: " + ex.Message);
    }
  }
  
  private String checkUsername(String userid, String newun, String deviceId) {
/*    DataSet ds = GetDS("select * from users where username='" + newun + "' and deviceid <> 0 and deviceid=" + deviceId);
    if (GetRowCount(ds) > 0) {
      int olduserid = GetInt(ds, 0, "id");
      ExecSql("update scores set userid=" + userid + " where userid=" + olduserid);
      ExecSql("update games set usermaster=" + userid + " where usermaster=" + olduserid);
      ExecSql("update games set userslave=" + userid + " where userslave=" + olduserid);
      ExecSql("update errorlog set userid=" + userid + " where userid=" + olduserid);
      ExecSql("delete from users where id=" + userid);
      return "NewId" + userid;
    }
    else {*/
      if (newun.Length == 0) return "The username cannot be empty.";
      else if (newun.Length > 20) return "Maximum username length is 20";
      else if (newun.Contains(",")) return "The username cannot contain ','.";
      else if (newun.Contains(";")) return "The username cannot contain ';'.";
      else if (RowExists("select id from users where username='" + newun + "' and id <> " + userid)) return "The username is already taken. Please select another one.";
      else return "";
//    }
  }
  
  private int CreateUser(String version, int deviceId) {
    int count = GetInt("select count(*) as nof from users", "nof") + 100;
    while (RowExists("select id from users where username='Player " + count + "'"))
      count++;
    String username = "Player " + count;
    String ip = Request.UserHostAddress;
    ExecSql("insert into users (username, regdate, version, deviceid, ip, rank) values('" + username + "', GETDATE(), '" + version + "', " + deviceId + ", '" + ip + "', " + INITIAL_RANK + ")");
    return GetInt("select id from users where username='" + username + "'", "id");
  }
  
  private int[] getRanks(int score1, int score2, int rank1, int rank2) {
    if (score1 > score2) {
      rank1 += Convert.ToInt32(Math.Round(0.1*rank2));
      rank2 -= Convert.ToInt32(Math.Round(0.1*rank2));
    }
    else if (score2 > score1) {
      rank2 += Convert.ToInt32(Math.Round(0.1*rank1));
      rank1 -= Convert.ToInt32(Math.Round(0.1*rank1));
    }
    return new int[2]{rank1, rank2};
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
      throw new Exception(ex.Message);
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

  public static int GetRowCount(DataSet ds) {
    return (ds.Tables.Count > 0 ? ds.Tables[0].Rows.Count : 0);
  }

  public static bool RowExists(String sqlStr) {
    DataSet ds = GetDS(sqlStr);
    return (GetRowCount(ds) > 0);
  }

  public static void Debug(String txt) {
    HttpServerUtility server = HttpContext.Current.Server;
    StreamWriter sw = File.AppendText(server.MapPath("/debug.txt"));
    sw.WriteLine(txt);
    sw.Close();    
  }

}
