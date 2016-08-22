function initLocal() {
  $(".inpq1").focus();
}

function getRow(idx) {
  return "<div class='oneq ready'>" +
    "<div class='nr'>Uppgift " + String(idx+1) + "</div>" +
    "<div class='q1'><div class='r'>Ort:</div><div class='q'>" + questions[idx].q1 + "</div><div class='a'>" + questions[idx].a1.split('|')[0] + "</div></div>" +
    "<div class='q2'><div class='r'>Växt:</div><div class='q'>" + questions[idx].q2 + "</div><div class='a'>" + questions[idx].a2.split('|')[0] + "</div></div>" +
    "</div>";
}

function getQuestion() {
  return "<div class='oneq'>" +
    "<div class='nr'>Uppgift " + String(nextq+1) + "</div>" +
    "<div class='q1'><div class='r'>Ort:</div><div class='q'>" + questions[nextq].q1 + "</div><div class='a'><input id='q1_" + nextq + "' class='inpq1' type='text' onkeyup='checkFirstAnswer(this)' /></div></div>" +
    "<div class='q2'><div class='r'>Växt:</div><div class='q'>" + questions[nextq].q2 + "</div><div class='a'><input id='q2_" + nextq + "' class='inpq2' type='text' onkeyup='checkSecondAnswer(this)' /></div></div>" +
    "</div>";
}

function checkFirstAnswer(inp) {
  var currval = $(inp).val();
  var idx = parseInt($(inp).attr("id").substring(3));
  var val = questions[idx].a1.split('|')[0];
  if (answerCorrect(currval, questions[idx].a1)) {
    $(inp).css("color","#00f");
    var a = $(inp).parent();
    $(inp).remove();
    a.html(getShownAnswer(idx));
    if (a.parent().parent().find("input").length == 0)
      correctAnswer();
    else
      a.parent().parent().find("input").focus();
  }
}


function checkSecondAnswer(inp) {
  var currval = $(inp).val();
  var idx = parseInt($(inp).attr("id").substring(3));
  var val = questions[idx].a2.split('|')[0];
  if (answerCorrect(currval, questions[idx].a2)) {
    $(inp).css("color","#00f");
    var a = $(inp).parent();
    $(inp).remove();
    a.html(getShownAnswer(idx, 1));
    if (a.parent().parent().find("input").length == 0)
      correctAnswer();
    else
      a.parent().parent().find("input").focus();
  }
}


function correctAnswer() {
  nextq++;
  if (nextq < questions.length) {
    correctdiv.show().animate({"width":"743px", "height":"180px", "left":"0"}, {duration:500, step:function(){FB.Canvas.setSize();}, complete:function(){
      correctdiv.find(".anch").focus();
      $("#qarea").append(getQuestion());
      $(".inpq1").focus();
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
