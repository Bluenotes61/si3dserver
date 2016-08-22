<%@ Page language="C#" CodeFile="behind/default.cs" Inherits="DefaultPage" %>

<!DOCTYPE html>

<html>
  <head>
    <title>Bertils klurigheter</title>
    <meta http-equiv='content-type' content='text/html; charset=utf-8;' />
    <link rel="stylesheet" media="screen, print" type="text/css" href="../commonres/default.css" />
    <link rel="stylesheet" media="screen, print" type="text/css" href="res/default.css" />
    <script type='text/javascript' src='../commonres/jquery-1.6.1.min.js'></script>
    <script type='text/javascript' src='../commonres/jquery.cookie.js'></script>
    <script type='text/javascript' src='../commonres/default.js?'></script>
    <script type='text/javascript' src='res/default.js'></script>
    <script type='text/javascript'><asp:Literal id="ServerJs" runat="server" /></script>
    <script src="http://connect.facebook.net/en_US/all.js"></script>
    <script type="text/javascript">
      var _gaq = _gaq || [];
      _gaq.push(['_setAccount', 'UA-475568-38']);
      _gaq.push(['_trackPageview']);

      (function() {
        var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
        ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
        var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
      })();
    </script>
  </head>
  <body>
    <div id='correct'><h1>Rätt svar!</h1><h2>Gör dig redo för nästa uppgift.</h2><a href='#' class='anch'>.</a></div>
    <div id='top'>
      <div class='left'>
        <h1 class='headline'></h1>
        <p class='intro'></p>
        <p class='info'></p>
        <p class='descript'></p>
        <p class='tips'></p>
      </div>
      <div class='right'>
        <img id='logo1' src='img/logga.png' alt='Ordvändningen' width="200" height="53" />
      </div>
    </div>
    <hr />
    <div id='qarea'></div>
    <div id='final'><h1>Grattis!</h1><h2>Alla uppgifterna besvarade.</h2><a href='#' class='anch'>.</a></div>
    <div id='reset'>
      <hr />
      <p>Alla uppgifterna är rätt besvarade. Vill du <a href='javascript:void(0)' onclick='resetQuestions()'>gömma svaren</a> och prova igen?</p>
    </div>
    <div id="fb-root"></div>
    <script>
      FB.init({
        appId : '521774307849188'
      });
    </script>

  </body>
</html>
