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

const emotions = ["Amusement","Anger","Sadness","Tenderness","Fear","Disgust","Neutrality"];
const videoPlaystDiv = document.getElementById("playlist");
const sceneTitle = document.getElementById("scene-title");

//-- Start WebSocketPart --
const ws = new WebSocket("ws://localhost:7075");
ws.addEventListener("open", ()=>{
    console.log("connected");
    messageCodeExpected = 2;
    ws.send("2)")
})

/**
 * read WebSocket.js for code documentation
 */
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

/**
 * Send a message to the websocket 
 * to save the passend result json
 */
function save(){
    messageCodeExpected = 4;
    ws.send("4)"+JSON.stringify(statistics));
}
//-- End WebSocketPart --

/**
 * Start the playlist
 */
function startAnalyzing(){
    nextTrack();
}

/**
 * End the playlist reproduction
 * and open the operator page
 */
function endAnalyzing(){
    window.location.href="OperatorPage.html";
}

/**
 * Show the questions modal
 */
function askQuestion(){  
    passedScene = currentScene;
    emotions.forEach((e)=>$('#'+e+' input:radio').prop('checked',false));
    questionModal.show();  
}

/**
 * Poll the next/first video's url track
 * and shows it.
 */
function nextTrack(){
    currentScene = playlist.shift();
    if(currentScene.URL){
        myVideo.src = currentScene.URL;
        let text = []
        sceneTitle.innerHTML = currentScene.id+" "+currentScene["movie title"]+" | "+currentScene.emotions;
        playlist.forEach((e)=>{text.push(e.id+" "+e["movie title"]+" | "+e.emotions+"<br>")});
        videoPlaystDiv.innerHTML = text.join("");
        if(playlist.length == 0){
            videoPlaystDiv.innerHTML = "Nulla in coda";
        }
    }else{
        console.log("Playlist terminata");
    }
}

/**
 * Show the thanks and save modal
 */
function thanks(){
    thanksModal.show();
}

/**
 * Check if the question of given emotion 
 * is answered, if not the question color change
 * to red.
 * @param {emotion to check} emotion 
 * @returns 
 */
function validateAnswer(emotion){
    let answer = $('#'+emotion+' input:radio:checked').val();
        if(answer != undefined){
            document.getElementById(emotion+"-validation").className = "validation-hidden";
            return answer;
        }else{
            document.getElementById(emotion+"-validation").className = "validation-shown";
        }
}

//--Start listener--
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
//--Ends listener--

