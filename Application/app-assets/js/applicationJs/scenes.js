var allScenes = [];
var filteredScenes = [];
var chosenScenes = [];
var cardsHtml = [];
var movieTitleSet = new Set();
var emotionSet = new Set();
var messageCodeExpected = 0; //follows the socket message code documention 


const selectByTitle = document.getElementById('select-by-title');
const selectByEmotion = document.getElementById('select-by-emotion');
const playButton = document.getElementById('play-playlist');
const clearPlaylistButton = document.getElementById('clear-playlist');
const clearFilterButton  =  document.getElementById('clear-filters');
const cardsListScenes = document.getElementById('scenes-filtered');
const playlist = document.getElementById('playlist');
const formArray = [selectByTitle, selectByEmotion];
var idsHtmlSelected = [];

//----Start WebSocketPart----

/**
 * read WebSocket.js for code documentation
 */
const ws = new WebSocket("ws://localhost:7075");
ws.addEventListener("open", () => {
    console.log("connected");
    loadScenes();
})

ws.addEventListener("message", (data) => {
    switch (messageCodeExpected) {
        case 1:
            allScenes = JSON.parse(data.data);
            loadStartingOption();
            break;
        case 3:
            changeWindowToPlayer();
            break;
        default:
            console.log("Not valid message code");
    }
})

/**
 * Send message to socket 
 * to load all scenes
 */
function loadScenes() {
    messageCodeExpected = 1;
    ws.send("1)");
}

/**
 * Send a message to socket
 * to save the chosed playlist
 * scenes 
 */
function saveChosenScenes() {
    messageCodeExpected = 3;
    ws.send("3)" + JSON.stringify(chosenScenes));
}

//----End WebSocketPart----
/**
 * Create sets of movie title and emotion
 * and then create the starting filtering options
 */
function loadStartingOption() {
    allScenes.forEach(element => {
        movieTitleSet.add(element["movie title"].trim());
        emotionSet.add(element["emotions"]);
    });
    movieTitleSet = new Set(Array.from(movieTitleSet).sort());
    emotionSet = new Set(Array.from(emotionSet).sort());
    addFilteredCards();
    createOptionsFromSet(movieTitleSet, selectByTitle);
    createOptionsFromSet(emotionSet, selectByEmotion);
}

/**
 * Create a list of options from the set and inner them in
 * the given document element
 * @param {Set of options' value} set 
 * @param {The document element to inner html} documentElement 
 * @returns options created
 */
function createOptionsFromSet(set, documentElement) {
    var options = []
    set.forEach((element) => {
        var opt = document.createElement('option');
        opt.value = element;
        opt.innerHTML = element;
        documentElement.appendChild(opt);
        options.push(opt);
    });
    $(".select option").each(function () {
        $(this).siblings('[value="' + element + '"]').remove();
    });
    return options
}

/**
 * Create a cart for each filtered Scenes,
 * if they exist, else it create the cards
 * for all scenes and innered in the html code
 */
function addFilteredCards() {
    cardsHtml = [];
    counter = 0;
    if (filteredScenes.length > 0) {
        filteredScenes.forEach((e) => {
            addCard(e, counter++);
        });
    } else {
        allScenes.forEach((e) => {
            addCard(e, counter++);
        });
    }
    cardsListScenes.innerHTML = cardsHtml.join("");
}


/**
 * Create a card with the id, title and emotion, 
 * of the given json scene and push the html code for the card
 * in the global array
 * 
 * @param {scene of the card} jsonObject 
 * @param {the number of the card} numberOfCard 
 */
