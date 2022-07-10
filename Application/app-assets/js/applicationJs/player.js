const myVideo = document.getElementById('video_1');
const videoContainer = document.getElementById('video-container')

const bar = document.querySelector('.bar')
const barContent = document.querySelector('.bar-content');

const playBtn = document.getElementById('play-pause');


const volumeBtn = document.getElementById('mute-unmute');
const volume = document.querySelector('.volume');
var volumeBeforeChange = volume.value;
var boolMuted = false;

const currentTimeElement = document.querySelector('.current');
const durationTimeElement = document.querySelector('.duration');

const fullScreenBtn = document.getElementById('full-screen');
var timeout;
var isFullScreen = false;

//Play Pause

function togglePlayPause(){
    if(myVideo.paused){
        myVideo.play();
    }else{
        myVideo.pause();
    }
}


playBtn.onclick = function(){       
    togglePlayPause();
};

myVideo.addEventListener('play', ()=>{
    playBtn.className = "pause";
})

myVideo.addEventListener('pause', ()=>{
    playBtn.className = "play";
})

myVideo.addEventListener('click',()=>togglePlayPause());

myVideo.addEventListener('ended',()=>{
    if(isFullScreen){
        fullScreenChange();
    }
    askQuestion();
    nextTrack();
})

//Progress Bar

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

//Volume Control

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

//FullScreen

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

fullScreenBtn.onclick = function(){
    fullScreenChange();
}

//Time progress

function timeConverter(time){
    var hour = addZero(parseInt(time / 3600));
    var minutes = addZero(parseInt((time % 3600)/60));
    var second = addZero(parseInt((time % 3600)%60));
    return hour+":"+minutes+":"+second;
}

function addZero(timeToAdd){
    var str = String(timeToAdd).length;
    if(str === 1){   
        return "0"+timeToAdd;
    }else{
        return timeToAdd;
    }
}

const currentTime= ()=>{
   currentTimeElement.innerHTML = timeConverter(myVideo.currentTime);
   durationTimeElement.innerHTML = timeConverter(myVideo.duration);
}

myVideo.addEventListener('timeupdate', currentTime);



function setTransform(){
    document.getElementById("controls").style.transform = " translateY(0)";
    document.getElementById("controls").style.transition= " all 0.2s ";
}

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

videoContainer.onfullscreenchange = ()=>{
    console.log("cambiato");
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
    console.log("cambiato");
    if(isFullScreen){
        videoContainer.classList.add("hover")
        myVideo.classList.remove("video-fullscreen");
    }else{
        videoContainer.classList.remove("hover")
        myVideo.classList.add("video-fullscreen");
    }
    isFullScreen = !isFullScreen
};
