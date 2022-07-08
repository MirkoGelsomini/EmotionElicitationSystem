var allScenes = [];
var chosenScenes = [];
var movieTitleSet = new Set();
var emotionSet = new Set();
var messageCodeExpected = 0; //follows the socket message code documention 

const selectById = document.getElementById('select-by-id');
const selectByTitle = document.getElementById('select-by-title');
const selectByEmotion = document.getElementById('select-by-emotion');
const confirmButton = document.getElementById('confirmSelect');
const formArray = [selectById,selectByTitle,selectByEmotion];

//----WebSocketPart----
const ws = new WebSocket("ws://localhost:7075");
ws.addEventListener("open", ()=>{
    console.log("connected");
    loadScenes();
})

ws.addEventListener("message",(data)=>{
    switch(messageCodeExpected){
        case 1:
            allScenes = JSON.parse(data.data)["Scenes"];
            loadStartingOption();
            break;
        case 3:
            changeWindowToPlayer();
            break;
        default:
            console.log("Not valid message code");
    }   
        
    
})


function loadScenes(){
    messageCodeExpected = 1;
    ws.send("1)");
} 

function loadStartingOption(){
    allScenes.forEach(element => {
        movieTitleSet.add(element["movie title"]);
        emotionSet.add(element["emotions"]);
    });
    createOptionsFromJson(allScenes,"id",selectById);
    createOptionsFromSet(movieTitleSet,selectByTitle);
    createOptionsFromSet(emotionSet,selectByEmotion);
}
function createOptionsFromJson(array,valueJson,documentElement){
    var options = []
    array.forEach(element =>{
        var opt = document.createElement('option');
        opt.value = element[valueJson];
        opt.innerHTML = element[valueJson];
        documentElement.appendChild(opt)
        options.push(opt);
    });
    return options
}
function createOptionsFromSet(set,documentElement){
    var options = []
    set.forEach((element)=>{  
        var opt = document.createElement('option');
        opt.value = element;
        opt.innerHTML = element;
        documentElement.appendChild(opt);
        options.push(opt);
    });
    $(".select option").each(function() {
        $(this).siblings('[value="'+ element +'"]').remove();
      });
    return options
}

function removeDuplicatedOption(documentElement){
    const optionsValue = [];
    const titlesHtmlSelected = Array.from(documentElement.options);
    titlesHtmlSelected.forEach((e) =>{
        if(!optionsValue.includes(e.value)){
            optionsValue.push(e.value);
        }else{
            documentElement.removeChild(e);
        }
    })
}

function removeAllDuplicatesFromSelects(documentArray){
    documentArray.forEach((e)=>removeDuplicatedOption(e));
}

function deleteNotSelectedChildren(documentElement){
    $(documentElement).find('option').not(':selected').remove();
}
function deleteAllChildren(documentElement){
    while(documentElement.firstChild){
        documentElement.remove(documentElement.lastChild);
    }
}

function changeIdsOption(){
    deleteNotSelectedChildren(selectById);
    createOptionsFromJson(chosenScenes,"id",selectById);
}

function changeTitlesOption(){
    deleteNotSelectedChildren(selectByTitle);
    var titleSet = new Set();
    chosenScenes.forEach((element) =>{
        titleSet.add(element["movie title"]);
    })
    createOptionsFromSet(titleSet,selectByTitle);
}

function changeEmotionsOption(){
    deleteNotSelectedChildren(selectByEmotion);
    var emoSet = new Set();
    chosenScenes.forEach((element) =>{
        emoSet.add(element["emotions"]);
    })
    createOptionsFromSet(emoSet,selectByEmotion);
}

function countScenesAvailable(){
    const idsHtmlSelected = Array.from(document.querySelectorAll('#select-by-id option:checked'));
    const titlesHtmlSelected = Array.from(document.querySelectorAll('#select-by-title option:checked'));
    const emotionsHtmlSelected = Array.from(document.querySelectorAll('#select-by-emotion option:checked'));
    var idsSelected = [];
    var titlesSelected = []; 
    var emotionsSelected = []; 
    
    if(idsHtmlSelected.length !== 0){
        idsHtmlSelected.forEach((e)=> idsSelected.push(Number(e.value)));
    }
    if(titlesHtmlSelected.length !== 0){
        titlesHtmlSelected.forEach((e )=> titlesSelected.push(e.value));
    }
    if(emotionsHtmlSelected.length !== 0){
        emotionsHtmlSelected.forEach((e )=> emotionsSelected.push(e.value));
    }
    chosenScenes = allScenes.filter((element)=>{
        var startSearch = true;

        if(idsSelected.length !== 0 && !idsSelected.includes(element["id"])){
            startSearch = false;
        }
        if(titlesSelected.length !== 0 && !titlesSelected.includes(element["movie title"])){
            startSearch = false;
        }
        if(emotionsSelected.length !== 0 && !emotionsSelected.includes(element["emotions"])){
            startSearch = false;
        }
        if(idsSelected.length === 0 && titlesSelected.length === 0 && emotionsSelected.length === 0){
            startSearch = false;
        }
        if(startSearch){
            return true;
        }else{
            return false;
        }
    }) 
    if(idsSelected.length === 0 && titlesSelected.length === 0 && emotionsSelected.length === 0){
        chosenScenes = allScenes;
        document.getElementById('counter-scenes').innerHTML = "Scene trovate: "+0;
    }else{
        document.getElementById('counter-scenes').innerHTML = "Scene trovate: "+chosenScenes.length;
    }
}

function saveChosenScenes(){
    messageCodeExpected = 3;
    ws.send("3)"+JSON.stringify(chosenScenes));
}

function changeWindowToPlayer(){
    window.location.href="index.html";
}

selectById.onchange = ()=>{countScenesAvailable();changeTitlesOption();changeEmotionsOption();removeAllDuplicatesFromSelects(formArray)};

selectByTitle.onchange = ()=>{countScenesAvailable();changeIdsOption();changeEmotionsOption();removeAllDuplicatesFromSelects(formArray)};

selectByEmotion.onchange = ()=>{countScenesAvailable();changeIdsOption();changeTitlesOption();removeAllDuplicatesFromSelects(formArray)};

confirmButton.onclick = ()=>{
    if(chosenScenes.length > 0){
        saveChosenScenes();
    }else{
        alert("Nessuna scena selezionata");
    }
}