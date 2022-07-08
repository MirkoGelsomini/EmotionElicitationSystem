var playlist = [];
var statistics = [];
var currentScene;
var passedScene;
var messageCodeExpected = 0;

const radios = document.getElementById
const confirmBtn = document.getElementById('confirm-button');
const saveBtn = document.getElementById('save-button');
const questionModal = new bootstrap.Modal(document.getElementById("Questions"), {});
const thanksModal = new bootstrap.Modal(document.getElementById("Thanks"), {});

const emotions = ["Amusement","Anger","Sadness","Tenderness","Fear","Disgust"];
const videoPlaystDiv = document.getElementById("playlist");

//----WebSocketPart----
const ws = new WebSocket("ws://localhost:7075");
ws.addEventListener("open", ()=>{
    console.log("connected");
    messageCodeExpected = 2;
    ws.send("2)")
})

ws.addEventListener("message",(data)=>{
    switch(messageCodeExpected){
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


function startAnalyzing(){
    nextTrack();
}
function endAnalyzing(){
    window.location.href="OperatorPage.html";
}


function askQuestion(){  
    passedScene = currentScene;
    emotions.forEach((e)=>$('#'+e+' input:radio').prop('checked',false));
    questionModal.show();  
}


function nextTrack(){
    currentScene = playlist.shift();
    if(currentScene.URL){
        myVideo.src = currentScene.URL;
        let text = []
        playlist.forEach((e)=>{text.push(e.id+" "+e["movie title"]+" "+e.emotions+"<br>")});
        videoPlaystDiv.innerHTML = text.join("");
        if(playlist.length == 0){
            videoPlaystDiv.innerHTML = "Nulla in coda";
        }
    }else{
        console.log("Playlist terminata");
    }
}

function thanks(){
    thanksModal.show();
}

function save(){
    messageCodeExpected = 4;
    ws.send("4)"+JSON.stringify(statistics));
}

function validateAnswer(emotion){
    let answer = $('#'+emotion+' input:radio:checked').val();
        if(answer != undefined){
            document.getElementById(emotion+"-validation").className = "validation-hidden";
            document.getElementById(emotion+"-validation").innerHTML = "";
            return answer;
        }else{
            document.getElementById(emotion+"-validation").className = "validation-shown";
            document.getElementById(emotion+"-validation").innerHTML = "Seleziona la tua risposta";
        }
}
saveBtn.onclick = function(){
    save();
    thanksModal.hide();
}

confirmBtn.onclick = function(){
    let questionsAnswer = [];
    
    emotions.forEach((e)=>{
        let answer = validateAnswer(e);
        if(answer != undefined){
            questionsAnswer.push(answer);
        }
    })
    if(questionsAnswer.length == emotions.length){
        var dict = {};
        dict["Scene"] = passedScene;
        emotions.forEach((e) =>{
            dict[e] = questionsAnswer.shift();
        })
        statistics.push(dict);
        console.log(statistics);
        questionModal.hide();
        if(playlist.length == 0 && currentScene == undefined){
            thanks();
        }
    }
}



