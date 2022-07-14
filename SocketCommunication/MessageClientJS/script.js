var start = function () {
    var inc = document.getElementById('incoming');
    var reqpub_button = document.getElementById('reqpub_button');
    var reqsub_button = document.getElementById('reqsub_button');
    var pub_button = document.getElementById('pub_button');
    var pubx1000_button = document.getElementById('pubx1000_button');
    var reqpub_topic = document.getElementById('reqpub_topic');
    var reqsub_topic = document.getElementById('reqsub_topic');
    var pub_topic = document.getElementById('pub_topic');
    var commands_button = document.getElementById('commands_button');
    inc.innerHTML += "connecting to server ..<br/>";	
		
		
    window.wsm = InitializeWebSocket("ws://127.0.0.1:50000/VCOCKPIT", onAuthorized, onMessage, onClose);		

    reqsub_button.addEventListener('click', function (e) {
        e.preventDefault();
        wsm.SubscribeTo(reqsub_topic.value);
    });

    pub_button.addEventListener('click', function (e) {
        e.preventDefault();
        wsm.Publish(pub_topic.value, pub_content.value);
    });
	
    pubx1000_button.addEventListener('click', function (e) {
        e.preventDefault();
        
        for(var i=0;i<1000;i++){
            var content = (i+1)+"/1000 - "+pub_content.value;            
            wsm.Publish(pub_topic.value, content);
        }
        
    });

    function onAuthorized(){
        inc.innerHTML += '.. connection open<br/>';
    }
    
    function onMessage(message){
        inc.innerHTML += message + '<br/>';
    }
    
    function onClose(){
        inc.innerHTML += '.. connection closed<br/>';
    }
    
}

function Message() {
    var action = null;
    var topic = null;
    var content = null;
}

window.onload = start;