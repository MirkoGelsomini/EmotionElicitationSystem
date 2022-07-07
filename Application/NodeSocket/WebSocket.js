const WebSocket = require('ws');
const fs = require('fs');
const wss = new WebSocket.Server({port: 7075});
let allScenesJSON = JSON.parse(fs.readFileSync(require('path').resolve(__dirname, '.')+"/movieData.json"));
let chosedScenesJSON = JSON.parse(fs.readFileSync(require('path').resolve(__dirname, '.')+"/chosedScenes.json"));
wss.on('connection', (ws)=>{
    console.log("new client connected")
    /**
     * [messageCode])(opt)[JSON]
     * messageCode:
     *       1: retrive all movie scenes json
     *       2: retrive choosed movie scenes
     *       3: save choosed movie scenes
     *       4: save form result
     */
    ws.on('message',(messageAsString)=>{
        let splits = String(messageAsString).split(")");
        let messageCode = splits[0];
        switch(messageCode){
            case "1":
                ws.send(JSON.stringify(allScenesJSON));
                break;
            case "2":
                ws.send(JSON.stringify(chosedScenesJSON));
                break;
            case "3":
                fs.writeFileSync(require('path').resolve(__dirname, '.')+"/chosedScenes.json",splits[1]);
                chosedScenesJSON = JSON.parse(fs.readFileSync(require('path').resolve(__dirname, '.')+"/chosedScenes.json"));
                ws.send("Scene salvate")
                break;
            case "4":
                break;
            default:
                ws.send("Message code not correct")
        }
        if(splits[0]==="ciao"){
            ws.send("hello");
        }else if(splits[0]==="hello"){
            ws.send("ciao");
        }

        /*const message = JSON.parse(messageAsString);
        const metadata = clients.get(ws);
        message.sender = metadata.id;
        message.color = metadata.color;
        const outbound = JSON.stringify(message);
        [...clients.keys()].forEach((client) =>{
            client.send(outbound);
        });*/
   });

    ws.on("close", () => {
        console.log("client disconnect")
    })
});

