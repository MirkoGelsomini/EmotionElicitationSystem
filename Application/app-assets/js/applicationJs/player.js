const myVideo = document.getElementById('video_1');
const videoContainer = document.getElementById('video-container');
const coverOverlay = document.querySelector('.overlays');

const bar = document.querySelector('.bar');
const barContent = document.querySelector('.bar-content');

const playBtn = document.getElementById('play-pause');
const bigPlayButton = document.querySelector('.big-play-button');

const volumeBtn = document.getElementById('mute-unmute');
const volume = document.querySelector('.volume');
var volumeBeforeChange = volume.value;
var boolMuted = false;

const skipTrack =document.querySelector('.next-track');

const currentTimeElement = document.querySelector('.current');
const durationTimeElement = document.querySelector('.duration');

const fullScreenBtn = document.getElementById('full-screen');
var timeout;
var isFullScreen = false;


//--Start Play/Pause--

/**
 * function for change the icon
 * of play/pause based on the video
 * current state
 */
function togglePlayPause(){
    if(myVideo.paused){
        myVideo.play();
    }else{
        myVideo.pause();
    }
}

//--Start video listener--
playBtn.onclick = function(){       
    togglePlayPause();
};

myVideo.addEventListener('canplaythrough',()=>{
    bigPlayButton.classList.remove("hide");
    document.querySelector('.spinner-border').classList.add("hide");
})

myVideo.addEventListener('loadstart',()=>{
    bigPlayButton.classList.add("hide");
    document.querySelector('.spinner-border').classList.remove("hide");
})

myVideo.addEventListener('play', ()=>{
    playBtn.className = "pause";
    videoContainer.classList.add("hover");
    coverOverlay.style.zIndex = -1;
})

myVideo.addEventListener('pause', ()=>{
    playBtn.className = "play";
    videoContainer.classList.remove("hover");
    coverOverlay.style.zIndex = 2;
})

myVideo.addEventListener('click',()=>togglePlayPause());

myVideo.addEventListener('ended',()=>{
    if(isFullScreen){
        fullScreenChange();
    }
    askQuestion();
    nextTrack();
})

bigPlayButton.addEventListener('click',()=>togglePlayPause());
//--End video listener--
//--End Play/Pause--

//--Start Skip Track--
skipTrack.addEventListener('click',()=>{
    nextTrack();
    if (playlist.length == 0 && currentScene == undefined) {
        thanks();
    }
})
//--Start Skip Track--


//--Start Progress Bar--

//Progress Bar modification
myVideo.addEventListener('timeupdate',function(){
    var barConPos = myVideo.currentTime / myVideo.duration;
    barContent.style.width = barConPos * 100 + "%";
    if(VideoColorSpace.ended){
        playBtn.className = "play";
    }
})


//Change progress bar on  click
bar.addEventListener('click',(e)=>{
    const progressTime = (e.offsetX / bar.offsetWidth)*myVideo.duration;
    myVideo.currentTime = progressTime;
})
//--End Progress Bar--

//--Start Volume Control--

volume.addEventListener('mousemove',(e)=>{
    myVideo.volume = e.target.value;
    if(e.target.value == 0){
        boolMuted = true;
        volumeBeforeChange = 0.5;
        volumeBtn.className = "muted";
    }else{
        boolMuted = false;
        volumeBtn.className = "unmuted"
    }
})

/**
 * Function to mute or unmute the video
 * saving the previous volume level.
 * When the button is pressed if it was muted
 * the volume level is set to the previous state,
 * if it exists, otherwise it will set at the middle
 */
function toggleMuteUnmute(){
    if(!boolMuted){
        myVideo.volume = 0;
        volumeBeforeChange = volume.value;
        volume.value = 0;
        volumeBtn.className = "muted";
    }else{
        volumeBtn.className = "unmuted";
        myVideo.volume = volumeBeforeChange;
        volume.value = volumeBeforeChange;
    }
    boolMuted = !boolMuted
}

volumeBtn.onclick = function(){
    toggleMuteUnmute();
};
//--End Volume Control--

//--Start FullScreen Control--

/**
 * Control the change of the screen size for both
 * Firefox and Chrome, changing the class for css part
 */
function fullScreenChange() { 
    if(!isFullScreen){
        if (myVideo.mozRequestFullScreen) {
            videoContainer.mozRequestFullScreen();
            fullScreenBtn.classList = "reduce";
        } else if (myVideo.webkitRequestFullScreen) {
            videoContainer.webkitRequestFullScreen();
            fullScreenBtn.classList = "reduce";
        }else{
            console.log("error fullScreen");
        }
    }else{
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
        } else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
        fullScreenBtn.classList = "full";
    }  
      
}

/**
 * css style for non-fullScreen video (mouse hover)
 */
 function setTransform(){
    document.getElementById("controls").style.transform = " translateY(0)";
    document.getElementById("controls").style.transition= " all 0.2s ";
}


/**
 * css style for fullScreen video (mouse move)
 */
function removeTransform(){
    document.getElementById("controls").style.removeProperty("transform")
    document.getElementById("controls").style.removeProperty("transition"); 
}

videoContainer.onmousemove = function(){
    if (isFullScreen){
        clearTimeout(timeout);
        setTransform();
        timeout = setTimeout(()=>{removeTransform()},"1500");
    }
}

//-- Start FullScreen Listener--
fullScreenBtn.onclick = function(){
    fullScreenChange();
}


videoContainer.onfullscreenchange = ()=>{
    if(isFullScreen){
        videoContainer.classList.add("hover")
        myVideo.classList.remove("video-fullscreen");
    }else{
        videoContainer.classList.remove("hover")
        myVideo.classList.add("video-fullscreen");
    }
    isFullScreen = !isFullScreen
};

videoContainer.onwebkitfullscreenchange = ()=>{
    if(isFullScreen){
        videoContainer.classList.add("hover")
        myVideo.classList.remove("video-fullscreen");
    }else{
        videoContainer.classList.remove("hover")
        myVideo.classList.add("video-fullscreen");
    }
    isFullScreen = !isFullScreen
};
//-- End FullScreen Listener--
//--end FullScreen Control--

//--Start time progress--
/**
 * Convert a given number of seconds in 
 * HH:MM:mm format
 * @param {time in sec to convert} time 
 * @returns second in HH:MM:mm format
 */
function timeConverter(time){
    var hour = addZero(parseInt(time / 3600));
    var minutes = addZero(parseInt((time % 3600)/60));
    var second = addZero(parseInt((time % 3600)%60));
    return hour+":"+minutes+":"+second;
}

/**
 * Format the given string in 2 char format.
 * If the string has only 1 char, will be
 * add a '0' in front of it.
 * 
 * @param {string in time to format} timeToAdd 
 * @returns formatted string
 */
function addZero(timeToAdd){
    var str = String(timeToAdd).length;
    if(str === 1){   
        return "0"+timeToAdd;
    }else{
        return timeToAdd;
    }
}

/**
 * Update the time on the screen
 */
const currentTime= ()=>{
   currentTimeElement.innerHTML = timeConverter(myVideo.currentTime);
   durationTimeElement.innerHTML = timeConverter(myVideo.duration);
}

myVideo.addEventListener('timeupdate', currentTime);

myVideo.addEventListener('loadedmetadata', currentTime);
//--End time progress--





