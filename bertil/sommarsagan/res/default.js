function initLocal() {
}

function getRow(idx) {
  return "<div class='oneq ready'><div class='nr'>Uppgift " + String(idx+1) + "</div><div class='q'>" + questions[idx].q1 + "</div><div class='a'>" + questions[idx].a1.split('|')[0] + "</div></div>";
}

function getQuestion() {
  return "<div class='oneq'><div class='nr'>Uppgift " + String(nextq+1) + "</div><div class='q'>" + questions[nextq].q1 + "</div><div class='a'><input id='q" + nextq + "' type='text' onkeyup='checkAnswer(this)' /></div></div>";
}

