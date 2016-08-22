var nextq = 0;
var correctdiv;

function initPage() {
  $("#top .headline").html(tavling.rubrik);
  $("#top .intro").html(tavling.intro);
  $("#top .info").html(tavling.info);
  $("#top .descript").html(tavling.descript);
  $("#top .tips").html(tavling.tips);

  nextq = $.cookie(tavling.shortname);
  nextq = (nextq == null ? 0 : parseInt(nextq));

  correctdiv = $("#correct");
  for (var i=0; i < nextq; i++)
    $("#qarea").append(getRow(i));

  if (nextq < questions.length) {
    $("#qarea").append(getQuestion());
    $("div.a input").focus();
    correctdiv.remove();
    $("#qarea").append(correctdiv);
  }
  else {
    $("#reset").show();
  }
  FB.Canvas.setSize();

  initLocal();
}

function answerCorrect(guess, answer) {
  var a = answer.split('|');
  var correct = false;
  for (var i=0; i < a.length && !correct; i++)
    correct = (guess.toLowerCase().replace(/\s/g, '') == a[i].toLowerCase().replace(/\s/g, ''));
  return correct;
}

function getShownAnswer(idx, second) {
  return (second ? questions[idx].a2.split('|')[0] : questions[idx].a1.split('|')[0]);
}


function checkAnswer(inp) {
  var currval = $(inp).val();
  var idx = parseInt($(inp).attr("id").substring(1));
  var val = questions[idx].a1.split('|')[0];
  if (answerCorrect(currval, questions[idx].a1)) {
    $(inp).css("color","#00f");
    var a = $(inp).parent();
    $(inp).remove();
    a.html(getShownAnswer(idx));
    nextq++;
    if (nextq < questions.length) {
      correctdiv.show().animate({"width":"743px", "height":"180px", "left":"0"}, {duration:500, step:function(){FB.Canvas.setSize();}, complete:function(){
        correctdiv.find(".anch").focus();
        $("#qarea").append(getQuestion());
        $("div.a input").focus();
        $.cookie(tavling.shortname, String(nextq), { expires: 7 });

        setTimeout(function(){
          correctdiv.animate({"width":"0", "height":"0", "left":"350px"}, 500, function(){
            correctdiv.hide().remove();
            $("#qarea").append(correctdiv);
          });
        }, 4000);
      }});
    }
    else {
      $("#final").show().animate({"width":"743px", "height":"100px", "left":"0"}, {duration:500, step:function(){FB.Canvas.setSize();}, complete:function(){
        $("#final .anch").focus();
        $.cookie(tavling.shortname, String(nextq), { expires: 7 });

        setTimeout(function(){
          $("#final").animate({"width":"0", "height":"0", "left":"350px"}, 500, function(){
            $("#final").hide().remove();
            $("#reset").show();
          });
        }, 4000);
      }});
    }
  }
}

function resetQuestions() {
  if (confirm("Är du säker på att du vill ta bort de rätta svaren?")) {
    nextq = 0;
    $.cookie(tavling.shortname, "0", { expires: 7 });
    $("#qarea").empty();
    $("#qarea").append(getQuestion());
    correctdiv.remove();
    $("#qarea").append(correctdiv);
    $("#reset").hide();
  }
}


$(document).ready(function(){
   initPage();
});
