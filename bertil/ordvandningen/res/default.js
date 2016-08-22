function initLocal() {
}

function getRow(idx) {
  return "<div class='oneq ready'><div class='nr'>Uppgift " + String(idx+1) + "</div><div class='q'>" + questions[idx].q1 + "<br /><i>och omvänt</i><br />" + questions[idx].q2 + "</div><div class='a'>" + questions[idx].a1.split('|')[0] + "<br />" + questions[idx].a1.split('|')[1] + "</div></div>";
}

function getQuestion() {
  return "<div class='oneq'><div class='nr'>Uppgift " + String(nextq+1) + "</div><div class='q'>" + questions[nextq].q1 + "<br /><i>och omvänt</i><br />" + questions[nextq].q2 + "</div><div class='a'><input id='q" + nextq + "' type='text' onkeyup='checkAnswer(this)' /></div></div>";
}

function getShownAnswer(idx) {
  var arr = questions[idx].a1.split('|');
  return arr[0] + "<br />" + arr[1];
}