function addCard(jsonObject, numberOfCard) {
    cardsHtml.push("<div class=\"col-xl-4 col-md-6 col-12 text-center\">");
    cardsHtml.push("<div class=\"card\"  id=\"card" + numberOfCard + "\">");
    cardsHtml.push("<div class=\"card-body\">");
    cardsHtml.push("<div class=\"row\">");
    cardsHtml.push("<div class=\"col-xl-8 col-md-8 col-8 text-center\">")
    cardsHtml.push("<div class=\"card-title float-start\" style=\"font-size:15px;\">" + jsonObject["id"] + "<br />" + jsonObject["movie title"] + "</div></div>");
    cardsHtml.push("<div class=\"col-xl-4 col-md-4 col-4 text-center justify-content-center\">");

    if(!chosenScenes.includes(jsonObject)){
        cardsHtml.push("<button class=\"btn btn-primary scenesBtn addBtn\" id=\"add-" + numberOfCard + "-button\" onClick=\"addScene(this.id)\"></button>");
    }else{
        cardsHtml.push("<button class=\"btn btn-danger scenesBtn removeBtn\" id=\"add-" + numberOfCard + "-button\" onClick=\"addScene(this.id)\"></button>");
    }
    cardsHtml.push("</div></div>");
    cardsHtml.push("<div class=\"img_wrapper\" style=\"position:relative; padding-bottom:56.25%;\">")
    cardsHtml.push("<img style=\"pointer-events: none;\" src=\"" + jsonObject["URLimage"] + "\" frameborder=\"0\" width=\"100%\" height=\"100%\" id=\"img" + numberOfCard + "\" loading=\"lazy\"></img>");
    cardsHtml.push("</div></div></div></div></div>");
}

/**
 * Find and delete all duplicated options from selection element
 * @param {The selection to check} selectionElement 
 */
function removeDuplicatedOption(selectionElement) {
    const optionsValue = [];
    const titlesHtmlSelected = Array.from(selectionElement.options);
    titlesHtmlSelected.forEach((e) => {
        if (!optionsValue.includes(e.value)) {
            optionsValue.push(e.value);
        } else {
            selectionElement.removeChild(e);
        }
    })
}
/**
 * Delete all duplicates from all selection
 * @param {Array of selection to check} selectionArray 
 */
function removeAllDuplicatesFromSelects(selectionArray) {
    selectionArray.forEach((e) => removeDuplicatedOption(e));
}

/**
 * Remove all not selected option from the given selection
 * @param {Selection Element to check} selectionElement 
 */
function deleteNotSelectedChildren(selectionElement) {
    $(selectionElement).find('option').not(':selected').remove();
}

/**
 * Delete all children from the given document element
 * @param {Document element delete children} documentElement 
 */
function deleteAllChildren(documentElement) {
    while (documentElement.firstChild) {
        documentElement.remove(documentElement.lastChild);
    }
}

/**
 * Change titles options based on the other filter selection
 */
function changeTitlesOption() {
    deleteNotSelectedChildren(selectByTitle);
    var titleSet = new Set();
    
    if (filteredScenes.length != 0) {
        filteredScenes.forEach((element) => {
            titleSet.add(element["movie title"]);
        });
        createOptionsFromSet(titleSet, selectByTitle);
    }else {
        createOptionsFromSet(movieTitleSet, selectByTitle);
    }
    if($(selectByEmotion).find('option:selected').length == 0 && filteredScenes.length != 0){
        createOptionsFromSet(movieTitleSet, selectByTitle);
    }
}

/**
 * Change emotions options based on the other filter selection
 */
function changeEmotionsOption() {
    deleteNotSelectedChildren(selectByEmotion);
    var emoSet = new Set();
    if($(selectByTitle).find('option:selected').length == 0 && filteredScenes.length != 0){
        changeTitlesOption();
    }
    if (filteredScenes.length != 0 || $(selectByTitle).find('option:selected').length != 0) {
        filteredScenes.forEach((element) => {
            emoSet.add(element["emotions"]);
        });
        createOptionsFromSet(emoSet, selectByEmotion);
    } else {
        createOptionsFromSet(emotionSet, selectByEmotion);
    }
    if($(selectByTitle).find('option:selected').length == 0 && filteredScenes.length != 0){
        createOptionsFromSet(emotionSet, selectByEmotion);
    }
}

/**
 * Count how many cards have been selected
 */
