var bitalinoReady = false;
var start = function (){
    window.wsm = InitializeWebSocket("ws://127.0.0.1:50000/VCOCKPIT", onAuthorized, onMessage, onClose);		
    function onAuthorized(){
        console.log("VCopkit Ready");
        subscribeToAllTopics();
    }

    function subscribeToAllTopics(){
        wsm.SubscribeTo("Bitalino: StartSampling");
        wsm.SubscribeTo("Bitalino: StopSampling");
        wsm.SubscribeTo("Bitalino: RestartSampling");
        wsm.SubscribeTo("Bitalino: NewSampling");
        wsm.SubscribeTo("Bitalino: FinishSampling");
        wsm.SubscribeTo("Bitalino: StateUpdateRequest");
        wsm.SubscribeTo("Bitalino: StateUpdateAnswer");
        wsm.SubscribeTo("Bitalino: SaveSampling");
        wsm.SubscribeTo("Bitalino: DeleteSampling");
    }

    function onMessage(message){
        if(message["topic"] === "Bitalino: StateUpdateAnswer"){
            if(message["content"] == "OK"){
                bitalinoReady = true;
                document.getElementById("bitalino-ready").classList.remove("form-check-danger");
                document.getElementById("bitalino-ready").classList.add("form-check-success");
                document.getElementById("label-bitalino-ready").innerHTML = "Bitalino pronto";
            }
        }
    }
    
    function onClose(){
        console.log('.. connection closed');
    }
    
}

function Message() {
    var action = null;
    var topic = null;
    var content = null;
}

