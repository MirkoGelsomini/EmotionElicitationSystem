var playlist = [];
var statistics = [];
var currentScene;
var passedScene;
var messageCodeExpected = 0;

const saveBtn = document.getElementById('save-button');
const questionModal = new bootstrap.Modal(document.getElementById("Questions"), {});
const thanksModal = new bootstrap.Modal(document.getElementById("Thanks"), {});
const questionRadios= document.getElementById("questions-radio");

const emotions = ["Amusement", "Anger", "Sadness", "Tenderness", "Fear", "Disgust", "Neutrality"];
const videoPlaystDiv = document.getElementById("playlist");
const sceneTitle = document.getElementById("scene-title");

//-- Start WebSocketPart --
const ws = new WebSocket("ws://localhost:7075");
ws.addEventListener("open", () => {
    console.log("connected");
    messageCodeExpected = 2;
    ws.send("2)")
})

/**
 * read WebSocket.js for code documentation
 */
ws.addEventListener("message", (data) => {
    switch (messageCodeExpected) {
        case 2:
            var playlistObj = JSON.parse(data.data);
            Array.from(playlistObj).forEach(element => {
                playlist.push(element);
            });
            startAnalyzing();
            break;
        case 4:
            endAnalyzing();
            break;
        default:
            console.log("Not valid message code");
    }
})

/**
 * Send a message to the websocket 
 * to save the passend result json if 
 * at least 1 scene has been aswered
 */
function save() {
    if(statistics.length > 0){
        messageCodeExpected = 4;
        ws.send("4)" + JSON.stringify(statistics));
    }else{
        endAnalyzing();
    }
}
//-- End WebSocketPart --

/**
 * Start the playlist
 */
function startAnalyzing() {
    nextTrack();
}

/**
 * End the playlist reproduction
 * and open the operator page
 */
function endAnalyzing() {
    window.location.href = "OperatorPage.html";
}

/**
 * Show the questions modal
 */
function askQuestion() {
    createRandomizedQuestion();
    passedScene = currentScene;
    emotions.forEach((e) => $('#' + e + ' input:radio').prop('checked', false));
    questionModal.show();
}

/**
 * Poll the next/first video's url track
 * and shows it.
 */
function nextTrack() {
    currentScene = playlist.shift();
    if (typeof currentScene !== "undefined" && currentScene.URL) {
        myVideo.src = currentScene.URL;
        let text = []
        sceneTitle.innerHTML = "<span style=\"font-weight:900;\">" + currentScene["movie title"] + "</span><span style=\"font-size:16px;\"> (" + currentScene["id"].split('-')[1] + ")</span>"
        playlist.forEach((e) => { text.push("<span style=\"font-weight:900;\">" + e["movie title"] + "</span><span style=\"font-size:16px;\"> (" + e["id"].split('-')[1] + ")</span><br/>") });
        videoPlaystDiv.innerHTML = text.join("");
        if (playlist.length == 0) {
            videoPlaystDiv.innerHTML = "Nulla in coda";
        }
    } else {
        console.log("playlist terminata")
    }
}

/**
 * Show the thanks and save modal
 */
function thanks() {
    thanksModal.show();
}

/**
 * Check if the question of given emotion 
 * is answered, if not the question color change
 * to red.
 * @param {emotion to check} emotion 
 * @returns 
 */
function validateAnswer(emotion) {
    let answer = $('#' +  emotion + ' input:radio:checked').val();
    console.log(answer);
    if (answer != undefined) {
        document.getElementById(emotion + "-validation").className = "validation-hidden";
        return answer;
    } else {
        document.getElementById(emotion + "-validation").className = "validation-shown";
    }
}

//--Start listener--
saveBtn.onclick = function () {
    save();
    thanksModal.hide();
}

function confirm(){
    let questionsAnswer = [];

    emotions.forEach((e) => {
        console.log(e);
        let answer = validateAnswer(e);
        if (answer != undefined) {
            questionsAnswer.push(answer);
        }
    })
    if (questionsAnswer.length == emotions.length) {
        var dict = {};
        dict["Scene"] = passedScene;
        emotions.forEach((e) => {
            dict[e] = questionsAnswer.shift();
        })
        statistics.push(dict);
        console.log(statistics);
        questionModal.hide();
        if (playlist.length == 0 && currentScene == undefined) {
            thanks();
        }
    }
}

//--Ends listener--

//--Start Randomize Question--
/**
 * shuffle a given array
 */
function shuffle(arrayToShuffle) {
    let array = arrayToShuffle;
    for (let i = array.length - 1; i > 0; i--) {
      let j = Math.floor(Math.random() * (i + 1));
      [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
}
/**
 * randomize radio questions
 */
function createRandomizedQuestion() {
    var questions = [];
    var randomizedEmotions = shuffle(emotions);
    randomizedEmotions.forEach((e)=>{
        let questionDiv = [];
        questionDiv.push("<div class=\"row space\">");
        questionDiv.push("<div class=\"col-xl-4 col-md-4 col-4 d-flex justify-content-center text-center validation-hidden font-medium-1\">");
        let translation = "";
        switch(e){
            case "Amusement":
                translation = "Quanta felicità ";
                break;
            case "Anger":
                translation = "Quanta rabbia ";
                break;
            case "Sadness":
                translation = "Quanta tristezza ";
                break;
            case "Tenderness":
                translation = "Quanta tenerezza ";
                break;
            case "Fear":
                translation = "Quanta paura ";
                break;
            case "Disgust":
                translation = "Quanto disgusto ";
                break;
            case "Neutrality":
                translation = "Quanta neutralità ";
                break;
            default:
                console.log("switch case error");
                break;
        }
        questionDiv.push("<span id=\""+e+"-validation\">"+translation+"hai provato?</span></div>");
        questionDiv.push("<div class=\"col-xl-8 col-md-8 col-8 d-flex justify-content-center  font-medium-2\">");
        questionDiv.push("<section id=\"basic-radio\">");
        
        questionDiv.push("<div id=\""+e+"\">");
        for(let i=1;i<=5;i++){
            if(i===1){
                questionDiv.push("<div class=\"form-check form-check-inline first\">");
            }else if(i===5){
                questionDiv.push("<div class=\"form-check form-check-inline last\">");
            }else{
                questionDiv.push("<div class=\"form-check form-check-inline\">");
            }
            questionDiv.push("<input class=\"form-check-input\" type=\"radio\" name=\""+e+"+Options\" id=\""+e+i+"\" value=\""+i+"\"/>");
            questionDiv.push("<label class=\"form-check-label\" for=\""+e+i+"\">"+i+"</label></div>");
        }
        questionDiv.push("</div></section></div></div><hr/>");
        questions.push(questionDiv.join(""));
    })
    questions.push("<div class=\"row space m-25\"><button type=\"button\" class=\"btn btn-primary\"id=\"confirm-button\">Conferma</button></div>");    
    questionRadios.innerHTML = questions.join("");
    let confirmBtn = document.getElementById('confirm-button');
    confirmBtn.onclick = function () {
        confirm();
    }    
    document.querySelectorAll('.first').forEach((e)=>e.insertAdjacentHTML('beforeBegin', "Poco "));
    document.querySelectorAll('.last').forEach((e)=>e.insertAdjacentHTML('afterEnd', "Molto"));
}

//--End Randomize Question--