function countScenesAvailable() {
    const idsHtmlSelected = Array.from(document.querySelectorAll('#select-by-id option:checked'));
    const titlesHtmlSelected = Array.from(document.querySelectorAll('#select-by-title option:checked'));
    const emotionsHtmlSelected = Array.from(document.querySelectorAll('#select-by-emotion option:checked'));
    var idsSelected = [];
    var titlesSelected = [];
    var emotionsSelected = [];

    if (idsHtmlSelected.length !== 0) {
        idsHtmlSelected.forEach((e) => idsSelected.push(e.value));
    }
    if (titlesHtmlSelected.length !== 0) {
        titlesHtmlSelected.forEach((e) => titlesSelected.push(e.value));
    }
    if (emotionsHtmlSelected.length !== 0) {
        emotionsHtmlSelected.forEach((e) => emotionsSelected.push(e.value));
    }
    filteredScenes = allScenes.filter((element) => {
        var startSearch = true;

        if (titlesSelected.length !== 0 && !titlesSelected.includes(element["movie title"])) {
            startSearch = false;
        }
        if (emotionsSelected.length !== 0 && !emotionsSelected.includes(element["emotions"])) {
            startSearch = false;
        }
        if (titlesSelected.length === 0 && emotionsSelected.length === 0) {
            startSearch = false;
        }
        if (startSearch) {
            return true;
        } else {
            return false;
        }
    })
    addFilteredCards();
}

/**
 * Add the given card's id scene
 * to the playlist.
 * If the scene is already added the
 * function will remove it
 * @param {card's id to add} id 
 */
function addScene(id) {
    let index = id.split("-")[1];
    let btn = document.getElementById(id)
    let classList = btn.classList;
    if (classList.contains("addBtn")) {
        let scene;
        if (filteredScenes.length != 0) {
            scene = filteredScenes[index];
        } else {
            scene = allScenes[index];
        }
        chosenScenes.push(scene);
        btn.classList.add("removeBtn");
        btn.classList.add("btn-danger");
        btn.classList.remove("addBtn");
        btn.classList.remove("btn-primary");
    } else {
        removeScene(id);
    }
    refreshPlayList();
}

/**
 * Remove the given card's id scene
 * to the playlist
 * @param {card's id to remove} id 
 */
function removeScene(id) {
    let index = id.split("-")[1];
    let btn = document.getElementById(id);
    let classList = btn.classList;
    let selectedScene;
    if (classList.contains("removeBtn")) {
        if (filteredScenes.length != 0) {
            selectedScene = filteredScenes[index];
        } else {
            selectedScene = allScenes[index];
        }
        chosenScenes = chosenScenes.filter(scene => scene["id"] !== selectedScene["id"]);
        btn.classList.add("addBtn");
        btn.classList.add("btn-primary");
        btn.classList.remove("removeBtn");
        btn.classList.remove("btn-danger");
    }
}

/**
 * Refresh the playlist two show the latest
 * scenes added or removed
 */
function refreshPlayList(){
    if(chosenScenes.length == 0){
        playlist.innerHTML = "Playlist vuota";
    }else{
        let scenes = [];
        chosenScenes.forEach(s=>scenes.push("id: "+s["id"]+"| titolo: "+s["movie title"]));
        playlist.innerHTML = scenes.join("<br/>");
    }
}

/**
 * Change the html page to the videoplayer
 * page
 */
function changeWindowToPlayer() {
    window.location.href = "Player.html";
}

//--Start event listener--
selectByTitle.onchange = () => {
    countScenesAvailable();
    changeEmotionsOption();
    removeAllDuplicatesFromSelects(formArray)
};

selectByEmotion.onchange = () => {
    countScenesAvailable();
    changeTitlesOption();
    removeAllDuplicatesFromSelects(formArray)
};

playButton.onclick = ()=>{
    if(chosenScenes.length > 0){
        saveChosenScenes();
    }else{
        alert("Nessuna scena selezionata");
    }
}

clearPlaylistButton.onclick = ()=>{
    chosenScenes = [];
    $(".scenesBtn").each(function() {
        $(this).addClass("addBtn");
        $(this).addClass("btn-primary");
        $(this).removeClass("removeBtn");
        $(this).removeClass("btn-danger");
      });
    refreshPlayList();
}

clearFilterButton.onclick = () =>{
    $("option").remove();
    createOptionsFromSet(movieTitleSet, selectByTitle);
    createOptionsFromSet(emotionSet, selectByEmotion);
    countScenesAvailable();
}
//--End event listener--

