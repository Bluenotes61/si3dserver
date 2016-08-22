function init() {
  $("#selimg").append("<option></option>");
  for (var i=0; i < images.length; i++) {
    $("#selimg").append("<option value='" + images[i].filename + "'>id=" + images[i].id + " - ip=" + images[i].ip + " - country=" + images[i].country + " - date=" + images[i].date + "</option>");
  }
  $("#imgdiv img").css("height", $(window).height()-50);

  //initGrid();
};

function showImg() {
  var fn = $("#selimg").val();
  $("#imgdiv img").attr("src", fn);
}

function initGrid() {
  $("#grid").jqGrid({
    url:'si3dview.aspx',
    postData: {
      idcol:"id",
      cols:"id,ip,name,gendate,filename,todelete,deleted",
      sql: "select s.*, c.name from si3d s left outer join countrycodes c on c.code = s.countrycode where s.deleted=0"
    },
    colNames:['Id','IP', 'Land', 'Datum', 'Filnamn', 'Raderas', 'Raderad'],
    colModel:[
      {name:'id', width:30, search:false},
      {name:'ip',index:'ip'},
      {name:'name',index:'name'},
      {name:'gendate',index:'gendate'},
      {name:'filename',index:'filename' },
      {name:'todelete',index:'todelete'},
      {name:'deleted',index:'deleted'}
    ],
    caption:"Genererade Si3D",
    datatype: "json",
    altRows:false,
    rowNum:20,
    rowList:[10,20,50,100],
    pager: '#gridctrl',
    sortname: 'gendate',
    viewrecords: true,
    sortorder: "desc",
    height:'100%',
    forceFit:true,
    shrinkToFit:false,
    width:860
  });
  $("#grid").navGrid(
    '#gridctrl',
    {edit:false,add:false,del:false,search:false,refresh:false},
    {},{},{},{}
  );
}

$(document).ready(function(){
	init();
});
