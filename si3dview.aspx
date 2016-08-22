<%@ Page language="C#" CodeFile="si3dview.cs" Inherits="DefaultPage" %>
<!DOCTYPE html>

<html>
  <head>
    <meta charset="utf-8">
    <title>si3d</title>
    <link rel="stylesheet" media="screen, print" type="text/css" href="css/pw-main.css" />
    <link rel="stylesheet" type="text/css" href='css/ui-lightness/jquery-ui-1.10.3.custom.min.css'>
    <link rel="stylesheet" type="text/css" href='js/jqgrid-4.5.4/css/ui.jqgrid.css'>
    <script type="text/javascript" src="js/jquery-1.10.2.min.js"></script>
    <script type='text/javascript' src="js/jqgrid-4.5.4/js/i18n/grid.locale-sv.js"></script>
    <script type='text/javascript' src="js/jqgrid-4.5.4/js/jquery.jqGrid.min.js"></script>

    <script type="text/javascript"><asp:Literal id='ImageJs' runat="server" /></script>
    <script type="text/javascript" src="si3dview.js"></script>
  </head>
  <body>
    <table id="grid"></table>
    <div id="gridctrl"></div>
    <div id='imgdiv'><img /></div>
    <div id='info'><select id='selimg' onchange='showImg()'></select></div>
  </body>
</html>
